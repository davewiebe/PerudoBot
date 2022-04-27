using Discord.Commands;
using PerudoBot.Database.Data;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System.Linq;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("bet")]
        public async Task Bet(params string[] betText)
        {
            if (betText == null || betText.Length < 2) return;

            var betType = betText[0].ToLower();
            if (betType != BetType.Exact && betType != BetType.Liar) return;

            if (!int.TryParse(betText[1], out int betAmount)) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var targetAction = game.GetPreviousBid();
            if (targetAction == null) return;

            var bettingPlayer = _gameHandler
                .CreateAndGetDiscordPlayer(Context.User.Id, Context.User.Username, Context.User.IsBot)
                .Player;

            if (targetAction.GamePlayer.PlayerId == bettingPlayer.Id)
            {
                await SendMessageAsync($"You can't bet on your own bid.");
                return;
            }

            var existingBetsAmount = game
                .GetCurrentRound()
                .Actions.OfType<Bet>()
                .Where(x => x.BettingPlayerId == bettingPlayer.Id)
                .Sum(x => x.BetAmount);

            if (GetAvailablePoints(bettingPlayer.Id) < (betAmount + existingBetsAmount))
            {
                await SendMessageAsync($"You don't have enough points to place this bet.");
                return;
            };

            DeleteCommandFromDiscord();
            game.BetOnLatestAction(bettingPlayer, betAmount, betType);

            var typeString = (betType == BetType.Liar) ? "a lie" : "exact";
            await SendMessageAsync($":dollar: {bettingPlayer.Name} bets that `{targetAction.Quantity}` ˣ {targetAction.Pips.ToEmoji()} is {typeString}.");
        }
    }
}
