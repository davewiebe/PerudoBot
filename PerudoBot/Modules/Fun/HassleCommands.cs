using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("bug")]
        public async Task Bug(params string[] options)
        {

            var responses = new List<string>
            {
                "It's not a bug, it's a feature!",
                "I think it's actually Discord that's having issues.",
                "We don't know if that's actually a bug yet...",
                "I think this one just comes down to user error.",
                "Wow. Shots fired.",
                $"I can track who's submitting these. Ready for some garbage hands {Context.User.Username}?."
            };

            await SendMessageAsync(responses.OrderBy(x => Guid.NewGuid()).First());
        }
    }
}