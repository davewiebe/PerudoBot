using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using PerudoBot.GameService.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{

    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private const int MAX_USES = 6;
        private Random RAND = new Random();

        [Command("swap")]
        public async Task Swap(SocketUser swapWith)
        {
            if (swapWith == null) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Swap)) DeleteCommandFromDiscord();
            else return;

            var swapWithPlayerId = GetPlayerId(swapWith.Id, Context.Guild.Id);

            if (!game.HasPlayerWithDice(swapWithPlayerId))
            {
                await SendMessageAsync($"Swap target must be a player with dice");
                return;
            }

            game.SwapPlayerHands(powerUpPlayer.PlayerId, swapWithPlayerId);

            var swapWithPlayer = game.GetPlayer(swapWithPlayerId);
            powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { powerUpPlayer, swapWithPlayer };
            await SendOutDice(playersToUpates);

            await SendMessageAsync($":zap: Swapped hands of players {powerUpPlayer.Name} and {swapWithPlayer.Name} :zap:");
        }

        [Command("gamble")]
        public async Task Gamble()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Gamble)) DeleteCommandFromDiscord();
            else return;

            var minToGet = (-powerUpPlayer.Dice.Count) + 1;
            var maxToGet = powerUpPlayer.Dice.Count + 2;
            maxToGet = Math.Min(maxToGet, 4);

            var diceToGet = RAND.Next(minToGet, maxToGet);
            game.GrantDice(powerUpPlayer.PlayerId, diceToGet);

            if (diceToGet == 0)
            {
                await SendMessageAsync($":zap: {powerUpPlayer.Name} used gamble but nothing happened :zap:");
            }
            else
            {
                var gainsLoses = diceToGet > 0 ? "gains" : "loses";
                await SendMessageAsync($":zap: {powerUpPlayer.Name} {gainsLoses} {Math.Abs(diceToGet)} dice :zap:");

                powerUpPlayer = game.GetPlayer(powerUpPlayer.PlayerId);
                var playersToUpates = new List<PlayerData> { powerUpPlayer };
                await SendOutDice(playersToUpates);
            }
        }

        [Command("claim")]
        public async Task Claim(params string[] bidText)
        {
            if (bidText == null || bidText.Length < 2) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            if (await AbleToUsePowerUp(game, powerUpPlayer, PowerUps.Claim)) DeleteCommandFromDiscord();
            else return;

            if (!int.TryParse(bidText[0], out int claimedCount)) return;
            if (!int.TryParse(bidText[1], out int face)) return;

            var actualCount = powerUpPlayer.Dice.Count(x => x == face);

            if (actualCount != claimedCount)
            {
                await SendMessageAsync($":no_entry: Unable to verify {powerUpPlayer.Name}'s claim of `{claimedCount}` ˣ {face.ToEmoji()}");
            }
            else
            {
                await SendMessageAsync($":white_check_mark: Verified {powerUpPlayer.Name}'s claim of `{claimedCount}` ˣ {face.ToEmoji()}");
            }
        }

        [Command("uses")]
        public async Task Uses()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var powerUpPlayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var powerUpPlayer = game.GetPlayer(powerUpPlayerId);

            var numUsed = GetPowerUpsUsed(game, powerUpPlayer.PlayerId);
            await SendMessageAsync($"{powerUpPlayer.Name} has {MAX_USES - numUsed}/{MAX_USES} :zap: Power Up uses left");
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
                await SendMessageAsync($"You can only use gamble on your own turn");
                return false;
            }

            if (GetPowerUpsUsed(game, player.PlayerId) < MAX_USES)
            {
                var numUsed = GetPowerUpsUsed(game, player.PlayerId);
                SetPowerUpsUsed(game, player.PlayerId, ++numUsed);
                
                //await SendMessageAsync($"{player.Name} has {MAX_USES - numUsed}/{MAX_USES} :zap: Power Up uses left");
            }
            else
            {
                await SendMessageAsync($"Power Up use limit reached for {player.Name}. Keep track of uses with `!uses`.");
                return false;
            }

            return true;
        }

        private int GetPowerUpsUsed(GameObject game, int playerId)
        {
            var metaDataKey = $"{playerId}-powerups-used";
            var numUsedString = game.GetMetadata(metaDataKey);

            if (string.IsNullOrEmpty(numUsedString))
            {
                game.SetMetadata(metaDataKey, "0");
                return 0;
            }

            return int.Parse(numUsedString);
        }

        private void SetPowerUpsUsed(GameObject game, int playerId, int value)
        {
            var metaDataKey = $"{playerId}-powerups-used";
            game.SetMetadata(metaDataKey, value.ToString());
        }

    }
}
