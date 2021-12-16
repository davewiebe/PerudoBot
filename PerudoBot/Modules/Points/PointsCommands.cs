using Discord.Commands;
using PerudoBot.GameService;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        private async Task AwardPoints(GameObject game)
        {
            var gamePlayers = game.GetAllPlayers()
                .OrderBy(x => x.Rank)
                .ToList();

            await SendMessageAsync("`Points:`");
            foreach (var gamePlayer in gamePlayers)
            {
                var player = _db.Players.First(x => x.Id == gamePlayer.PlayerId);
                var calculatedOrder = gamePlayers.Count() - gamePlayer.Rank;
                var awardedPoints = GetNthFibbonacciNumber(calculatedOrder);
                player.Points += awardedPoints;

                _db.SaveChanges();

                await SendMessageAsync($"{gamePlayer.Name} `{player.Points}` ({awardedPoints})");
            }
        }

        private static int GetNthFibbonacciNumber(int n)
        {
            int number = n + 1; 
            int[] Fib = new int[number + 1];
            Fib[0] = 0;
            Fib[1] = 1;
            for (int i = 2; i <= number; i++)
            {
                Fib[i] = Fib[i - 2] + Fib[i - 1];
            }
            return Fib[number];
        }

        [Command("points")]
        public async Task Points(params string[] options)
        {
            var players = _db.Players
                .ToList()
                .OrderByDescending(x => x.Points)
                .ToList();

            var message = "`Points:`";

            foreach (var player in players)
            {
                message += $"\n{player.Name}: {player.Points}";
            }
            await SendMessageAsync(message);
        }
    }
}