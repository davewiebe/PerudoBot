using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.Database.Data
{
    public class GamePlayerRound
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public Round Round { get; set; }

        public int GamePlayerId { get; set; }
        public GamePlayer GamePlayer { get; set; }

        public ICollection<Action> Actions { get; set; }

        public int NumberOfDice { get; set; }
        public string Dice { get; set; }

        public int TurnOrder { get; set; }

        public bool IsEliminated { get; set; }

    }
}