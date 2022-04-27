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

        [ForeignKey("GamePlayerId")]
        public virtual GamePlayer GamePlayer { get; set; }

        public int? GamePlayerId { get; set; }

        [ForeignKey("GamePlayerRoundId")]
        public virtual GamePlayerRound GamePlayerRound { get; set; }

        public int? GamePlayerRoundId { get; set; }

        public int RoundId { get; set; }
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

    public class Bet : Action
    {
        public int BettingPlayerId { get; set; }

        [ForeignKey("BettingPlayerId")]
        public virtual Player BettingPlayer { get; set; }

        public int TargetActionId { get; set; }

        [ForeignKey("TargetActionId")]
        public virtual Action TargetAction { get; set; }

        public int BetAmount { get; set; }
        public string BetType { get; set; }

        public bool? IsSuccessful { get; set; }
    }
}