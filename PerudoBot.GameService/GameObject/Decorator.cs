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

        public virtual bool Bid(int playerId, int quantity, int pips)
        {
            return _game.Bid(playerId, quantity, pips);
        }

        public virtual PlayerData GetCurrentPlayer()
        {
            return _game.GetCurrentPlayer();
        }

        public virtual int GetCurrentRoundNumber()
        {
            return _game.GetCurrentRoundNumber();
        }
        public virtual bool BidReverse(int playerId, int quantity, int pips)
        {
            return _game.BidReverse(playerId, quantity, pips);
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
    }
}