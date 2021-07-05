using System;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public class LiarResult
    {
        public bool IsSuccessful { get; set; }
        public int DiceLost { get; set; }
        public PlayerData PlayerWhoBidLast { get; set; }
        public PlayerData PlayerWhoCalledLiar { get; set; }
        public PlayerData PlayerWhoLostDice { get; set; }
        public int BidQuantity { get; internal set; }
        public int ActualQuantity { get; internal set; }
        public int BidPips { get; internal set; }

        public LiarResult()
        {
        }
    }
}