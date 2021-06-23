using System;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public class LiarResult
    {
        public bool IsSuccessful { get; set; }
        public int DiceLost { get; set; }
        public PlayerObject PlayerWhoBidLast { get; set; }
        public PlayerObject PlayerWhoCalledLiar { get; set; }
        public PlayerObject PlayerWhoLostDice { get; set; }
        public int BidQuantity { get; internal set; }
        public int ActualQuantity { get; internal set; }
        public int BidPips { get; internal set; }

        public LiarResult()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is LiarResult other &&
                   IsSuccessful == other.IsSuccessful &&
                   DiceLost == other.DiceLost &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoBidLast, other.PlayerWhoBidLast) &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoCalledLiar, other.PlayerWhoCalledLiar) &&
                   EqualityComparer<PlayerObject>.Default.Equals(PlayerWhoLostDice, other.PlayerWhoLostDice);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsSuccessful, DiceLost, PlayerWhoBidLast, PlayerWhoCalledLiar, PlayerWhoLostDice);
        }
    }
}