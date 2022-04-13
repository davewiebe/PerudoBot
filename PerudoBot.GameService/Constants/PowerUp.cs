using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerudoBot.GameService.Constants
{
    public class PowerUp
    {
        public string Name { get; set; }
        public int MinPlayers { get; set; }
        public bool OutOfTurn { get; set; }
    }

    public static class PowerUps
    {
        public static PowerUp Swap = new PowerUp
        {
            Name = "Swap",
            MinPlayers = 3,
            OutOfTurn = false
        };

        public static PowerUp Gamble = new PowerUp
        {
            Name = "Gamble",
            MinPlayers = 3,
            OutOfTurn = false
        };

        public static PowerUp Claim = new PowerUp
        {
            Name = "Claim",
            MinPlayers = 2,
            OutOfTurn = true
        };
    }
}
