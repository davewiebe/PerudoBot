using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("newseason")]
        [Alias("startnewseason", "startseason")]
        public async Task NewSeason(params string[] seasonNameParam)
        {
            var seasonName = string.Join(" ", seasonNameParam);

            var discordPlayer = _db.DiscordPlayers
                .Include(x => x.Player)
                .Where(x => x.GuildId == Context.Guild.Id)
                .SingleOrDefault(x => x.UserId == Context.User.Id);

            if (!discordPlayer.IsAdministrator) return;

            var newSeason = new EloSeason
            {
                GuildId = Context.Guild.Id,
                SeasonName = seasonName
            };

            _db.EloSeasons.Add(newSeason);
            _db.SaveChanges();

            await SendMessageAsync($"A new ELO season, `{seasonName}`, has begun!");
        }
    }
}