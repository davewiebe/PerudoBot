using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using PerudoBot.GameService.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private Random _random = new Random();

        [Command("recombobulate")]
        [Alias("recomb")]
        public async Task Recombobulate()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Recombobulate)) return;

            var removedDice = game.RemoveRandomDice(powerUpPlayerId, powerUpPlayer.Dice.Count);
            var otherPlayers = game.GetAllPlayers()
                .Where(x => x.NumberOfDice > 0)
                .Where(x => x.PlayerId != powerUpPlayerId)
                .ToList();

            for (int i = 0; i < otherPlayers.Count; i++)
            {
                var playerToGiveDiceTo = otherPlayers[i];
                game.AddDice(playerToGiveDiceTo.PlayerId, new List<int> { removedDice[i % removedDice.Count] }, isMystery: true);
            }
            game.AddRandomDice(powerUpPlayerId, removedDice.Count);

            await SendMessageAsync($":zap: {powerUpPlayer.Name} recombobulated their hand");

            var playersToUpdate = game.GetAllPlayers()
                .Where(x => x.NumberOfDice > 0)
                .ToList();

            await SendOutDice(playersToUpdate, isUpdate: true);
        }

        [Command("charm")]
        public async Task Charm()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Charm)) return;

            var playersWithDice = game.GetAllPlayers()
                .Where(x => x.PlayerId != powerUpPlayerId)
                .Where(x => x.NumberOfDice > 0)
                .ToList();

            var charmedPlayerId = playersWithDice[_random.Next(0, playersWithDice.Count)].PlayerId;
            var powerUpPlayerDice = new List<int>(powerUpPlayer.Dice);

            int numToGive = _random.Next(1, 3);
            numToGive = Math.Min(numToGive, powerUpPlayer.Dice.Count);

            var diceToGive = new List<int>();
            for (int i = 0; i < numToGive; i++)
            {
                var randomDieIndex = _random.Next(0, powerUpPlayerDice.Count);
                diceToGive.Add(powerUpPlayerDice[randomDieIndex]);
                powerUpPlayerDice.RemoveAt(randomDieIndex);
            }

            game.AddDice(charmedPlayerId, diceToGive, isMystery: true);
            var charmedPlayer = game.GetPlayer(charmedPlayerId);

            await SendMessageAsync($":zap: {powerUpPlayer.Name} charmed {charmedPlayer.Name} with {numToGive} of their dice");
 
            var playersToUpates = new List<PlayerData> { charmedPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
        }


        [Command("promote")]
        public async Task Promote()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Promote)) return;

            var numToReroll = (int)Math.Ceiling(powerUpPlayer.Dice.Count / 2.0);
            var numRevealed = numToReroll / 2;

            game.RemoveDiceFromStart(powerUpPlayerId, numToReroll);

            game.AddRandomDice(powerUpPlayerId, numRevealed);
            game.AddRandomDice(powerUpPlayerId, numToReroll - numRevealed, isMystery: true);

            await SendMessageAsync($":zap: {powerUpPlayer.Name} promoted {numToReroll} of their dice");

            powerUpPlayer = game.GetPlayer(powerUpPlayerId);
            var playersToUpates = new List<PlayerData> { powerUpPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
        }

        [Command("steal")]
        public async Task Steal(SocketUser stealFrom)
        {
            if (stealFrom == null) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            var stealFromPlayerId = GetPlayerId(stealFrom.Id, Context.Guild.Id);

            if (!game.HasPlayerWithDice(stealFromPlayerId) || stealFrom.IsBot)
            {
                await SendMessageAsync($"Steal target must be a human player with dice");
                return;
            }

            var previousBid = game.GetPreviousBid();

            if (previousBid?.GamePlayer.PlayerId == stealFromPlayerId)
            {
                await SendMessageAsync($"You can't steal from a player that just bid");
                return;
            }

            var stealFromPlayer = game.GetPlayer(stealFromPlayerId);
            var numberToSteal = _random.Next(2, 4);
            numberToSteal = Math.Min(numberToSteal, stealFromPlayer.Dice.Count - 1);

            if (numberToSteal < 1)
            {
                await SendMessageAsync($"Steal target must be a player with more than one dice");
                return;
            }

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Steal)) return;

            var stolenDice = game.RemoveRandomDice(stealFromPlayerId, numberToSteal);
            game.AddDice(powerUpPlayerId, stolenDice, isMystery: true);

            await SendMessageAsync($":zap: {powerUpPlayer.Name} stole {numberToSteal} mystery dice from {stealFromPlayer.Name}");

            stealFromPlayer = game.GetPlayer(stealFromPlayerId);
            powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { powerUpPlayer, stealFromPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
        }

        [Command("gamble")]
        public async Task Gamble()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Gamble)) return;

            var chanceRoll = _random.Next(0, 100);

            if (chanceRoll < 1)
            {
                game.AddDice(powerUpPlayerId, new List<int> { 1, 1, 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} gets three special dice");
            }
            else if (chanceRoll < 4)
            {
                game.AddDice(powerUpPlayerId, new List<int> { 1, 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} gets two special dice");
            }
            else if (chanceRoll < 10)
            {
                game.AddDice(powerUpPlayerId, new List<int> { 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} gets a special dice");
            }
            else if (chanceRoll < 25)
            {
                game.RemoveRandomDice(powerUpPlayerId, 1);
                game.AddRandomDice(powerUpPlayerId, 1, isMystery: true);
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded one of his dice for a special die");
            }
            else if (chanceRoll < 40)
            {
                game.RemoveRandomDice(powerUpPlayerId, 2);
                game.AddRandomDice(powerUpPlayerId, 2, isMystery: true);
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded two of his dice for two special dice");
            }
            else if (chanceRoll < 55)
            {
                game.RemoveRandomDice(powerUpPlayerId, 2);
                game.AddRandomDice(powerUpPlayerId, 1, isMystery: true);
                game.AddDice(powerUpPlayerId, new List<int> { 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded two of his dice for two special dice");
            }
            else if (chanceRoll < 70)
            {
                game.RemoveRandomDice(powerUpPlayerId, 3);
                game.AddRandomDice(powerUpPlayerId, 2, isMystery: true);
                game.AddDice(powerUpPlayerId, new List<int> { 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded three of his dice for three special dice");
            }
            else if (chanceRoll < 85)
            {
                game.RemoveRandomDice(powerUpPlayerId, 3);
                game.AddRandomDice(powerUpPlayerId, 1, isMystery: true);
                game.AddDice(powerUpPlayerId, new List<int> { 1, 1 });
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded three of his dice for three special dice");
            }
            else
            {
                game.RemoveRandomDice(powerUpPlayerId, 3);
                game.AddRandomDice(powerUpPlayerId, 3, isMystery: true);
                await SendMessageAsync($":zap: {powerUpPlayer.Name} traded three of his dice for three special dice");
            }

            powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { powerUpPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
        }

        [Command("claim")]
        public async Task Claim()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (GetAvailablePoints(powerUpPlayerId) > PowerUps.CLAIM_THRESHOLD)
            {
                await SendMessageAsync($"You must have less than {PowerUps.CLAIM_THRESHOLD} available points to use claim.");
                return;
            }

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Claim)) return;

            AddTotalPoints(powerUpPlayerId, PowerUps.CLAIM_AMOUNT);

            var updatedPoints = GetAvailablePoints(powerUpPlayerId);
            await SendMessageAsync($":zap: {powerUpPlayer.Name} claims `{PowerUps.CLAIM_AMOUNT}` points. " +
                $"{powerUpPlayer.Name} now has `{updatedPoints}` points.");
        }

        [Command("lifetap")]
        public async Task Lifetap()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            var playerDiceCount = powerUpPlayer.NumberOfDice;

            if (playerDiceCount >= 5 && game.GetMode() == GameMode.Reverse) return;
            if (playerDiceCount <= 1 && game.GetMode() != GameMode.Reverse) return;

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Lifetap)) return;


            if (game.GetMode() == GameMode.Reverse)
            {
                game.SetPlayerDice(powerUpPlayerId, playerDiceCount + 1);
                AddTotalPoints(powerUpPlayerId, PowerUps.LIFETAP_AMOUNT);
            }
            else
            {
                game.SetPlayerDice(powerUpPlayerId, playerDiceCount - 1);
                AddTotalPoints(powerUpPlayerId, PowerUps.LIFETAP_AMOUNT);
            }

            var updatedPoints = GetAvailablePoints(powerUpPlayerId);

            await SendMessageAsync($":zap: {powerUpPlayer.Name} loses a life to get `{PowerUps.LIFETAP_AMOUNT}` points. " +
                $"{powerUpPlayer.Name} now has `{updatedPoints}` points.");
        }

        [Command("powerups")]
        public async Task PowerUpInfo()
        {
            var builder = new EmbedBuilder().WithTitle($"Power Up Information");

            foreach (var powerUp in PowerUps.PowerUpList)
            {
                builder.AddField($":zap: {powerUp.Name} - `{powerUp.Cost}` pts", $"{powerUp.Description}");
            }

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        private async Task<bool> AbleToUsePowerUp(GameObject game, PlayerData player, PowerUp powerUp)
        {
            var activePlayers = game.GetAllPlayers().Where(x => x.NumberOfDice > 0).Count();

            if (activePlayers < powerUp.MinPlayers)
            {
                await SendMessageAsync($"You can only use :zap: {powerUp.Name} with {powerUp.MinPlayers} or more players remaining");
                return false;
            }

            var userId = GetUserId(player);
            if (!powerUp.OutOfTurn && Context.User.Id != userId)
            {
                await SendMessageAsync($"You can only use :zap: {powerUp.Name} on your own turn");
                return false;
            }

            if (player.Dice.Count < powerUp.MinDice)
            {
                await SendMessageAsync($"You can only use :zap: {powerUp.Name} with {powerUp.MinDice} or more dice");
                return false;
            }

            if (GetAvailablePoints(player.PlayerId) < powerUp.Cost)
            {
                await SendMessageAsync($"You don't have enough points to use :zap: {powerUp.Name}");
                return false;
            }

            if (GetPowerUpUses(game, player.PlayerId, powerUp) >= powerUp.UsesPerGame)
            {
                await SendMessageAsync($"You have reached use limit for :zap: {powerUp.Name} this game.");
                return false;
            }

            if (GetPowerUpUses(game, player.PlayerId, powerUp, perRound: true) >= powerUp.UsesPerRound)
            {
                await SendMessageAsync($"You have reached use limit for :zap: {powerUp.Name} this round.");
                return false;
            }

            if (powerUp.Cost > 0)
            {
                AddUsedPoints(player.PlayerId, powerUp.Cost);
                await SendMessageAsync($":grey_exclamation: {player.Name} spent `{powerUp.Cost}` points to use `{powerUp.Name}`.");
            }

            AddPowerUpUses(game, player.PlayerId, powerUp, 1, perRound: true);
            AddPowerUpUses(game, player.PlayerId, powerUp, 1);
            DeleteCommandFromDiscord();

            Thread.Sleep(2000);

            return true;
        }

        private int GetPowerUpUses(GameObject game, int playerId, PowerUp powerUp, bool perRound = false)
        {
            var metaDataKey = perRound ? 
                $"{playerId}-{game.GetCurrentRound().RoundNumber}-{powerUp.Name}-uses": 
                $"{playerId}-{powerUp.Name}-uses";

            var numUsedString = game.GetMetadata(metaDataKey);

            if (string.IsNullOrEmpty(numUsedString))
            {
                game.SetMetadata(metaDataKey, "0");
                return 0;
            }

            return int.Parse(numUsedString);
        }

        private void AddPowerUpUses(GameObject game, int playerId, PowerUp powerUp, int value, bool perRound = false)
        {
            var metaDataKey = perRound ?
                $"{playerId}-{game.GetCurrentRound().RoundNumber}-{powerUp.Name}-uses" :
                $"{playerId}-{powerUp.Name}-uses";

            var currentyPowerUpUses = GetPowerUpUses(game, playerId, powerUp);
            game.SetMetadata(metaDataKey, (currentyPowerUpUses + value).ToString());
        }
    }
}
