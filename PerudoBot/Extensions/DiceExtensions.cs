using PerudoBot.GameService;
using System.Linq;

namespace PerudoBot.Extensions
{
    public static class DiceExtensions
    {
        public static string ToEmoji(this int die)
        {
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
    }
}
