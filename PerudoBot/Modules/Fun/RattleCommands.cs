﻿using Discord;
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
            await SendMessageAsync($"Set rattles with `!deathrattle giflink` and `!winrattle giflink` (I've PM'd you yours)");

            var player = _db.Players
                .Include(x => x.DiscordPlayer)
                .Include(x => x.Metadata)
                .AsQueryable()
                .First(x => x.DiscordPlayer.UserId == Context.User.Id);

            var deathrattle = player.GetMetadata("deathrattle");
            var winrattle = player.GetMetadata("winrattle");

            var message = $"**Your rattles**\ndeathrattle:\n{deathrattle}\n\nwinrattle:\n{winrattle}";
            var requestOptions = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit };
            await Context.User.SendMessageAsync(message, options: requestOptions);

        }

        [Command("deathrattle")]
        public async Task DeathRattle(params string[] rattle)
        {
            var players = _db.Players
                .Include(x => x.DiscordPlayer)
                .Include(x => x.Metadata)
                .AsQueryable()
                .Where(x => x.DiscordPlayer.UserId == Context.User.Id);

            foreach (var player in players)
            {
                player.SetMetadata("deathrattle", string.Join(" ", rattle));
            }

            await SendMessageAsync($"Death rattle set.");
        }
        [Command("winrattle")]
        public async Task WinRattle(params string[] rattle)
        {
            var players = _db.Players
                .Include(x => x.DiscordPlayer)
                .Include(x => x.Metadata)
                .AsQueryable()
                .Where(x => x.DiscordPlayer.UserId == Context.User.Id);

            foreach (var player in players)
            {
                player.SetMetadata("winrattle", string.Join(" ", rattle));
            }

            await SendMessageAsync($"Win rattle set.");
        }
    }
}