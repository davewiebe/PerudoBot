using Discord;
using Discord.Commands;
using PerudoBot.EloService;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("elo")]
        public async Task Elo(params string[] options)
        {
            var gameMode = GameMode.Reverse;

            var suddendeathList = new List<string> { "suddendeath", "sd", "sudden", "death" };
            if (suddendeathList.Contains(options.FirstOrDefault()?.ToLower()))
            {
                gameMode = GameMode.SuddenDeath;
            }

            var reverseList = new List<string> { "variable", "v" };
            if (reverseList.Contains(options.FirstOrDefault()?.ToLower()))
            {
                gameMode = GameMode.Variable;
            }

            var eloHandler = new EloHandler(_db, Context.Guild.Id, gameMode);
            var eloSeason = eloHandler.GetCurrentEloSeason();

            var message = $"`{gameMode}: {eloSeason.SeasonName}`";

            var playerElos = eloSeason.PlayerElos
                .Where(x => x.GameMode == gameMode) // TODO: shouldn't have to do this since we instantiated the elohandler with gamemode. something needs to be fixed.
                .OrderByDescending(x => x.Rating)
                .ToList();

            foreach (var playerElo in playerElos)
            {
                message += $"\n{playerElo.Player.Name}: {playerElo.Rating}";
            }
            await SendMessageAsync(message);
        }

        private async Task CalculateEloAsync(GameObject game)
        {
            var gameMode = game.GetGameMode();
            var eloHandler = new EloHandler(_db, Context.Guild.Id, gameMode);
            var gamePlayers = game.GetAllPlayers()
                .OrderBy(x => x.Rank);

            foreach (var gamePlayer in gamePlayers)
            {
                eloHandler.AddPlayer(gamePlayer.PlayerId, gamePlayer.Rank);
            }
            eloHandler.CalculateAndSaveElo();

            var eloResults = eloHandler.GetEloResults();

            foreach (var gamePlayer in gamePlayers)
            {
                var eloResult = eloResults.Single(x => x.PlayerId == gamePlayer.PlayerId);
                await SendMessageAsync($"`{gamePlayer.Rank}` {gamePlayer.Name} `{eloResult.PreviousElo}` => `{eloResult.Elo}` ({eloResult.Elo - eloResult.PreviousElo})");
                Thread.Sleep(500); // For suspense! 
            }
        }
    }
}