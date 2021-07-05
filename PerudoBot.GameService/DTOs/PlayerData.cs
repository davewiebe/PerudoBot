using System;

namespace PerudoBot.GameService
{
    public class PlayerData
    {
        public string Name { get; set; }
        public ulong UserId { get; set; }
        public string Dice { get; set; }
        public int NumberOfDice { get; internal set; }
        public int TurnOrder { get; internal set; }
        public int Rank { get; internal set; }
        public bool IsBot { get; set; }

        public PlayerData()
        {
        }

        public bool IsEliminated => NumberOfDice == 0;
    }
}