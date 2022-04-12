using Discord.Commands;
using Discord.WebSocket;
using PerudoBot.GameService;
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

            if (game.GetAllPlayers().Count < 4)
            {
                await SendMessageAsync($"You can only use swap with four or more players remaining");
                return;
            }

            var currentPlayer = game.GetCurrentPlayer();
            var userId = GetUserId(currentPlayer);

            if (Context.User.Id != userId)
            {
                await SendMessageAsync($"You can only use swap on your own turn");
                return;
            }

            var swapWithPlayerId = GetPlayerId(swapWith.Id, Context.Guild.Id);

            if (!game.HasPlayerWithDice(swapWithPlayerId)) return;

            // TODO: Refactor this to use points?
            if (AbleToUsePowerUp(game, currentPlayer.PlayerId))
            {
                var numUsed = GetPowerUpsUsed(game, currentPlayer.PlayerId);
                SetPowerUpsUsed(game, currentPlayer.PlayerId, ++numUsed);
                await SendMessageAsync($"{currentPlayer.Name} has {MAX_USES - numUsed}/{MAX_USES} :zap: Power Up uses left");
            }
            else
            {
                await SendMessageAsync($"Power Up limit reached for {currentPlayer.Name}");
                return;
            }

            game.SwapPlayerHands(currentPlayer.PlayerId, swapWithPlayerId);

            var swapWithPlayer = game.GetPlayer(swapWithPlayerId);
            currentPlayer = game.GetPlayer(currentPlayer.PlayerId);

            var playersToUpates = new List<PlayerData> { currentPlayer, swapWithPlayer };
            await SendOutDice(playersToUpates);

            await SendMessageAsync($":zap: Swapped hands of players {currentPlayer.Name} and {swapWithPlayer.Name} :zap:");
        }

        [Command("gamble")]
        public async Task Gamble()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            if (game == null) return;

            if (game.GetAllPlayers().Count < 3)
            {
                await SendMessageAsync($"You can only use swap with three or more players remaining");
                return;
            }

            var currentPlayer = game.GetCurrentPlayer();
            var userId = GetUserId(currentPlayer);

            if (Context.User.Id != userId)
            {
                await SendMessageAsync($"You can only use gamble on your own turn");
                return;
            }

            // TODO: Refactor this to use points?
            if (AbleToUsePowerUp(game, currentPlayer.PlayerId))
            {
                var numUsed = GetPowerUpsUsed(game, currentPlayer.PlayerId);
                SetPowerUpsUsed(game, currentPlayer.PlayerId, ++numUsed);
                await SendMessageAsync($"{currentPlayer.Name} has {MAX_USES - numUsed}/{MAX_USES} :zap: Power Up uses left");
            }
            else
            {
                await SendMessageAsync($"Power Up limit reached for {currentPlayer.Name}");
                return;
            }

            var minToGet = (-currentPlayer.Dice.Count) + 1;
            var maxToGet = currentPlayer.Dice.Count + 2;
            maxToGet = Math.Min(maxToGet, 4);

            var diceToGet = RAND.Next(minToGet, maxToGet);
            game.GrantDice(currentPlayer.PlayerId, diceToGet);

            if (diceToGet == 0)
            {
                await SendMessageAsync($":zap: {currentPlayer.Name} used gamble but nothing happened :zap:");
            }
            else
            {
                var gainsLoses = diceToGet > 0 ? "gains" : "loses";
                await SendMessageAsync($":zap: {currentPlayer.Name} {gainsLoses} {Math.Abs(diceToGet)} dice :zap:");

                currentPlayer = game.GetCurrentPlayer();
                var playersToUpates = new List<PlayerData> { currentPlayer };
                await SendOutDice(playersToUpates);
            }
        }

        private bool AbleToUsePowerUp(GameObject game, int playerId)
        {
            return GetPowerUpsUsed(game, playerId) < 6;
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
