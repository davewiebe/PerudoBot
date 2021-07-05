using PerudoBot.GameService;
using System.Linq;

namespace PerudoBot.Extensions
{
    public static class PlayerObjectExtensions
    {
        public static string GetMention(this PlayerData playerObject)
        {
            return $"<@!{playerObject.UserId}>";
        }
    }
}
