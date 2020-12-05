using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using PerudoBot.Database.Sqlite.Data;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly PerudoBotDbContext _db;

        public Commands()
        {
            //TODO: Let DI handle instantiation
            _db = new PerudoBotDbContext();
        }


        [Command("ping")]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }
    }
}