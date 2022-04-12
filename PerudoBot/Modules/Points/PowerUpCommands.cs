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
        [Command("use swap")]
        public async Task Swap(SocketUser swapWith)
        {
            if (swapWith == null) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            if (game == null) return;
            var currentPlayer = game.GetCurrentPlayer();
            var userId = GetUserId(currentPlayer);

            if (Context.User.Id != userId) return;

            var swapWithPlayerId = GetPlayerId(swapWith.Id, Context.Guild.Id);

            if (!game.HasPlayerWithDice(swapWithPlayerId)) ;

            // TODO: Refactor this to use points?
            if (AbleToUsePowerUp(game, currentPlayer.PlayerId))
            {
                var numUsed = GetPowerUpsUsed(game, currentPlayer.PlayerId);
                SetPowerUpsUsed(game, currentPlayer.PlayerId, numUsed + 1);
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
