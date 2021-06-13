using PerudoBot.GameService;
using System.Linq;

namespace PerudoBot.Services
{
    public static class PlayerObjectExtensions
    {
        public static string GetMention(this PlayerObject playerObject)
        {
            return $"<@!{playerObject.UserId}>";
        }
    }
}
