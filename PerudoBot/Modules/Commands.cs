using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using PerudoBot.Database.Data;
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

        [Command("crash")]
        public async Task Crash()
        {
            throw new NullReferenceException("Error message here", new Exception("Inner exception message here"));
        }
        private void DeleteCommandFromDiscord(ulong? messageId = null)
        {
            try
            {
                if (messageId != null)
                {
                    _ = Task.Run(() => Context.Channel.DeleteMessageAsync(messageId.Value));
                }
                else
                {
                    _ = Task.Run(() => Context.Message.DeleteAsync());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}