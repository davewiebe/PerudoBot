using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.GameService.Extensions;
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

        public bool AddPlayer(ulong userId, ulong guildId, string username, string nickname, bool isBot)
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
                    UserId = userId,
                    IsBot = isBot
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

        public void SetModeVariable()
        {
            var game = _db.Games.Single(x => x.Id == _gameId);
            game.Mode = GameMode.Variable;
            _db.SaveChanges();
        }

        public void SetModeSuddenDeath()
        {
            var game = _db.Games.Single(x => x.Id == _gameId);
            game.Mode = GameMode.SuddenDeath;
            _db.SaveChanges();
        }

        public List<string> GetPlayers()
        {
            var game = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId);
            
            return game.GamePlayers.Select(x => x.Player.Name).ToList();
        }

        public string GetMode()
        {
            var game = _db.Games.Single(x => x.Id == _gameId);
            if (game.Mode == GameMode.SuddenDeath) return "Sudden Death";
            if (game.Mode == GameMode.Variable) return "Variable";
            return "Error";
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

            game.GamePlayerTurnId = shuffledGamePlayers.First().Id;
            game.State = (int)GameState.InProgress;

            _db.SaveChanges();
        }

        public RoundStatus StartNewRound()
        {
            var game = _db.Games
                .Include(x => x.Rounds)
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId);

            var activeGamePlayers = game.GamePlayers.Where(x => x.NumberOfDice > 0);
            if (activeGamePlayers.Count() == 1)
            {
                // end the game they won!
                game.WinnerPlayerId = activeGamePlayers.Single().Player.Id;
                game.State = (int)GameState.Finished;

                _db.SaveChanges();

                return new RoundStatus
                {
                    IsActive = false,
                    Winner = activeGamePlayers.Single().ToPlayerObject(),
                    PlayerDice = new List<PlayerDice>()
                };
            }

            // create new round
            var newRound = new StandardRound
            {
                GameId = _gameId,
                RoundNumber = game.Rounds.Count + 1,
                StartingPlayerId = game.GamePlayerTurnId
            };

            _db.Rounds.Add(newRound);


            // and set dice
            var r = new Random();
            var gamePlayers = game.GamePlayers.Where(x => x.NumberOfDice > 0).ToList();
            foreach (var player in gamePlayers)
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

            _db.SaveChanges();

            return new RoundStatus
            {
                IsActive = true,
                PlayerDice = GetPlayerDice()
            };
        }

        public void Terminate()
        {
            var game = _db.Games
                .Single(x => x.Id == _gameId);
            game.State = (int)GameState.Terminated;
            _db.SaveChanges();
        }

        public void Bid(int quantity, int pips)
        {
            var game = _db.Games
                .Include(x => x.Rounds)
                .ThenInclude(x => x.Actions)
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

            game.GamePlayerTurnId = GetNextActiveGamePlayerId();

            _db.Games.Single(x => x.Id == _gameId)
                .CurrentRound.Actions.Add(newBid);

            _db.SaveChanges();
        }


        public LiarResult Liar()
        {
            var game = _db.Games
                .Include(x => x.Rounds)
                .ThenInclude(x => x.Actions)
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.GamePlayerRounds)
                .Single(x => x.Id == _gameId);


            var liarCall = new LiarCall()
            {
                GamePlayer = game.CurrentGamePlayer,
                Round = game.CurrentRound,
                GamePlayerRound = game.CurrentGamePlayer.CurrentGamePlayerRound,
                ParentAction = game.CurrentRound.LatestAction,
            };

            var previousBid = game.CurrentRound.LatestAction as Bid;
            if (previousBid == null) return null; //throw error?

            var liarResult = new LiarResult
            {
                PlayerWhoBidLast = previousBid.GamePlayer.ToPlayerObject(),
                PlayerWhoCalledLiar = game.CurrentGamePlayer.ToPlayerObject(),
                BidQuantity = previousBid.Quantity,
                BidPips = previousBid.Pips
            };


            var actualQuantity = GetNumberOfDiceMatchingBid(game, previousBid.Pips);
            liarResult.ActualQuantity = actualQuantity;

            if (previousBid.Quantity <= actualQuantity)
            {
                // Bidder was correct
                liarResult.DiceLost = GetDiceLost((actualQuantity - previousBid.Quantity) + 1);
                liarResult.IsSuccessful = false;
                liarResult.PlayerWhoLostDice = liarResult.PlayerWhoCalledLiar;

                game.CurrentGamePlayer.NumberOfDice -= liarResult.DiceLost;

                SetGamePlayerTurn(game.CurrentGamePlayer.Id);
            }
            else // Liar caller was correct
            {
                liarResult.DiceLost = GetDiceLost(previousBid.Quantity - actualQuantity);
                liarResult.IsSuccessful = true;
                liarResult.PlayerWhoLostDice = liarResult.PlayerWhoBidLast;

                previousBid.GamePlayer.NumberOfDice -= liarResult.DiceLost;
                SetGamePlayerTurn(previousBid.GamePlayer.Id);
            }

            _db.Actions.Add(liarCall);
            _db.SaveChanges();

            return liarResult;
        }

        private int GetDiceLost(int numberOfDiceOffBy)
        {
            var gameMode = _db.Games.Single(x => x.Id == _gameId).Mode;

            if (gameMode == GameMode.Variable)
            {
                return numberOfDiceOffBy;
            }
            return 5;
        }

        public object GetCurrentRoundNumber()
        {
            return _db.Games
                .Include(x => x.Rounds)
                .Single(x => x.Id == _gameId).Rounds.Count;
        }

        private void SetGamePlayerTurn(int gamePlayerId)
        {
            var game = _db.Games.Single(x => x.Id == _gameId);

            var gamePlayer = _db.GamePlayers.Single(x => x.Id == gamePlayerId);
            if (gamePlayer.NumberOfDice <= 0)
            {
                game.GamePlayerTurnId = GetNextActiveGamePlayerId();
            }
            else
            {
                game.GamePlayerTurnId = gamePlayerId;
            }
        }

        private int GetNumberOfDiceMatchingBid(Game game, int pips) // turn into extension method
        {
            var allDice = game.CurrentRound.GamePlayerRounds.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));

            return allDice.Count(x => x == pips || x == 1);
        }

        public PlayerObject GetCurrentPlayer()
        {
            var currentGamePlayer = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId)
                .CurrentGamePlayer;

            return currentGamePlayer.ToPlayerObject();
        }

        private int GetNextActiveGamePlayerId()
        {
            var currentPlayerId = _db.Games.Single(x => x.Id == _gameId).GamePlayerTurnId;

            var playerIds = _db.GamePlayers
                .AsQueryable()
                .Where(x => x.GameId == _gameId)
                .Where(x => x.NumberOfDice > 0 || x.Id == currentPlayerId) // in case the current user is eliminated and won't show up
                .OrderBy(x => x.TurnOrder)
                .Select(x => x.Id)
                .ToList();

            var playerIndex = playerIds.FindIndex(x => x == currentPlayerId);

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
                Dice = x.Dice,
                IsBot = x.GamePlayer.Player.IsBot
            }).ToList();
        }
    }

    public class PlayerDice
    {
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public string Dice { get; set; }
        public bool IsBot { get; set; }

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

    public class LiarResult
    {
        public bool IsSuccessful { get; set; }
        public int DiceLost { get; set; }
        public PlayerObject PlayerWhoBidLast { get; set; }
        public PlayerObject PlayerWhoCalledLiar { get; set; }
        public PlayerObject PlayerWhoLostDice { get; set; }
        public int BidQuantity { get; internal set; }
        public int ActualQuantity { get; internal set; }
        public int BidPips { get; internal set; }

        public LiarResult()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is LiarResult other &&
                   IsSuccessful == other.IsSuccessful &&
                   DiceLost == other.DiceLost &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoBidLast, other.PlayerWhoBidLast) &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoCalledLiar, other.PlayerWhoCalledLiar) &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoLostDice, other.PlayerWhoLostDice);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccessful, DiceLost, PlayerWhoBidLast, PlayerWhoCalledLiar, PlayerWhoLostDice);
        }
    }

    public class RoundStatus
    {
        public bool IsActive { get; set; }
        public List<PlayerDice> PlayerDice { get; set; }
        public PlayerObject Winner { get; set; }

        public RoundStatus()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is RoundStatus other &&
                   IsActive == other.IsActive &&
                   EqualityComparer<List<PlayerDice>>.Default.Equals(PlayerDice, other.PlayerDice) &&
                   EqualityComparer<PlayerObject>.Default.Equals(Winner, other.Winner);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsActive, PlayerDice, Winner);
        }
    }
}