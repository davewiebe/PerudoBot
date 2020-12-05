using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using PerudoBot.Database.Sqlite.Data;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands : ModuleBase<CommandContext>
    {
        private readonly PerudoBotDbContext _db;

        public ReactionCommands()
        {
            //TODO: Let DI handle instantiation
            _db = new PerudoBotDbContext();
        }


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
            public async Task Ping(params string[] bidText)
            {
                await ReplyAsync("Pong");
            }



            /* Move the following into a service
             * */
            private async Task<IUserMessage> SendMessageAsync(string message, bool isTTS = false)
            {
                if (string.IsNullOrEmpty(message)) return null;

                var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
                return await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
            }

            private async Task SendTempMessageAsync(string message, bool isTTS = false)
            {
                var requestOptions = new RequestOptions()
                { RetryMode = RetryMode.RetryRatelimit };
                var sentMessage = await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
                try
                {
                    _ = sentMessage.DeleteAsync();
                }
                catch
                { }
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
}