using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Database.Data;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using PerudoBot.GameService.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private Random _random = new Random();

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
                await SendMessageAsync($"Steal target must be a human player with dice.");
                return;
            }

            var previousBid = game.GetPreviousBid();

            if (previousBid?.GamePlayer.PlayerId == stealFromPlayerId)
            {
                await SendMessageAsync($"You can't steal from a player that just bid.");
                return;
            }

            var stealFromPlayer = game.GetPlayer(stealFromPlayerId);
            var numberToSteal = Math.Min(3, stealFromPlayer.Dice.Count - 1);

            if (numberToSteal < 1)
            {
                await SendMessageAsync($"Steal target must be a player with more than one dice.");
                return;
            }

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Steal)) return;

            var stolenDice = game.RemoveRandomDice(stealFromPlayerId, numberToSteal);
            game.AddDice(powerUpPlayerId, stolenDice, isMystery: true);

            await SendMessageAsync($":zap: **Steal**: {powerUpPlayer.Name} takes {numberToSteal} mystery dice from {stealFromPlayer.Name}.");

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

            var playerDiceCount = powerUpPlayer.Dice.Count;
            var chanceRoll = _random.Next(0, 100);

            if (chanceRoll < 5)
            {
                game.RemoveRandomDice(powerUpPlayerId, playerDiceCount);
                for (int i = 0; i < playerDiceCount; i++) game.AddDice(powerUpPlayerId, new List<int> { 1 });
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} rerolled their dice.");
            }
            else if (chanceRoll < 10)
            {
                game.AddRandomDice(powerUpPlayerId, 2);
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} gained 2 new dice.");
            }
            else if (chanceRoll < 20)
            {
                game.AddRandomDice(powerUpPlayerId, 2, isMystery: true);
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} gained 2 new dice.");
            }
            else if (chanceRoll < 35)
            {
                game.AddRandomDice(powerUpPlayerId, 1);
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} gained 1 new die.");
            }
            else if (chanceRoll < 55)
            {
                game.AddRandomDice(powerUpPlayerId, 1, isMystery: true);
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} gained 1 new die.");
            }
            else
            {
                game.RemoveRandomDice(powerUpPlayerId, playerDiceCount);
                game.AddRandomDice(powerUpPlayerId, playerDiceCount);
                await SendMessageAsync($":zap: **Gamble**: {powerUpPlayer.Name} rerolled their dice.");
            }

            powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { powerUpPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
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
            if (playerDiceCount != 5 && game.GetMode() != GameMode.Reverse) return;

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Lifetap)) return;

            game.AddRandomDice(powerUpPlayerId, 3);

            if (game.GetMode() == GameMode.Reverse)
            {
                game.SetPlayerDice(powerUpPlayerId, playerDiceCount + 1);
                await SendMessageAsync($":zap: **Lifetap**: {powerUpPlayer.Name} loses a life to get 3 random dice this round.");
            }
            else
            {
                game.SetPlayerDice(powerUpPlayerId, 1);
                await SendMessageAsync($":zap: **Lifetap**: {powerUpPlayer.Name} loses all their lives but one to get 3 random dice this round.");
            }

            powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { powerUpPlayer };
            await SendOutDice(playersToUpates, isUpdate: true);
        }

        [Command("touch")]
        public async Task Touch(params string[] bidText)
        {
            if (bidText == null || bidText.Length < 1) return;
            if (!int.TryParse(bidText[0], out int touchPips)) return;
            if (touchPips < 1 || touchPips > 6) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            if (touchPips == 1)
            {
                await SendMessageAsync($"You can't touch ones.");
                return;
            }

            var bids = game.GetCurrentRound().Actions.OfType<Bid>().Select(x => x.Pips).ToList();

            if (bids.Contains(touchPips))
            {
                await SendMessageAsync($"You can't touch a number that was bid this round.");
                return;
            }

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (!await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Touch)) return;

            var allDice = game.GetAllDice();
            var touchedCount = allDice.Where(x => x == touchPips || x == 1).Count();

            var quickMath = allDice.Count / 3.0;
            
            if (touchedCount > quickMath)
            {
                await SendMessageAsync($":zap: **Touch**: {powerUpPlayer.Name} checked {touchPips}s and they felt :fire:");
            }
            else
            {
                await SendMessageAsync($":zap: **Touch**: {powerUpPlayer.Name} checked {touchPips}s and they felt :ice_cube:");
            }
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

        [Command("odds")]
        public async Task GambleOdds()
        {
            var odds = "**Gamble Odds**\n" +
                        "```\n" +
                        "45% Reroll all of your dice \n" +
                        "20% Gain a new mystery die \n" +
                        "15% Gain a new regular die \n" +
                        "10% Gain two new mystery dice \n" +
                        " 5% Change all your dice to 1s \n" +
                        " 5% Gain two new regular dice \n" +
                        "```";

            await SendMessageAsync(odds);
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

            var roundNumber = game.GetCurrentRound().RoundNumber;
  
            var specifcPowerPerGame = $"{powerUp.Name}";
            var anyPowerPerRound = $"{roundNumber}";
            var specificPowerPerRound = $"{roundNumber}-{powerUp.Name}";

            if (GetPowerUpUses(game, player.PlayerId, specifcPowerPerGame) >= powerUp.UsesPerGame)
            {
                await SendMessageAsync($"You have reached use limit for :zap: {powerUp.Name} this game.");
                return false;
            }

            if (GetPowerUpUses(game, player.PlayerId, anyPowerPerRound) >= PowerUps.TOTAL_USES_PER_ROUND)
            {
                await SendMessageAsync($"You have reached power up use limit for this round.");
                return false;
            }

            if (GetPowerUpUses(game, player.PlayerId, specificPowerPerRound) >= powerUp.UsesPerRound)
            {
                await SendMessageAsync($"You have reached use limit for :zap: {powerUp.Name} this round.");
                return false;
            }

            if (powerUp.Cost > 0)
            {
                AddUsedPoints(player.PlayerId, powerUp.Cost);
                //await SendMessageAsync($":grey_exclamation: {player.Name} spent `{powerUp.Cost}` points to use `{powerUp.Name}`.");
            }

            AddPowerUpUses(game, player.PlayerId, specifcPowerPerGame);
            AddPowerUpUses(game, player.PlayerId, anyPowerPerRound);
            AddPowerUpUses(game, player.PlayerId, specificPowerPerRound);

            DeleteCommandFromDiscord();

            return true;
        }

        private int GetPowerUpUses(GameObject game, int playerId, string usesKey)
        {
            var metaDataKey = $"{playerId}-{usesKey}-uses";

            var numUsedString = game.GetMetadata(metaDataKey);

            if (string.IsNullOrEmpty(numUsedString))
            {
                game.SetMetadata(metaDataKey, "0");
                return 0;
            }

            return int.Parse(numUsedString);
        }

        private void AddPowerUpUses(GameObject game, int playerId, string usesKey, int value = 1)
        {
            var metaDataKey = $"{playerId}-{usesKey}-uses";

            var currentyPowerUpUses = GetPowerUpUses(game, playerId, usesKey);
            game.SetMetadata(metaDataKey, (currentyPowerUpUses + value).ToString());
        }
    }
}
