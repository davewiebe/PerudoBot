using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.Extensions;
using PerudoBot.GameService.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.GameService
{
    public class GameObject
    {
        private readonly PerudoBotDbContext _db;
        private readonly ulong _channelId;
        private readonly ulong _guildId;
        private Game _game;
        private Random _random;

        public GameObject(PerudoBotDbContext db, ulong channelId, ulong guildId)
        {
            _db = db;
            _channelId = channelId;
            _guildId = guildId;
            _random = new Random();
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
                .ThenInclude(x => x.DiscordPlayer)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            return (_game != null);
        }

        public Round GetCurrentRound()
        {
            return _game.CurrentRound;
        }

        public RoundStatus GetCurrentRoundStatus()
        {
            return new RoundStatus
            {
                IsActive = _game.State == (int)GameState.InProgress,
                Players = GetAllPlayers(),
                RoundNumber = _game.CurrentRoundNumber
            };
        }

        public void SetPlayerDice(int playerId, string dice)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.Player.Id == playerId);
            gamePlayer.CurrentGamePlayerRound.Dice = dice;
            _db.SaveChanges();
        }

        public bool CreateGame()
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
                GuildId = _guildId,
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

        public bool AddPlayer(int playerId, string name)
        {
            if (string.IsNullOrEmpty(name)) return false;

            var isUserInGame = _game.GamePlayers
                .Where(x => x.Player.Id == playerId)
                .Any();

            if (isUserInGame) return false;

            // check if user exists
            var player = _db.Players
                .Where(x => x.Id == playerId)
                .SingleOrDefault();

            if (player == null)
            {
                // add new player
                player = new Player
                {
                    Name = name,
                    Id = playerId
                };
                _db.Players.Add(player);
            }
            else
            {
                // update player name if it changed
                if (player.Name != name) player.Name = name;
            }

            var numberOfPlayers = _game.GamePlayers.Count();

            _game.GamePlayers.Add(new GamePlayer
            {
                GameId = _game.Id,
                Player = player,
                TurnOrder = numberOfPlayers + 1,
                NumberOfDice = _game.Mode == GameMode.Reverse ? 1 : 5
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

        public bool SetModeReverse()
        {
            _game.Mode = GameMode.Reverse;
            _db.SaveChanges();
            return true;
        }

        public List<PlayerData> GetAllPlayers()
        {
            return _game.GamePlayers
                .Select(x => x.ToPlayerObject())
                .OrderBy(x => x.Name)
                .ToList();
        }

        public bool HasPlayerWithDice(int playerId)
        {
            return _game.GamePlayers
                .Any(x => x.PlayerId == playerId && x.NumberOfDice > 0);
        }

        public string GetMode()
        {
            if (_game.Mode == GameMode.SuddenDeath) return "Sudden Death";
            if (_game.Mode == GameMode.Variable) return "Variable";
            if (_game.Mode == GameMode.Reverse) return "Reverse";
            return "Error";
        }

        public void ShufflePlayers()
        {
            var shuffledGamePlayers = _game.GamePlayers.OrderBy(x => Guid.NewGuid()).ToList();
            var turnOrder = 0;
            foreach (var gamePlayer in shuffledGamePlayers)
            {
                gamePlayer.TurnOrder = turnOrder;
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
            if (_game.GamePlayerTurnId == 0)
            {
                var firstPlayer = _game.GamePlayers.OrderBy(x => x.TurnOrder).First();
                _game.GamePlayerTurnId = firstPlayer.Id;
            }

            var activeGamePlayers = _game.GamePlayers.Where(x => x.NumberOfDice > 0);
            foreach (var player in _game.GamePlayers)
            {
                player.IsAutoLiar = false;
            }
            _db.SaveChanges();

            if (activeGamePlayers.Count() == 1)
            {
                // end the game they won!
                var winner = activeGamePlayers.Single();
                winner.Rank = 1;
                _game.WinnerPlayerId = winner.Player.Id;
                _game.State = (int)GameState.Finished;

                _db.SaveChanges();
                OnEndOfRound();

                return new RoundStatus
                {
                    IsActive = false,
                    Winner = activeGamePlayers.Single().ToPlayerObject(),
                    Players = _game.GamePlayers.Select(x => new PlayerData
                    {
                        //IsBot = x.Player.IsBot,
                        Name = x.Player.Name,
                        Rank = x.Rank,
                        NumberOfDice = x.NumberOfDice,
                        PlayerId = x.Player.Id,
                        PlayerMetadata = x.Player.Metadata.ToDictionary(x => x.Key, x => x.Value),
                        GamePlayerMetadata = x.Metadata.ToDictionary(x => x.Key, x => x.Value)
                    }).ToList()
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
            var gamePlayers = _game.GamePlayers.ToList();
            foreach (var player in gamePlayers)
            {
                var dice = new List<int>();
                for (int i = 0; i < player.NumberOfDice; i++)
                {
                    dice.Add(_random.Next(1, 6 + 1));
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
                Players = GetAllPlayers(),
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

        public void ReversePlayerOrder()
        {
            var turnOrder = 1;
            foreach (var gamePlayer in _game.GamePlayers.OrderByDescending(x => x.TurnOrder))
            {
                gamePlayer.TurnOrder = turnOrder;
                turnOrder += 1;
            }
        }

        public void Bid(int playerId, int quantity, int pips)
        {
            var previousBid = GetPreviousBid();

            var player = _game.GamePlayers.Single(x => x.PlayerId == playerId);
            var newBid = new Bid
            {
                Quantity = quantity,
                Pips = pips,
                ParentAction = previousBid,
                Round = _game.CurrentRound,
                GamePlayer = player,
                GamePlayerRound = player.CurrentGamePlayerRound,
            };
            // TODO: This is a bit ugly. would prefer only 1 savechanges
            _db.SaveChanges();

            _game.GamePlayerTurnId = GetNextActiveGamePlayerId(_game.GamePlayerTurnId);

            _game.CurrentRound.Actions.Add(newBid);

            _db.SaveChanges();
        }

        public bool BidValidate(int playerId, int quantity, int pips)
        {
            if (_game.CurrentGamePlayer.PlayerId != playerId) return false;

            if (pips > 6) return false;

            var previousBid = GetPreviousBid();

            if (previousBid != null)
            {
                var previousBidSize = previousBid.Quantity * 100 + previousBid.Pips;
                if (previousBid.Pips == 1) previousBidSize += previousBid.Quantity * 100;

                var currentBidSize = quantity * 100 + pips;
                if (pips == 1) currentBidSize += quantity * 100;

                if (previousBidSize >= currentBidSize)
                {
                    return false;
                }
            }

            if (previousBid == null && pips == 1)
            {
                return false;
            }

            return true;
        }

        public LiarResult Liar(int playerId)
        {
            //if (_game.CurrentGamePlayer.PlayerId != playerId) return null;
            var player = _game.GamePlayers.Single(x => x.PlayerId == playerId);

            if (player.NumberOfDice == 0) return null;

            var liarCall = new LiarCall()
            {
                GamePlayer = player,
                Round = _game.CurrentRound,
                GamePlayerRound = player.CurrentGamePlayerRound,
                ParentAction = _game.CurrentRound.LatestAction,
            };

            if (_game.CurrentRound.LatestAction is not Bid previousBid) return null; //throw error?

            var PlayerWhoBidLast = previousBid.GamePlayer;
            var PlayerWhoCalledLiar = player;
            GamePlayer PlayerWhoLostDice;
            var liarResult = new LiarResult
            {
                BidQuantity = previousBid.Quantity,
                BidPips = previousBid.Pips
            };


            var actualQuantity = GetNumberOfDiceMatchingBid(previousBid.Pips);
            liarResult.ActualQuantity = actualQuantity;

            if (previousBid.Quantity <= actualQuantity)
            {
                // Bidder was correct
                liarResult.DiceLost = GetDiceLost((actualQuantity - previousBid.Quantity) + 1, PlayerWhoCalledLiar.NumberOfDice);
                liarResult.IsSuccessful = false;
                PlayerWhoLostDice = PlayerWhoCalledLiar;

                DecrementDice(PlayerWhoCalledLiar, liarResult.DiceLost);

                SetGamePlayerTurn(PlayerWhoCalledLiar.Id);
            }
            else // Liar caller was correct
            {
                liarResult.DiceLost = GetDiceLost(previousBid.Quantity - actualQuantity, PlayerWhoBidLast.NumberOfDice);
                liarResult.IsSuccessful = true;
                PlayerWhoLostDice = PlayerWhoBidLast;

                DecrementDice(PlayerWhoBidLast, liarResult.DiceLost);

                SetGamePlayerTurn(PlayerWhoBidLast.Id);
            }

            _db.Actions.Add(liarCall);
            _db.SaveChanges();

            liarResult.PlayerWhoBidLast = PlayerWhoBidLast.ToPlayerObject();
            liarResult.PlayerWhoCalledLiar = PlayerWhoCalledLiar.ToPlayerObject();
            liarResult.PlayerWhoLostDice = PlayerWhoLostDice.ToPlayerObject();


            return liarResult;
        }

        private void DecrementDice(GamePlayer PlayerWhoLostDice, int diceLost)
        {
            PlayerWhoLostDice.NumberOfDice -= diceLost;
            if (PlayerWhoLostDice.NumberOfDice <= 0)
            {
                PlayerWhoLostDice.NumberOfDice = 0;
                PlayerWhoLostDice.Rank = _game.GamePlayers
                    .Where(x => x.PlayerId != PlayerWhoLostDice.PlayerId)
                    .Count(x => x.NumberOfDice > 0) + 1;
            }
        }

        private int GetDiceLost(int numberOfDiceOffBy, int currentDice)
        {
            var gameMode = _game.Mode;

            if (gameMode == GameMode.Variable)
            {
                return numberOfDiceOffBy;
            }

            if (gameMode == GameMode.SuddenDeath)
            {
                return 5;
            }

            int diceReverseGained = currentDice + numberOfDiceOffBy;

            if (diceReverseGained > 5)
            {
                return currentDice;
            }

            return -numberOfDiceOffBy;
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
                _game.GamePlayerTurnId = GetNextActiveGamePlayerId(gamePlayerId);
            }
            else
            {
                _game.GamePlayerTurnId = gamePlayerId;
            }
        }

        private int GetNumberOfDiceMatchingBid(int pips) // turn into extension method
        {
            return GetAllDice().Count(x => x == pips || x == 1);
        }

        public List<int> GetAllDice()
        {
            return _game.CurrentRound
                .GamePlayerRounds
                .Where(x => x.Dice != "")
                .SelectMany(x => x.Dice.ToIntegerDice())
                .ToList();
        }

        public bool? IsPlayerAutoLiar(int playerId)
        {
            var gamePlayer = _game.GamePlayers.SingleOrDefault(x => x.PlayerId == playerId);
            return gamePlayer.IsAutoLiar;
        }

        public PlayerData GetPlayer(int playerId)
        {
            var gamePlayer = _game.GamePlayers.SingleOrDefault(x => x.PlayerId == playerId);
            return gamePlayer.ToPlayerObject();
        }

        public void ApplyAutoLiarToPlayer(int playerId)
        {
            var gamePlayer = _game.GamePlayers.SingleOrDefault(x => x.PlayerId == playerId);
            gamePlayer.IsAutoLiar = true;
            _db.SaveChanges();
        }

        public void RemoveAutoLiarFromPlayer(int playerId)
        {
            var gamePlayer = _game.GamePlayers.SingleOrDefault(x => x.PlayerId == playerId);
            gamePlayer.IsAutoLiar = false;
            _db.SaveChanges();
        }

        public PlayerData GetCurrentPlayer()
        {
            var currentGamePlayer = _game.CurrentGamePlayer;

            return currentGamePlayer.ToPlayerObject();
        }

        private int GetNextActiveGamePlayerId(int gamePlayerId)
        {
            var currentGamePlayerId = gamePlayerId;

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

        public void OnEndOfGame()
        {
            //throw new NotImplementedException();
        }

        public string GetGameMode()
        {
            return _game.Mode;
        }

        public bool HasBots()
        {
            return _game.GamePlayers.Any(p => p.Player.DiscordPlayer.IsBot && p.NumberOfDice > 0);
        }

        public void SetPlayerDice(int playerId, int numDice)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.PlayerId == playerId);
            gamePlayer.NumberOfDice = numDice;
            _db.SaveChanges();
        }

        public void AddRandomDice(int playerId, int numDice, bool isMystery = false)
        {
            var diceToAdd = new List<int>();
            for (int i = 0; i < numDice; i++)
            {
                diceToAdd.Add(_random.Next(1, 7));
            }
            AddDice(playerId, diceToAdd, isMystery);
        }

        public void AddDice(int playerId, List<int> dice, bool isMystery = false)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.PlayerId == playerId);

            foreach (int die in dice)
            {
                AddDie(gamePlayer, die, isMystery);
            }

            _db.SaveChanges();
        }

        public List<int> RemoveDiceFromStart(int playerId, int numDice)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.PlayerId == playerId);
            var removedDice = new List<int>();

            for (int i = 0; i < numDice; i++)
            {
                var removedDie = RemoveDie(gamePlayer, 0);
                removedDice.Add(removedDie);
            }

            _db.SaveChanges();

            return removedDice;
        }

        public List<int> RemoveDiceFromEnd(int playerId, int numDice)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.PlayerId == playerId);
            var currentDiceCount = gamePlayer.CurrentGamePlayerRound.Dice.ToIntegerDice().Count();
            var removedDice = new List<int>();

            for (int i = 0; i < numDice; i++)
            {
                var indexToRemove = currentDiceCount - (i + 1);
                var removedDie = RemoveDie(gamePlayer, indexToRemove);
                removedDice.Add(removedDie);
            }

            _db.SaveChanges();

            return null;
        }

        public List<int> RemoveRandomDice(int playerId, int numDice)
        {
            var gamePlayer = _game.GamePlayers.Single(x => x.PlayerId == playerId);
            var removedDice = new List<int>();

            for (int i = 0; i < numDice; i++)
            {
                var indexToRemove = _random.Next(0, 99);
                var removedDie = RemoveDie(gamePlayer, indexToRemove);
                removedDice.Add(removedDie);
            }

            _db.SaveChanges();

            return removedDice;
        }

        private void AddDie(GamePlayer gamePlayer, int face, bool isMystery = false)
        {
            var diceIntegers = gamePlayer.CurrentGamePlayerRound.Dice.ToIntegerDice();
            var dieString = isMystery ? $"{face}?" : $"{face}";

            if (diceIntegers.Count == 0)
            {
                gamePlayer.CurrentGamePlayerRound.Dice = dieString;
                return;
            }

            int spotToInsert;
            for (spotToInsert = 0; spotToInsert < diceIntegers.Count; spotToInsert++)
            {
                if (diceIntegers[spotToInsert] >= face) break;
            }

            var diceStrings = gamePlayer.CurrentGamePlayerRound.Dice.Split(',').ToList();
            diceStrings.Insert(spotToInsert, dieString);

            gamePlayer.CurrentGamePlayerRound.Dice = string.Join(",", diceStrings);
        }

        private int RemoveDie(GamePlayer gamePlayer, int index)
        {
            var diceIntegers = gamePlayer.CurrentGamePlayerRound.Dice.ToIntegerDice();
            var diceStrings = gamePlayer.CurrentGamePlayerRound.Dice.Split(',').ToList();

            diceStrings.RemoveAt(index % diceStrings.Count);
            gamePlayer.CurrentGamePlayerRound.Dice = string.Join(",", diceStrings);

            return diceIntegers[index % diceIntegers.Count];
        }

        public Bid GetPreviousBid()
        {
            return _game.CurrentRound
                .Actions.OfType<Bid>()
                .LastOrDefault();
        }

        public string GetMetadata(string key)
        {
            return _game.GetMetadata(key);
        }

        public void SetMetadata(string key, string value)
        {
            _game.SetMetadata(key, value);
        }
    }
}