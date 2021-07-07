using PerudoBot.GameService;
using System.Linq;

namespace PerudoBot.Extensions
{
    public static class PlayerObjectExtensions
    {
        public static string GetMention(this PlayerData playerObject, Database.Data.PerudoBotDbContext _db)
        {
           var userId = _db.DiscordPlayers.Single(x => x.PlayerId == playerObject.PlayerId).UserId;
            return $"<@!{userId}>";
        }
    }
}
