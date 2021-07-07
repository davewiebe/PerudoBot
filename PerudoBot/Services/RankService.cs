using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.Services
{
    public class RankService
    {
        private PerudoBotDbContext _db;

        public RankService(PerudoBotDbContext db)
        {
            _db = db;
        }
        //public async Task UpdateGamePlayerRanksAsync(int gameId)
        //{
        //    var rankings = _db.GamePlayers.AsQueryable()
        //        .Include(gp => gp.GamePlayerRounds)
        //        .Where(gp => gp.GameId == gameId)
        //        .Select(gp => new
        //        {
        //            gp.GameId,
        //            GamePlayerId = gp.Id,
        //            gp.Player.Name,
        //            HighestRoundPlayed = gp.GamePlayerRounds.OrderByDescending(gpr => gpr.Round.RoundNumber).First()
        //        })
        //        .Select(gp => new
        //        {
        //            gp.GameId,
        //            gp.GamePlayerId,
        //            gp.Name,
        //            HighestRoundPlayed = gp.HighestRoundPlayed.Round.RoundNumber,
        //            gp.HighestRoundPlayed.IsEliminated,
        //        })
        //        .OrderByDescending(x => x.HighestRoundPlayed).ThenBy(x => x.IsEliminated ? 1 : 0)
        //        .ToList()
        //        // The rest must be done in memory
        //        .Where(x => x.HighestRoundPlayed != 0)
        //        .Select((x, i) => new
        //        {
        //            Rank = ++i,
        //            x.GameId,
        //            x.GamePlayerId,
        //            x.Nickname,
        //            x.HighestRoundPlayed,
        //            x.IsEliminated,
        //            x.IsGhost
        //        })
        //        .ToList();

        //    var gamePlayers = _db.GamePlayers.AsQueryable()
        //        .Where(gp => gp.GameId == gameId)
        //        .ToList();

        //    foreach (var ranking in rankings)
        //    {
        //        var gamePlayerToUpdate = gamePlayers
        //            .Where(gp => gp.Id == ranking.GamePlayerId)
        //            .Single();

        //        gamePlayerToUpdate.Rank = ranking.Rank;
        //    }

        //    await _db.SaveChangesAsync();
        //}
    }
}
