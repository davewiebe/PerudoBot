using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class DiscordServices : ModuleBase<SocketCommandContext>
    {
        private SocketCommandContext _context;

        public DiscordServices(SocketCommandContext context)
        {
            _context = context;
        }

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
                    _ = Task.Run(() => _context.Channel.DeleteMessageAsync(messageId.Value));
                }
                else
                {
                    _ = Task.Run(() => _context.Message.DeleteAsync());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
