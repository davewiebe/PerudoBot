using Discord;
using Discord.Commands;
using Newtonsoft.Json;
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
            await ProcessBid(bidText, true);
        }

        [Command("bid")]
        [Alias("b")]
        public async Task Bid(params string[] bidText)
        {
            await ProcessBid(bidText, false);
        }

        private async Task ProcessBid(string[] bidText, bool isReverse)
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            var currentPlayer = game.GetCurrentPlayer();
            var userId = GetUserId(currentPlayer);

            if (Context.User.Id != userId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            var isValid = game.BidValidate(currentPlayer.PlayerId, quantity, pips);

            if (game.GetPreviousBid() != null && isReverse)
            {
                await SendMessageAsync($"Cannot reverse bid after first bid");
                return;
            }

            if (!isValid)
            {
                await SendMessageAsync("Invalid bid");
                return;
            }

            

            if (isReverse)
            {
                game.ReversePlayerOrder();
                await SendMessageAsync($"Player order *REVERSED*");
            }

            game.Bid(currentPlayer.PlayerId, quantity, pips);
            DeleteCommandFromDiscord();

            var nextPlayer = game.GetCurrentPlayer();

            await SendMessageAsync($"{currentPlayer.Name} bids `{quantity}` ˣ { pips.ToEmoji() }. { nextPlayer.GetMention(_db)} is up.");

            if (game.HasBots())
            {
                var botMessage = new
                {
                    nextPlayer = nextPlayer.GetDiscordId(_db),
                    diceCount = game.GetAllDice().Count,
                    round = game.GetCurrentRoundNumber(),
                    action = BidToActionIndex(quantity, pips),
                };

                await SendMessageAsync($"||`@bots update {JsonConvert.SerializeObject(botMessage)}`||");
            }
        }

        // Unwrap bid to it's action index where 0:1x2, 1:1x3, 2:1x4, etc.
        private int BidToActionIndex(int quantity, int pips)
        {
            if (pips != 1)
            {
                int nonWildcard = ((quantity - 1) * 5);
                int wildcard = quantity / 2;
                return nonWildcard + wildcard + (pips - 2);
            }
            else
            {
                // starting at 5, every 11 actions there is a wildcard action
                return 5 + ((quantity - 1) * 11);
            }
        }
    }
}