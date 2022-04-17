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
        public string Description { get; set; }
        public int EnergyCost { get; set; } = 0;
        public bool OutOfTurn { get; set; } = false;
        public int MinPlayers { get; set; } = 2;
        public int MinDice { get; set; } = 2;
        public int UsesPerGame { get; set; } = 99;

        //public int UsesPerRound { get; set; } = 99;
    }

    public static class PowerUps
    {
        public static PowerUp Lifetap = new PowerUp
        {
            Name = "Lifetap",
            Description = "Permanently lose a life to gain 8-12 energy up to a maximum",
            MinPlayers = 3,
            OutOfTurn = true
        };

        public static PowerUp Gamble = new PowerUp
        {
            Name = "Gamble",
            Description = "Transform your dice unpredictably",
            EnergyCost = 2,
            MinPlayers = 3,
            MinDice = 2,
            OutOfTurn = true
        };

        public static PowerUp Steal = new PowerUp
        {
            Name = "Steal",
            Description = "Steal 2-3 dice from a random player, new dice are mystery",
            EnergyCost = 2,
            MinPlayers = 3,
        };

        public static PowerUp Minify = new PowerUp
        {
            Name = "Minify",
            Description = "Reroll 1-3 of your right-most dice, half of the new dice are mystery",
            EnergyCost = 3,
            MinPlayers = 3
        };

        public static PowerUp Magnify = new PowerUp
        {
            Name = "Magnify",
            Description = "Reroll 1-3 of your left-most dice, half of the new dice are mystery",
            EnergyCost = 3,
            MinPlayers = 3
        };

        public static PowerUp Reforge = new PowerUp
        {
            Name = "Reforge",
            Description = "Reroll your hand, each player gets one of your old dice as a mystery die",
            UsesPerGame = 1,
            MinPlayers = 5,
            MinDice = 5
        };
    }
}
