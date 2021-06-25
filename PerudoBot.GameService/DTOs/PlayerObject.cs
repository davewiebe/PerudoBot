using System;

namespace PerudoBot.GameService
{
    public class PlayerObject
    {
        public int NumberOfDice { get; internal set; }
        public bool IsBot { get; internal set; }
        public string Name { get; internal set; }
        public ulong UserId { get; internal set; }

        public PlayerObject()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerObject other &&
                   NumberOfDice == other.NumberOfDice &&
                   IsBot == other.IsBot &&
                   Name == other.Name &&
                   UserId == other.UserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NumberOfDice, IsBot, Name, UserId);
        }
    }
}