using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PerudoBot.Database.Data
{
    public class GamePlayer : MetadataEntity
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        public virtual Player Player { get; set; }

        public int GameId { get; set; }

        public virtual Game Game { get; set; }

        //public virtual ICollection<Action> Actions { get; set; }
        public virtual ICollection<GamePlayerRound> GamePlayerRounds { get; set; }// = new List<GamePlayerRound>();

        public int NumberOfDice { get; set; } // Current Number of Dice
        public int TurnOrder { get; set; }
        public int Rank { get; set; }

        public GamePlayerRound CurrentGamePlayerRound
        {
            get
            {
                return GamePlayerRounds?.LastOrDefault();
            }
        }

    }
}