using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("getmeta")]
        public async Task GetMeta(params string[] options)
        {
            var player = _db.Players.AsQueryable().Single(x => x.DiscordPlayer.UserId == Context.User.Id);

            var metadata = player.GetMetadata("test");

            await SendMessageAsync($"Retrieved metadata: {metadata}");
        }
        [Command("setmeta")]
        public async Task SetMeta(params string[] options)
        {
            var player = _db.Players
                .AsQueryable()
                .Include(x => x.Metadata)
                .Single(x => x.DiscordPlayer.UserId == Context.User.Id);

            player.SetMetadata("test", string.Join(" ", options));
            _db.SaveChanges();

            await SendMessageAsync("Metadata saved.");
        }
    }
}