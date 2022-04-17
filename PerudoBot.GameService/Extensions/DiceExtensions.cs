using PerudoBot.GameService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerudoBot.Extensions
{
    public static class DiceExtensions
    {
        public static string ToEmoji(this int die)
        {
            if (die == 0) return ":grey_question:";
            if (die == 1) return ":one:";
            if (die == 2) return ":two:";
            if (die == 3) return ":three:";
            if (die == 4) return ":four:";
            if (die == 5) return ":five:";
            if (die == 6) return ":six:";
            if (die == 7) return ":seven:";
            if (die == 8) return ":eight:";
            if (die == 9) return ":nine:";
            return die.ToString();
        }

        public static List<int> ToIntegerDice(this string dice)
        {
            if (string.IsNullOrEmpty(dice)) return new List<int>();
            return dice
                .Replace("?", "")
                .Split(",")
                .Select(x => int.Parse(x))
                .ToList();
        }

        public static List<int> GetHiddenDiceIndeces(this string dice)
        {
            if (string.IsNullOrEmpty(dice)) return new List<int>();
            var indexList = new List<int>();
            var diceList = dice.Split(',');
            for (int i = 0; i < diceList.Length; i++)
            {
                if (diceList[i].Contains('?')) indexList.Add(i);
            }
            return indexList;
        }
    }
}
