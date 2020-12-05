using System;
using System.Linq;
using PerudoBot.Database.Sqlite.Data;

namespace PerudoBot.GameService
{
    public class GameHandler
    {
        private PerudoBotDbContext _db;

        public GameHandler(PerudoBotDbContext db)
        {
            _db = db;
        }

        public GameObject CreateGame(ulong channelId, ulong guildId)
        {
            if (_db.Games
                .Where(x => x.ChannelId == channelId)
                //.Where(x => x.State == (int)(object)GameState.InProgress
                //        || x.State == (int)(object)GameState.Setup)
               .Any())
            {
                string message = $"A game already being set up or is in progress.";
                return null; // how do we output this information... error message I guess?
            }

            var game = new Game
            {
                ChannelId = channelId,
                State = 0,
                DateCreated = DateTime.Now,
                GuildId = guildId,
            };

            var gameObject = new GameObject(game);

            _db.Games.Add(game);
            _db.SaveChanges();

            return gameObject;
        }
    }
}
