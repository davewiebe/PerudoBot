using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("rattles")]
        public async Task Rattles(params string[] rattle)
        {
            await SendMessageAsync($"Set rattles with `!deathrattle giflink` and `!winrattle giflink`");
        }

        [Command("deathrattle")]
        public async Task DeathRattle(params string[] rattle)
        {
            var player = _db.Players
                .Include(x => x.DiscordPlayer)
                .Include(x => x.Metadata)
                .AsQueryable().Single(x => x.DiscordPlayer.UserId == Context.User.Id);

            player.SetMetadata("deathrattle", string.Join(" ", rattle));

            await SendMessageAsync($"Death rattle set.");
        }
        [Command("winrattle")]
        public async Task WinRattle(params string[] rattle)
        {
            var player = _db.Players
                .Include(x => x.DiscordPlayer)
                .Include(x => x.Metadata)
                .AsQueryable()
                .Where(x => x.DiscordPlayer.GuildId == Context.Guild.Id)
                .Single(x => x.DiscordPlayer.UserId == Context.User.Id);

            player.SetMetadata("winrattle", string.Join(" ", rattle));

            await SendMessageAsync($"Win rattle set.");
        }

    }
}