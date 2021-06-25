using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using System;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("bidr")]
        [Alias("br", "dib", "rbid", "rb")]
        public async Task BidReverse(params string[] bidText)
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            var currentPlayer = game.GetCurrentPlayer();

            if (Context.User.Id != currentPlayer.UserId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            try
            {
                var result = game.BidReverse(quantity, pips);
                if (result == false)
                {

                    await SendMessageAsync($"Cannot reverse bid after first bid");
                    return;
                }
                DeleteCommandFromDiscord();
                await SendMessageAsync($"Player order *REVERSED*");
                await SendMessageAsync($"{currentPlayer.Name} bids `{quantity}` ˣ { pips.ToEmoji() }. { game.GetCurrentPlayer().GetMention()} is up.");
            }
            catch (ArgumentOutOfRangeException e)
            {
                await SendMessageAsync($"{e.Message}");
                return;
            }

        }

        [Command("bid")]
        [Alias("b")]
        public async Task Bid(params string[] bidText)
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            if (game == null) return;
            var currentPlayer = game.GetCurrentPlayer();

            if (Context.User.Id != currentPlayer.UserId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            try
            {
                game.Bid(quantity, pips);
            }
            catch (ArgumentOutOfRangeException e)
            {
                await SendMessageAsync($"{e.Message}");
                return;
            }
            DeleteCommandFromDiscord();
            await SendMessageAsync($"{currentPlayer.Name} bids `{quantity}` ˣ { pips.ToEmoji() }. { game.GetCurrentPlayer().GetMention()} is up.");
        }
    }
}