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
        [Command("exact")]
        public async Task Exact(params string[] bidText)
        {
            await SendMessageAsync("Exact not implemented.\nIf you can't call liar and can't go up, time to start bluffing!");
        }
    }
}