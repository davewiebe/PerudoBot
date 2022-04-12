using Discord.Commands;
using PerudoBot.GameService;
using System.Linq;
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

            if(gamePlayers.Count == 1)
            {
                var gamePlayer = gamePlayers.First();
                await SendMessageAsync($"{gamePlayer.Name} has been awarded no points for participating in an incomplete game.");
                return;
            }

            await SendMessageAsync("`Points:`");
            foreach (var gamePlayer in gamePlayers)
            {
                var player = _db.Players.First(x => x.Id == gamePlayer.PlayerId);
                var originalPoints = player.TotalPoints;

                var awardedPoints = (gamePlayers.Count() - gamePlayer.Rank + 1) * 10;
                player.TotalPoints += awardedPoints;

                _db.SaveChanges();

                await SendMessageAsync($"`{gamePlayer.Rank}` {gamePlayer.Name} `{originalPoints}` => `{player.TotalPoints}` ({awardedPoints})");
            }
        }

        [Command("points")]
        public async Task Points(params string[] options)
        {
            var players = _db.Players
                .ToList()
                .OrderByDescending(x => x.TotalPoints)
                .ToList();

            var message = "`Points:`";

            foreach (var player in players)
            {
                message += $"\n{player.Name}: `{player.TotalPoints}`";
            }
            await SendMessageAsync(message);
        }
    }
}