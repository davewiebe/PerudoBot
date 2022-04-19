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
        public int Cost { get; set; } = 0;
        public bool OutOfTurn { get; set; } = false;
        public int MinPlayers { get; set; } = 2;
        public int MinDice { get; set; } = 1;
        public int UsesPerRound { get; set; } = 1;
        public int UsesPerGame { get; set; } = 99;
    }

    public static class PowerUps
    {
        public static int LIFETAP_AMOUNT = 60;
        public static int CLAIM_AMOUNT = 30;
        public static int CLAIM_THRESHOLD = 200;

        public static PowerUp Lifetap = new PowerUp
        {
            Name = "Lifetap",
            Description = "Permanently lose a life to gain points",
            MinPlayers = 3,
            OutOfTurn = true
        };

        public static PowerUp Claim = new PowerUp
        {
            Name = "Claim",
            Description = "Claim free points once per game",
            UsesPerGame = 1,
            OutOfTurn = true
        };

        public static PowerUp Gamble = new PowerUp
        {
            Name = "Gamble",
            Description = "Transform your dice unpredictably",
            Cost = 10,
            MinPlayers = 3,
            MinDice = 2,
            OutOfTurn = true
        };

        public static PowerUp Steal = new PowerUp
        {
            Name = "Steal",
            Description = "Steal 2-3 dice from a target player, new dice are mystery",
            Cost = 10,
            MinPlayers = 3,
        };

        public static PowerUp Promote = new PowerUp
        {
            Name = "Promote",
            Description = "Reroll 1-3 of your smallest dice, half of the new dice are mystery",
            Cost = 10,
            MinPlayers = 3,
            MinDice = 2
        };

        public static PowerUp Charm = new PowerUp
        {
            Name = "Charm",
            Description = "Charm a random player with a copy of 1-2 dice from your hand, new dice are mystery",
            Cost = 10,
            MinDice = 3,
            MinPlayers = 3
        };

        public static PowerUp Recombobulate = new PowerUp
        {
            Name = "Recombobulate",
            Description = "Reroll your hand, each player gets one of your old dice as a mystery die",
            UsesPerGame = 1,
            MinPlayers = 5,
            MinDice = 5
        };

        public static List<PowerUp> PowerUpList = new List<PowerUp>
        {
             Gamble, Steal, Charm, Promote, Recombobulate, Lifetap, Claim
        };
    }
}
