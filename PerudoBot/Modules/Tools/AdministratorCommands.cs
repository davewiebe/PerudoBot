using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("addadmin")]
        public async Task AddAdministrator(params string[] bidText)
        {
            var discordPlayer = _db.DiscordPlayers
                .Include(x => x.Player)
                .SingleOrDefault(x => x.UserId == Context.User.Id);

            if (discordPlayer == null) return;

            var numberOfAdmins = _db.DiscordPlayers
                .AsQueryable()
                .Where(x => x.GuildId == Context.Guild.Id)
                .Count(x => x.IsAdministrator == true);

            if (numberOfAdmins == 0)
            {
                discordPlayer.IsAdministrator = true;
                _db.SaveChanges();
                await SendMessageAsync($"{discordPlayer.Player.Name} has been granted administrator priviledges");
                return;
            }

            if (discordPlayer.IsAdministrator)
            {
                var newAdmin = _db.DiscordPlayers
                    .Include(x => x.Player)
                    .SingleOrDefault(x => x.UserId == Context.Message.MentionedUsers.First().Id);

                if (newAdmin.IsAdministrator)
                {
                    await SendMessageAsync($"{newAdmin.Player.Name} already has administrator priviledges");
                    return;
                }

                newAdmin.IsAdministrator = true;
                _db.SaveChanges();
                await SendMessageAsync($"{newAdmin.Player.Name} has been granted administrator priviledges");
            }
        }

        [Command("admins")]
        public async Task ListAdmins(params string[] text)
        {
            var adminNames = _db.DiscordPlayers
                .AsQueryable()
                .Where(x => x.GuildId == Context.Guild.Id)
                .Where(x => x.IsAdministrator == true)
                .Select(x => x.Player.Name);

            await SendMessageAsync($"Server admins: {string.Join(", ", adminNames)}");
        }

        //[Command("updateplayerdice")]
        //public async Task UpdatePlayerDice(params string[] stringArray)
        //{
        //    //var game = await GetGameAsync(GameState.InProgress);

        //    //var userToAddDiceTo = Context.Message.MentionedUsers.Single();
        //    //var player = _perudoGameService.GetGamePlayers(game).Where(x => x.Player.Username == userToAddDiceTo.Username).Single();

        //    //int monkey = 0;
        //    //if (int.TryParse(stringArray[0], out monkey))
        //    //{
        //    //    player.NumberOfDice = monkey;
        //    //    await SendMessageAsync($"{GetUserNickname(userToAddDiceTo.Username)}'s dice has been updated to {monkey}.");

        //    //    await SendRoundSummaryForBots(game);
        //    //    await SendRoundSummary(game);

        //    //    SetTurnPlayerToRoundStartPlayer(game);
        //    //    Thread.Sleep(2000);
        //    //    await RollDiceStartNewRoundAsync(game);
        //    //    return;
        //    //}
        //}
    }
}