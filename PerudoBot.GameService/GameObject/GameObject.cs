using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.GameService.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.GameService
{
    public class GameObject : IGameObject
    {
        private readonly PerudoBotDbContext _db;
        private readonly ulong _channelId;
        private Game _game;

        public GameObject(PerudoBotDbContext db, ulong channelId)
        {
            _db = db;
            _channelId = channelId;
        }
        public bool LoadActiveGame()
        {
            _game = _db.Games
                .Where(x => x.ChannelId == _channelId)
                .Where(x => x.State == (int)GameState.InProgress)
                .Include(x => x.Rounds)
                .ThenInclude(x => x.Actions)
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.GamePlayerRounds)
                .Include(x => x.GamePlayers)
                .ThenInclude(x => x.Player)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            return (_game != null);
        }

        public bool CreateGame(ulong guildId)
        {
            var existingGame = _db.Games
                .Where(x => x.ChannelId == _channelId)
                .Where(x => x.State == (int)GameState.InProgress)
                .SingleOrDefault();
            if (existingGame != null)
            {
                return false;
            }

            var game = new Game
            {
                ChannelId = _channelId,
                State = (int)GameState.InProgress,
                GuildId = guildId,
                Mode = GameMode.Variable
            };

            _db.Games.Add(game);
            _db.SaveChanges();

            LoadActiveGame();
            return true;
        }


        internal bool IsInProgress()
        {
            return _game != null;
        }

        public int GetGameNumber()
        {
            return _game.Id;
        }
        
        public bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot)
        {
            if (string.IsNullOrEmpty(name)) return false;

            var isUserInGame = _game.GamePlayers
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
                GameId = _game.Id,
                Player = player
            });

            _db.SaveChanges();
            return true;
        }

        public bool SetModeVariable()
        {
            _game.Mode = GameMode.Variable;
            _db.SaveChanges();
            return true;
        }

        public bool SetModeSuddenDeath()
        {
            _game.Mode = GameMode.SuddenDeath;
            _db.SaveChanges();
            return true;
        }

        public List<PlayerObject> GetPlayers()
        {            
            return _game.GamePlayers.Select(x => x.ToPlayerObject()).OrderBy(x => x.Name).ToList();
        }

        public string GetMode()
        {
            if (_game.Mode == GameMode.SuddenDeath) return "Sudden Death";
            if (_game.Mode == GameMode.Variable) return "Variable";
            return "Error";
        }

        public void Start()
        {
            // shuffle players

            var shuffledGamePlayers = _game.GamePlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var turnOrder = 0;
            foreach (var gamePlayer in shuffledGamePlayers)
            {
                gamePlayer.TurnOrder = turnOrder;
                gamePlayer.NumberOfDice = 5;
                turnOrder += 1;
            }

            _game.GamePlayerTurnId = shuffledGamePlayers.First().Id;
            _game.State = (int)GameState.InProgress;

            _db.SaveChanges();

            // TODO: look into updating the private object more frequently?
            LoadActiveGame();
        }

        public RoundStatus StartNewRound()
        {
            var activeGamePlayers = _game.GamePlayers.Where(x => x.NumberOfDice > 0);
            if (activeGamePlayers.Count() == 1)
            {
                // end the game they won!
                _game.WinnerPlayerId = activeGamePlayers.Single().Player.Id;
                _game.State = (int)GameState.Finished;

                _db.SaveChanges();
                OnEndOfRound();

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
                GameId = _game.Id,
                RoundNumber = _game.Rounds.Count + 1,
                StartingPlayerId = _game.GamePlayerTurnId
            };

            _db.Rounds.Add(newRound);


            // and set dice
            var r = new Random();
            var gamePlayers = _game.GamePlayers.Where(x => x.NumberOfDice > 0).ToList();
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

            //TODO: DO I have to do this every time??
            LoadActiveGame();

            return new RoundStatus
            {
                IsActive = true,
                PlayerDice = GetPlayerDice(),
                RoundNumber = _game.CurrentRoundNumber
            };
        }

        public void OnEndOfRound()
        {
            Console.WriteLine("End Of Round");
        }

        public void Terminate()
        {
            _game.State = (int)GameState.Terminated;
            _db.SaveChanges();
        }

        public bool BidReverse(int quantity, int pips)
        {
            return Bid(quantity, pips, true);
        }
        public bool Bid(int quantity, int pips)
        {
            return Bid(quantity, pips, false);
        }
        private bool Bid(int quantity, int pips, bool reverse = false)
        {
            var previousBid = _game.CurrentRound
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
            if (reverse)
            {
                if (previousBid != null)
                {
                    return false;
                }

                var turnOrder = 1;
                foreach (var gamePlayer in _game.GamePlayers.OrderByDescending(x => x.TurnOrder))
                {
                    gamePlayer.TurnOrder = turnOrder;
                    turnOrder += 1;
                }
            }


            var newBid = new Bid
            {
                Quantity = quantity,
                Pips = pips,
                ParentAction = previousBid,
                Round = _game.CurrentRound,
                GamePlayer = _game.CurrentGamePlayer,
                GamePlayerRound = _game.CurrentGamePlayer.CurrentGamePlayerRound,
            };
            // TODO: This is a bit ugly. would prefer only 1 savechanges
            _db.SaveChanges();
            _game.GamePlayerTurnId = GetNextActiveGamePlayerId();

            _game.CurrentRound.Actions.Add(newBid);

            _db.SaveChanges();

            return true;
        }


        public LiarResult Liar()
        {
            var liarCall = new LiarCall()
            {
                GamePlayer = _game.CurrentGamePlayer,
                Round = _game.CurrentRound,
                GamePlayerRound = _game.CurrentGamePlayer.CurrentGamePlayerRound,
                ParentAction = _game.CurrentRound.LatestAction,
            };

            if (_game.CurrentRound.LatestAction is not Bid previousBid) return null; //throw error?

            var liarResult = new LiarResult
            {
                PlayerWhoBidLast = previousBid.GamePlayer.ToPlayerObject(),
                PlayerWhoCalledLiar = _game.CurrentGamePlayer.ToPlayerObject(),
                BidQuantity = previousBid.Quantity,
                BidPips = previousBid.Pips
            };


            var actualQuantity = GetNumberOfDiceMatchingBid(_game, previousBid.Pips);
            liarResult.ActualQuantity = actualQuantity;

            if (previousBid.Quantity <= actualQuantity)
            {
                // Bidder was correct
                liarResult.DiceLost = GetDiceLost((actualQuantity - previousBid.Quantity) + 1);
                liarResult.IsSuccessful = false;
                liarResult.PlayerWhoLostDice = liarResult.PlayerWhoCalledLiar;

                _game.CurrentGamePlayer.NumberOfDice -= liarResult.DiceLost;

                SetGamePlayerTurn(_game.CurrentGamePlayer.Id);
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
            var gameMode = _game.Mode;

            if (gameMode == GameMode.Variable)
            {
                return numberOfDiceOffBy;
            }
            return 5;
        }

        public int GetCurrentRoundNumber()
        {
            return _game.Rounds.Count;
        }

        private void SetGamePlayerTurn(int gamePlayerId)
        {
            var gamePlayer = _db.GamePlayers.Single(x => x.Id == gamePlayerId);
            if (gamePlayer.NumberOfDice <= 0)
            {
                _game.GamePlayerTurnId = GetNextActiveGamePlayerId();
            }
            else
            {
                _game.GamePlayerTurnId = gamePlayerId;
            }
        }

        private int GetNumberOfDiceMatchingBid(Game game, int pips) // turn into extension method
        {
            var allDice = game.CurrentRound.GamePlayerRounds.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));

            return allDice.Count(x => x == pips || x == 1);
        }

        public PlayerObject GetCurrentPlayer()
        {
            var currentGamePlayer = _game.CurrentGamePlayer;

            return currentGamePlayer.ToPlayerObject();
        }

        private int GetNextActiveGamePlayerId()
        {
            var currentGamePlayerId = _game.GamePlayerTurnId;

            var gamePlayerIds = _game.GamePlayers
                .Where(x => x.NumberOfDice > 0 || x.Id == currentGamePlayerId) // in case the current user is eliminated and won't show up
                .OrderBy(x => x.TurnOrder)
                .Select(x => x.Id)
                .ToList();

            var playerIndex = gamePlayerIds.FindIndex(x => x == currentGamePlayerId);

            if (playerIndex >= gamePlayerIds.Count - 1)
            {
                return gamePlayerIds.ElementAt(0);
            }
            else
            {
                return gamePlayerIds.ElementAt(playerIndex + 1);
            }
        }

        public List<PlayerDice> GetPlayerDice()
        {
            return _game.CurrentRound.GamePlayerRounds.Select(x => new PlayerDice
            {
                Name = x.GamePlayer.Player.Name,
                UserId = x.GamePlayer.Player.UserId,
                NumberOfDice = x.GamePlayer.NumberOfDice,
                Dice = x.Dice,
                IsBot = x.GamePlayer.Player.IsBot,
                TurnOrder = x.GamePlayer.TurnOrder
            }).ToList();
        }

        public void OnEndOfGame()
        {
            //throw new NotImplementedException();
        }
    }
}