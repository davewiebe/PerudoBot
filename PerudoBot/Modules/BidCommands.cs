using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("bidr")]
        [Alias("br", "dib", "rbid", "rb")]
        public async Task BidReverse(params string[] bidText)
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            var currentPlayer = game.GetCurrentPlayer();
            ulong userId = GetUserId(currentPlayer);

            if (Context.User.Id != userId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            try
            {
                var result = game.BidReverse(currentPlayer.PlayerId, quantity, pips);
                if (result == false)
                {

                    await SendMessageAsync($"Cannot reverse bid after first bid");
                    return;
                }
                DeleteCommandFromDiscord();
                await SendMessageAsync($"Player order *REVERSED*");
                await SendMessageAsync($"{currentPlayer.Name} bids `{quantity}` ˣ { pips.ToEmoji() }. { game.GetCurrentPlayer().GetMention(_db)} is up.");
            }
            catch (ArgumentOutOfRangeException e)
            {
                await SendMessageAsync($"{e.Message}");
                return;
            }

        }

        private ulong GetUserId(PlayerData currentPlayer)
        {
            return _db.Players
                .Single(x => x.Id == currentPlayer.PlayerId)
                .DiscordPlayer.UserId;
        }

        [Command("bid")]
        [Alias("b")]
        public async Task Bid(params string[] bidText)
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            //if (game == null) return;
            var currentPlayer = game.GetCurrentPlayer();
            var userId = GetUserId(currentPlayer);
            if (Context.User.Id != userId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            try
            {
                game.Bid(currentPlayer.PlayerId, quantity, pips);
            }
            catch (ArgumentOutOfRangeException e)
            {
                await SendMessageAsync($"{e.Message}");
                return;
            }
            DeleteCommandFromDiscord();
            await SendMessageAsync($"{currentPlayer.Name} bids `{quantity}` ˣ { pips.ToEmoji() }. { game.GetCurrentPlayer().GetMention(_db)} is up.");
        }
    }
}