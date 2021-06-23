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
        private readonly int _gameId;
        private readonly PerudoBotDbContext _db;

        public GameObject(Game game, PerudoBotDbContext db)
        {
            _db = db;
            _gameId = game.Id;
        }

        public int GetGameNumber()
        {
            return _gameId;
        }

        public bool RemovePlayer(ulong userId)
        {
            var gamePlayer = _db.GamePlayers
                            .Where(x => x.GameId == _gameId)
                            .Single(x => x.Player.UserId == userId);

            if (gamePlayer == null) return false;

            _db.GamePlayers.Remove(gamePlayer);
            _db.SaveChanges();
            return true;
        }

        public bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot)
        {
            if (string.IsNullOrEmpty(name)) return false;

            var isUserInGame = _db.GamePlayers
                .Where(x => x.GameId == _gameId)
                .Where(x => x.Player.UserId == userId)
                .Any();

            if (isUserInGame) return false;

            // check if user exists
            var player = _db.Players
                .Where(x => x.UserId == userId)
                .Where(x => x.GuildId == guildId)
                .SingleOrDefault();

            if (player == null)
            {
                // add new player
                player = new Player
                {
                    Name = name,
                    GuildId = guildId,
                    UserId = userId,
                    IsBot = isBot
                };
                _db.Players.Add(player);
            }
            else
            {
                // update player name if it changed
                if (player.Name != name) player.Name = name;
            }

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

        public List<PlayerObject> GetPlayers()
        {
            var game = _db.Games
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .Single(x => x.Id == _gameId);
            
            return game.GamePlayers.Select(x => x.ToPlayerObject()).ToList();
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
                dice.Sort();
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
                PlayerDice = GetPlayerDice(),
                RoundNumber = game.CurrentRoundNumber
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

            if (game.CurrentRound.LatestAction is not Bid previousBid) return null; //throw error?

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

        public int GetCurrentRoundNumber()
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
                NumberOfDice = x.GamePlayer.NumberOfDice,
                Dice = x.Dice,
                IsBot = x.GamePlayer.Player.IsBot,
                TurnOrder = x.GamePlayer.TurnOrder
            }).ToList();
        }
    }
}