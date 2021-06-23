using System;

namespace PerudoBot.GameService
{
    public class PlayerDice
    {
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public string Dice { get; set; }
        public bool IsBot { get; set; }
        public int NumberOfDice { get; internal set; }
        public int TurnOrder { get; internal set; }

        public PlayerDice()
        {
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerDice other &&
                   Name == other.Name &&
                   UserId == other.UserId &&
                   Dice == other.Dice;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, UserId, Dice);
        }
    }
}