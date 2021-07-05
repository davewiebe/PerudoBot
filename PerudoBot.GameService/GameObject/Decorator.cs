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

        public virtual void Start()
        {
            _game.Start();
        }

        public virtual bool Bid(int quantity, int pips)
        {
            return _game.Bid(quantity, pips);
        }
        
        public virtual PlayerData GetCurrentPlayer()
        {
            return _game.GetCurrentPlayer();
        }

        public virtual int GetCurrentRoundNumber()
        {
            return _game.GetCurrentRoundNumber();
        }

        public virtual bool BidReverse(int quantity, int pips)
        {
            return _game.BidReverse(quantity, pips);
        }

        public virtual List<PlayerData> GetPlayerDice()
        {
            return _game.GetPlayerDice();
        }
        public virtual LiarResult Liar()
        {
            return _game.Liar();
        }
        public virtual bool CreateGame(ulong guildId)
        {
            return _game.CreateGame(guildId);
        }
        public virtual bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot)
        {
            return _game.AddPlayer(userId, guildId, name, isBot);
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
    }
}