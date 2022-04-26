using Discord.Commands;
using PerudoBot.GameService;
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
            if (betType != "exact" && betType != "liar") return;

            if (!int.TryParse(betText[1], out int betAmount)) return;

            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();
            if (game == null) return;

            var bettinglayerId = GetPlayerId(Context.User.Id, Context.Guild.Id);

            if (GetAvailablePoints(bettinglayerId) < betAmount)
            {
                await SendMessageAsync($"You don't have enough points to place this bet.");
                return;
            };




        }

        private void RegisterBet(GameObject game, int playerId, string usesKey)
        {
            //var metaDataKey = $"{playerId}-{usesKey}-uses";

            //var numUsedString = game.GetMetadata(metaDataKey);

            //if (string.IsNullOrEmpty(numUsedString))
            //{
            //    game.SetMetadata(metaDataKey, "0");
            //    return 0;
            //}

            //return int.Parse(numUsedString);
        }
    }
}
