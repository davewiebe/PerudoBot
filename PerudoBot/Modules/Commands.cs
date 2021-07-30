using Discord;
using Discord.Commands;
using System.Threading.Tasks;
using System;
using PerudoBot.Database.Data;
using PerudoBot.GameService;
using System.IO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        private readonly IMemoryCache _cache;
        private readonly PerudoBotDbContext _db;
        //private readonly GameObject _gameObject;
        private readonly GameHandler _gameHandler;

        public Commands(IServiceProvider serviceProvider)
        {
            _cache = serviceProvider.GetRequiredService<IMemoryCache>();
            _db = serviceProvider.GetRequiredService<PerudoBotDbContext>();

            _gameHandler = new GameHandler(_db, _cache);
        }
        private async Task<IUserMessage> SendMessageAsync(string message, bool isTTS = false)
        {
            if (string.IsNullOrEmpty(message)) return null;

            var requestOptions = new RequestOptions()
            { RetryMode = RetryMode.RetryRatelimit };
            return await base.ReplyAsync(message, options: requestOptions, isTTS: isTTS);
        }

        private void SetGuildAndChannel()
        {
            _gameHandler.SetChannel(Context.Channel.Id);
            _gameHandler.SetGuild(Context.Guild.Id);
        }

        private ulong GetUserId(PlayerData currentPlayer)
        {
            return _db.Players
                .Single(x => x.Id == currentPlayer.PlayerId)
                .DiscordPlayer.UserId;
        }
        private int GetPlayerId(ulong userId)
        {
            return _db.Players
                .Single(x => x.DiscordPlayer.UserId == userId)
                .Id;
        }

        [Command("updateavatar")]
        public async Task UpdateAvatarCommand(params string[] options)
        {
            //Is this causing hang-ups? Uncomment when bot it stable
            //await UpdateAvatar($"{options.First()}.png");
        }

        private async Task UpdateAvatar(string avatarName)
        {
            //try
            //{
            //    var fileStream = new FileStream(Directory.GetCurrentDirectory() + $"/Avatars/{avatarName}", FileMode.Open);
            //    var image = new Image(fileStream);
            //    await Context.Client.CurrentUser.ModifyAsync(u => u.Avatar = image);
            //}
            //catch { }
        }

        [Command("ping")]
        [Alias("p")]
        public async Task Ping()
        {
            await SendMessageAsync("Pong");
        }


        [Command("set")]
        public async Task Set(string param)
        {
            _cache.Set("test", param); // cache testing
        }

        [Command("get")]
        public async Task Get()
        {
            await SendMessageAsync((string)_cache.Get("test")); // cache testing
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