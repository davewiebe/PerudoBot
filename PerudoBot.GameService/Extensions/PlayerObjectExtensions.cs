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

        public static ulong GetDiscordId(this PlayerData playerObject, Database.Data.PerudoBotDbContext _db)
        {
            return _db.DiscordPlayers.Single(x => x.PlayerId == playerObject.PlayerId).UserId;
        }

        public static string GetBotKey(this PlayerData playerObject, Database.Data.PerudoBotDbContext _db)
        {
            var discordPlayer = _db.DiscordPlayers.Single(x => x.PlayerId == playerObject.PlayerId);
            var key = discordPlayer.BotKey;

            if (key == null)
            {
                return discordPlayer.Player.Name; 
            } 
            else
            {
                return key;
            }
        }
    }
}
