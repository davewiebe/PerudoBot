using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PerudoBot.Database.Data
{
    public abstract class Action
    {
        public Action()
        {
        }

        public int Id { get; set; }

        public virtual GamePlayer GamePlayer { get; set; }

        public int? GamePlayerId { get; set; }

        public virtual GamePlayerRound GamePlayerRound { get; set; }

        public int GamePlayerRoundId { get; set; }

        public int? RoundId { get; set; }
        public virtual Round Round { get; set; }

        public int? ParentActionId { get; set; }

        [ForeignKey("ParentActionId")]
        public virtual Action ParentAction { get; set; }

    }

    public class Bid : Action
    {
        public int Quantity { get; set; }
        public int Pips { get; set; }
    }

    public class LiarCall : Action
    {
    }

    public class ExactCall : Action
    {
    }
}