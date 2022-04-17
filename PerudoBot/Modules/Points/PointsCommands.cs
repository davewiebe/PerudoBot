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

            var message = "`Points:`";
            foreach (var gamePlayer in gamePlayers)
            {
                var player = _db.Players.First(x => x.Id == gamePlayer.PlayerId);
                var originalPoints = player.AvailablePoints;

                var awardedPoints = (gamePlayers.Count() - gamePlayer.Rank + 1) * 10;
                player.TotalPoints += awardedPoints;

                _db.SaveChanges(); ;

                message += $"\n`{gamePlayer.Rank}` {gamePlayer.Name} `{originalPoints}` => `{player.AvailablePoints}` ({awardedPoints})";
            }

            await SendMessageAsync(message);
        }

        public int GetAvailablePoints(int playerId)
        {
            return _db.Players.First(x => x.Id == playerId).AvailablePoints;
        }

        public void AddTotalPoints(int playerId, int points)
        {
            var player = _db.Players.First(x => x.Id == playerId);
            player.TotalPoints += points;
            _db.SaveChanges();
        }

        public void AddUsedPoints(int playerId, int points)
        {
            var player = _db.Players.First(x => x.Id == playerId);
            player.UsedPoints += points;
            _db.SaveChanges();
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
                message += $"\n{player.Name}: `{player.AvailablePoints}` `({player.TotalPoints})`";
            }
            await SendMessageAsync(message);
        }
    }
}