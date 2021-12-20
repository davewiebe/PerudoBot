using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("autoliar")]
        [Alias("auto")]
        public async Task AutoLiar(params string[] bidText)
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            var playerId = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var player = game.GetPlayer(playerId);

            if (player.IsEliminated)
            {
                return;
            }

            if (player == null)
                return;

            if(playerId == game.GetCurrentPlayer().PlayerId)
            {
                await SendMessageAsync($"Cannot lock in a liar call on your own turn.");
                return;
            }

            if(game.IsPlayerAutoLiar(playerId) == true)
            {
                await SendMessageAsync($"You already have a liar call locked in.");
                return;
            }

            DeleteCommandFromDiscord(Context.Message.Id);

            game.ApplyAutoLiarToPlayer(playerId);

            await SendMessageAsync($":egg: {player.Name} has locked in a **liar** call.");

            _db.SaveChanges();
        }
    }
}