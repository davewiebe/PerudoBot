using PerudoBot.GameService;
using System.Linq;

namespace PerudoBot.Services
{
    public static class PlayerObjectExtensions
    {
        public static string GetMention(this PlayerObject playerObject, Discord.Commands.SocketCommandContext context)
        {
            var user = context.Guild.Users.SingleOrDefault(x => x.Id == playerObject.UserId);
            if (user == null) return playerObject.Name;

            return user.Mention;
        }
    }
}
