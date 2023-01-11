using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("resenddice")]
        public async Task ResendDice()
        {
            _gameHandler.SetChannel(Context.Channel.Id);
            var game = _gameHandler.GetActiveGame();
            var playerDice = game.GetAllPlayers();
            await SendOutDice(playerDice);
        }

        [Command("resetround")]
        public async Task ResetRound()
        {
            SetGuildAndChannel();
            var gameObject = _gameHandler.GetActiveGame();

            if (gameObject != null)
            {
                await SendRoundSummary();
                await StartNewRound(gameObject);
            }
            else
            {
                await SendMessageAsync("No active game");
            }
        }

        [Command("status")]
        public async Task Status()
        {
            SetGuildAndChannel();
            var gameObject = _gameHandler.GetActiveGame();
            if (gameObject != null)
            {
                var roundStatus = gameObject.GetCurrentRoundStatus();
                await SendCurrentRoundStatus(roundStatus);
            }
            else
            {
                await SendMessageAsync("No active game");
            }
        }
    }


}