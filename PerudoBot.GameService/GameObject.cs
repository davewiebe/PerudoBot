using PerudoBot.Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.GameService
{
    public class GameObject
    {
        private int _gameId;
        private PerudoBotDbContext _db;

        public GameObject(Game game, PerudoBotDbContext db)
        {
            _db = db;
            _gameId = game.Id;
        }

        public int GetGameNumber()
        {
            return _gameId;
        }

        public bool AddPlayer(ulong userId, ulong guildId, string username, string nickname)
        {
            // check if user exists
            var player = _db.Players
                .Where(x => x.UserId == userId)
                .Where(x => x.GuildId == guildId)
                .SingleOrDefault();

            // update player nickname if it's out of date
            if (player != null)
            {
                if (player.Name != nickname && !string.IsNullOrEmpty(nickname)) player.Name = nickname;
            }
            else
            {
                // add new player
                player = new Player
                {
                    Name = nickname ?? username,
                    GuildId = guildId,
                    UserId = userId
                };
                _db.Players.Add(player);
            }

            var userInGameAlready = _db.GamePlayers
                .Where(x => x.GameId == _gameId)
                .Where(x => x.Player.UserId == userId)
                .Any();

            if (userInGameAlready) return false;

            _db.GamePlayers.Add(new GamePlayer
            {
                GameId = _gameId,
                Player = player
            });

            _db.SaveChanges();
            return true;
        }

        public List<string> GetPlayers()
        {
            var game = _db.Games.Single(x => x.Id == _gameId);
            
            return game.GamePlayers.Select(x => x.Player.Name).ToList();
        }

        public void Start()
        {
            // create new round

            // shuffle players
            // and set dice
            var game = _db.Games.Single(x => x.Id == _gameId);

            var shuffledPlayers = game.GamePlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var turnOrder = 0;
            foreach (var player in shuffledPlayers)
            {
                player.TurnOrder = turnOrder;
                player.NumberOfDice = 5;
                turnOrder += 1;
            }


            // Start Round

            var activeGamePlayers = game.GamePlayers.Where(x => x.NumberOfDice > 0);
            bool onlyOnePlayerLeft = activeGamePlayers.Count() == 1;
            if (onlyOnePlayerLeft)
            {
                // end the game they won!
                game.WinnerPlayerId = activeGamePlayers.Single().Player.Id;
            }
        }

        public IEnumerable<object> GetPlayerDice()
        {
            throw new NotImplementedException();
        }
    }
}