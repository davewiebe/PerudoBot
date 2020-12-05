using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using PerudoBot.Database.Sqlite.Data;
using PerudoBot.GameService;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly PerudoBotDbContext _db;
        private readonly GameHandler _gameHandler;

        public Commands()
        {
            //TODO: Let DI handle instantiation
            _db = new PerudoBotDbContext();
            _gameHandler = new GameHandler(_db);
        }


        [Command("ping")]
        [Alias("p")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }
    }
}