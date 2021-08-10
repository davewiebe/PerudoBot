using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.Database.Data
{
    public class GamePlayerRound
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public virtual Round Round { get; set; }

        public int GamePlayerId { get; set; }
        public virtual GamePlayer GamePlayer { get; set; }

        public virtual ICollection<Action> Actions { get; set; }

        public string Dice { get; set; }
    }
}