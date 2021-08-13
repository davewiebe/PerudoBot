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
    }
}