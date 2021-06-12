using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Action = PerudoBot.Database.Data.Action;

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
            var game = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId);
            
            return game.GamePlayers.Select(x => x.Player.Name).ToList();
        }

        public void Start()
        {
            // shuffle players

            var game = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId);

            var shuffledGamePlayers = game.GamePlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var turnOrder = 0;
            foreach (var gamePlayer in shuffledGamePlayers)
            {
                gamePlayer.TurnOrder = turnOrder;
                gamePlayer.NumberOfDice = 5;
                turnOrder += 1;
            }

            // create new round
            var newRound = new StandardRound
            {
                GameId = _gameId,
                RoundNumber = 1,
                StartingPlayerId = shuffledGamePlayers.First().Id
            };

            _db.Rounds.Add(newRound);


            var r = new Random();
            // and set dice
            foreach (var player in shuffledGamePlayers)
            {
                var dice = new List<int>();
                for (int i = 0; i < player.NumberOfDice; i++)
                {
                    dice.Add(r.Next(1, 6 + 1));
                }
                var gamePlayerRound = new GamePlayerRound
                {
                    Round = newRound,
                    Dice = string.Join(",", dice),
                    GamePlayer = player
                };

                _db.GamePlayerRounds.Add(gamePlayerRound);
            }

            // Start Round
            game.State = (int)GameState.InProgress;
            game.PlayerTurnId = shuffledGamePlayers.First().Id;

            var activeGamePlayers = game.GamePlayers.Where(x => x.NumberOfDice > 0);
            if (activeGamePlayers.Count() == 1)
            {
                // end the game they won!
                game.WinnerPlayerId = activeGamePlayers.Single().Player.Id;
            }

            _db.SaveChanges();
        }

        public void Bid(int quantity, int pips)
        {
            var game = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.GamePlayerRounds)
                .Single(x => x.Id == _gameId);


            var previousBid = game.CurrentRound
                .Actions.OfType<Bid>()
                .LastOrDefault();

            if (previousBid != null)
            {
                if (previousBid.Quantity > quantity)
                {
                    throw new ArgumentOutOfRangeException("Bid not high enough");
                }
                if (previousBid.Quantity == quantity)
                {
                    if (previousBid.Pips >= pips)
                    {
                        throw new ArgumentOutOfRangeException("Bid not high enough");
                    }
                }
            }


            var newBid = new Bid
            {
                Quantity = quantity,
                Pips = pips,
                ParentAction = previousBid,
                Round = game.CurrentRound,
                GamePlayer = game.CurrentGamePlayer,
                GamePlayerRound = game.CurrentGamePlayer.CurrentGamePlayerRound,
            };

            _db.Games.Single(x => x.Id == _gameId)
                .CurrentRound.Actions.Add(newBid);

            _db.SaveChanges();
        }

        public PlayerObject GetCurrentPlayer()
        {
            var currentRound = _db.Games
                .Include(x => x.Rounds)
                .ThenInclude(x => x.Actions)
                .Single(x => x.Id == _gameId)
                .CurrentRound;

            int currentGamePlayerId;
            if (currentRound.LatestAction == null) currentGamePlayerId = currentRound.StartingPlayerId;
            else
            {
                var lastPlayerId = currentRound.LatestAction.GamePlayerId;

                currentGamePlayerId = GetNextPlayer(lastPlayerId);
            }

            var currentGamePlayer = _db.GamePlayers
                .Include(x => x.Player)
                .Single(x => x.Id == currentGamePlayerId);
                
            return new PlayerObject
            {
                NumberOfDice = currentGamePlayer.NumberOfDice,
                IsBot = currentGamePlayer.Player.IsBot,
                Name = currentGamePlayer.Player.Name,
                UserId = currentGamePlayer.Player.UserId
            };
        }

        private int GetNextPlayer(int lastPlayerId)
        {
            var playerIds = _db.GamePlayers
                .AsQueryable()
                .Where(x => x.GameId == _gameId)
                .Where(x => x.NumberOfDice > 0 || x.Player.Id == lastPlayerId) // in case the current user is eliminated and won't show up
                .OrderBy(x => x.TurnOrder)
                .Select(x => x.Id)
                .ToList();

            var playerIndex = playerIds.FindIndex(x => x == lastPlayerId);

            if (playerIndex >= playerIds.Count - 1)
            {
                return playerIds.ElementAt(0);
            }
            else
            {
                return playerIds.ElementAt(playerIndex + 1);
            }
        }

        public List<PlayerDice> GetPlayerDice()
        {
            var game = _db.Games
                .Include(x => x.Rounds)
                .ThenInclude(x => x.GamePlayerRounds)
                .ThenInclude(x => x.GamePlayer.Player)
                .Single(x => x.Id == _gameId);

            return game.CurrentRound.GamePlayerRounds.Select(x => new PlayerDice
            {
                Name = x.GamePlayer.Player.Name,
                UserId = x.GamePlayer.Player.UserId,
                Dice = x.Dice
            }).ToList();
        }
    }

    public class PlayerDice
    {
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public string Dice { get; set; }

        public PlayerDice()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerDice other &&
                   Name == other.Name &&
                   UserId == other.UserId &&
                   Dice == other.Dice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UserId, Dice);
        }
    }

    public class PlayerObject
    {
        public int NumberOfDice { get; internal set; }
        public bool IsBot { get; internal set; }
        public string Name { get; internal set; }
        public ulong UserId { get; internal set; }

        public PlayerObject()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerObject other &&
                   NumberOfDice == other.NumberOfDice &&
                   IsBot == other.IsBot &&
                   Name == other.Name &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NumberOfDice, IsBot, Name, UserId);
        }
    }
}