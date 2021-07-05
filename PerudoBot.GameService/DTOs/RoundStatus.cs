using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.GameService
{
    public class RoundStatus
    {
        public bool IsActive { get; set; }
        public List<PlayerData> Players { get; set; }
        public PlayerData Winner { get; set; }
        public int RoundNumber { get; internal set; }

        public RoundStatus()
        {
        }

        public List<PlayerData> ActivePlayers => Players.Where(x => !x.IsEliminated).ToList();
    }
}