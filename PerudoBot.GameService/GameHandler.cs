using System;
using System.Linq;
using PerudoBot.Database.Data;

namespace PerudoBot.GameService
{
    public class GameHandler
    {
        private readonly PerudoBotDbContext _db;

        public GameHandler(PerudoBotDbContext db)
        {
            _db = db;
        }

        public GameObject CreateVariableGame(ulong channelId, ulong guildId)
        {
            return CreateGame(channelId, guildId, GameMode.Variable);
        }

        public GameObject CreateSuddenDeathGame(ulong channelId, ulong guildId)
        {
            return CreateGame(channelId, guildId, GameMode.SuddenDeath);
        }

        private GameObject CreateGame(ulong channelId, ulong guildId, string gameMode)
        {
            if (_db.Games
                .Where(x => x.ChannelId == channelId)
                .Where(x => x.State == (int)(object)GameState.InProgress
                        || x.State == (int)(object)GameState.Setup)
               .Any())
            {
                string message = $"A game already being set up or is in progress.";
                return null; // how do we output this information... error message I guess?
            }

            var game = new Game
            {
                ChannelId = channelId,
                State = (int)(object)GameState.Setup,
                GuildId = guildId,
                Mode = gameMode
            };


            _db.Games.Add(game);
            _db.SaveChanges();

            var gameObject = new GameObject(game, _db);

            return gameObject;
        }
        public GameObject GetSettingUpGame(ulong channelId)
        {
            var game = _db.Games
                .Where(x => x.ChannelId == channelId)
                .Where(x => x.State == (int)GameState.Setup)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            if (game == null) return null;

            return new GameObject(game, _db);
        }
        public GameObject GetInProgressGame(ulong channelId)
        {
            var game = _db.Games
                .Where(x => x.ChannelId == channelId)
                .Where(x => x.State == (int)GameState.InProgress)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            if (game == null) return null;

            return new GameObject(game, _db);
        }


    }
}
