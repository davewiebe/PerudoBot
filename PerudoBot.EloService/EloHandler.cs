using PerudoBot.Database.Data;
using PerudoBot.EloService.Elo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.EloService
{
    public class EloHandler
    {
        private readonly PerudoBotDbContext _db;
        //private ulong _channelId;
        private ulong _guildId;
        private EloMatch _match;
        private readonly string _gameMode;

        public EloHandler(PerudoBotDbContext db, ulong guildId, string gameMode)
        {
            _db = db;
            _guildId = guildId;
            _match = new EloMatch();
            _gameMode = gameMode;
        }

        public EloSeason GetCurrentEloSeason()
        {
            var eloSeason = _db.EloSeasons
                .AsQueryable()
                .Where(x => x.GuildId == _guildId)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            if (eloSeason == null) return CreateNewEloSeason();
            return eloSeason;
        }

        public void CalculateAndSaveElo()
        {
            _match.CalculateElos(20);

            var playerResults = _match.GetPlayers();
            foreach (var playerData in playerResults)
            {
                SetEloForPlayer(playerData.PlayerId, playerData.EloPost);
            }
            _db.SaveChanges();
        }
        public List<PlayerElo> GetEloResults()
        {
            var players = _match.GetPlayers();

            return players
                .OrderBy(x => x.Place)
                .Select(x => new PlayerElo(x.PlayerId, x.EloPost, x.EloPre))
                .ToList();
        }

        public void AddPlayer(int playerId, int rank)
        {
            var elo = GetCurrentEloForPlayer(playerId);
            _match.AddPlayer(playerId, rank, elo);
        }

        private EloSeason CreateNewEloSeason()
        {
            EloSeason currentSeason = new EloSeason
            {
                GuildId = _guildId,
                SeasonName = "Season Zero"
            };
            _db.EloSeasons.Add(currentSeason);
            _db.SaveChanges();
            return currentSeason;
        }

        private int GetCurrentEloForPlayer(int playerId)
        {
            var currentSeason = GetCurrentEloSeason();

            var playerElo = _db.PlayerElos
                .AsQueryable()
                .Where(x => x.PlayerId == playerId)
                .Where(x => x.EloSeason == currentSeason)
                .Where(x => x.GameMode == _gameMode)
                .SingleOrDefault();

            if (playerElo == null)
            {
                return 1500;
            }
            return playerElo.Rating;
        }

        private void SetEloForPlayer(int playerId, int newEloRating)
        {
            var currentSeason = GetCurrentEloSeason();

            var playerElo = _db.PlayerElos
                .AsQueryable()
                .Where(x => x.PlayerId == playerId)
                .Where(x => x.EloSeason == currentSeason)
                .Where(x => x.GameMode == _gameMode)
                .SingleOrDefault();

            if (playerElo != null)
            {
                playerElo.Rating = newEloRating;
                _db.SaveChanges();
                return;
            }

            _db.PlayerElos.Add(new Database.Data.PlayerElo
            {
                EloSeason = currentSeason,
                PlayerId = playerId,
                GameMode = _gameMode,
                Rating = newEloRating
            });
            _db.SaveChanges();
        }
    }

    public class PlayerElo
    {
        public int PlayerId { get; }
        public int Elo { get; }
        public int PreviousElo { get; }

        public PlayerElo(int playerId, int elo, int previousElo)
        {
            PlayerId = playerId;
            Elo = elo;
            PreviousElo = previousElo;
        }
    }
}
