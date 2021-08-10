using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerudoBot.Database.Data
{
    public abstract class Round
    {
        public Round()
        {
        }

        public int Id { get; set; }
        public int GameId { get; set; }
        public int RoundNumber { get; set; }
        public virtual Game Game { get; set; }
        public virtual ICollection<Action> Actions { get; set; }
        public virtual ICollection<GamePlayerRound> GamePlayerRounds { get; set; }

        public int StartingPlayerId { get; set; } // GamePlayerId

        //Discriminator Column
        public string RoundType { get; private set; }

        public Action LatestAction => Actions.LastOrDefault();

    }

    public class StandardRound : Round
    {
    }

    public class FaceoffRound : Round
    {
    }
}