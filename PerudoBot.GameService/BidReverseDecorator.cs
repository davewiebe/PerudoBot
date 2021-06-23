using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.GameService
{

    public abstract class GameObjectDecorator : IGameObject
    {
        private IGameObject _gameObject;
        public GameObjectDecorator(IGameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public virtual void Bid(int quantity, int pips)
        {
            _gameObject.Bid(quantity, pips);
        }

    }
    public class BidReverseDecorator : GameObjectDecorator
    {
        public BidReverseDecorator(IGameObject gameObject) : base(gameObject)
        { 
        }

        public void BidReverse(int quantity, int pips)
        {

            //var game = _db.Games
            //   .Include(x => x.Rounds)
            //   .ThenInclude(x => x.Actions)
            //   .Include(x => x.GamePlayers)
            //   .ThenInclude(x => x.GamePlayerRounds)
            //   .Single(x => x.Id == _gameId);


            //var previousBid = game.CurrentRound
            //    .Actions.OfType<Bid>()
            //    .LastOrDefault();

            //if (previousBid != null)
            //{
            //}
            base.Bid(quantity, pips);
        }
        //public override void Bid(int quantity, int pips)
        //{

        //}
    }
}
