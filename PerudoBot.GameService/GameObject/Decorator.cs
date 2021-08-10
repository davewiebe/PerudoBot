using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.GameService.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.GameService
{
    public abstract class Decorator : IGameObject
    {
        protected IGameObject _game;

        public ulong BotUpdateMessageId {
            get { return _game.BotUpdateMessageId; }
            set { _game.BotUpdateMessageId = value; }
        }

        public Decorator(IGameObject game)
        {
            _game = game;
        }

        public virtual void OnEndOfRound()
        {
            _game.OnEndOfRound();
        }

        public virtual void OnEndOfGame()
        {
            _game.OnEndOfGame();
        }

        public virtual void Bid(int playerId, int quantity, int pips)
        {
            _game.Bid(playerId, quantity, pips);
        }

        public virtual PlayerData GetCurrentPlayer()
        {
            return _game.GetCurrentPlayer();
        }

        public virtual int GetCurrentRoundNumber()
        {
            return _game.GetCurrentRoundNumber();
        }

        public virtual List<PlayerData> GetAllPlayers()
        {
            return _game.GetAllPlayers();
        }
        public virtual LiarResult Liar(int playerId)
        {
            return _game.Liar(playerId);
        }
        public virtual bool CreateGame()
        {
            return _game.CreateGame();
        }
        public virtual bool AddPlayer(int playerId, string name)
        {
            return _game.AddPlayer(playerId, name);
        }
        public virtual bool SetModeSuddenDeath()
        {
            return _game.SetModeSuddenDeath();
        }
        public virtual bool SetModeVariable()
        {
            return _game.SetModeVariable();
        }
        public virtual RoundStatus StartNewRound()
        {
            return _game.StartNewRound();
        }

        public void ShufflePlayers()
        {
            _game.ShufflePlayers();
        }

        public string GetGameMode()
        {
            return _game.GetGameMode();
        }

        public bool HasBots()
        {
            return _game.HasBots();
        }

        public List<int> GetAllDice()
        {
            return _game.GetAllDice();
        }

        public bool BidValidate(int playerId, int quanity, int pips)
        {
            return _game.BidValidate(playerId, quanity, pips);
        }

        public Bid GetPreviousBid()
        {
            return _game.GetPreviousBid();
        }

        public void ReversePlayerOrder()
        {
            _game.ReversePlayerOrder();
        }
    }
}