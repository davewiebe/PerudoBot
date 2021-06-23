using System;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public class RoundStatus
    {
        public bool IsActive { get; set; }
        public List<PlayerDice> PlayerDice { get; set; }
        public PlayerObject Winner { get; set; }
        public int RoundNumber { get; internal set; }

        public RoundStatus()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is RoundStatus other &&
                   IsActive == other.IsActive &&
                   EqualityComparer<List<PlayerDice>>.Default.Equals(PlayerDice, other.PlayerDice) &&
                   EqualityComparer<PlayerObject>.Default.Equals(Winner, other.Winner);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsActive, PlayerDice, Winner);
        }
    }
}