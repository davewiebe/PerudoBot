using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("liar")]
        public async Task Liar()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            var currentPlayer = game.GetCurrentPlayer();

            var userId = GetUserId(currentPlayer);
            if (Context.User.Id != userId) return;

            var liarResult = game.Liar();

            DeleteCommandFromDiscord();

            await SendMessageAsync($"{liarResult.PlayerWhoCalledLiar.Name} called **liar** on `{liarResult.BidQuantity}` ˣ {liarResult.BidPips.ToEmoji()}.");

            // for dramatic effect
            Thread.Sleep(3000);

            await SendMessageAsync($"There was actually `{liarResult.ActualQuantity}` dice. :fire: {liarResult.PlayerWhoLostDice.GetMention()} loses {liarResult.DiceLost} dice. :fire:");

            await SendRoundSummary();

            await StartNewRound(game);
        }

        private async Task SendRoundSummary()
        {
            var game = _gameHandler.GetActiveGame();

            var players = game.GetPlayerDice().OrderBy(x => x.TurnOrder);
            var playerDice = players.Select(x => $"{x.Name}: {string.Join(" ", x.Dice.Split(",").Select(x => int.Parse(x).ToEmoji()))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            var allDiceGrouped = allDice
                .GroupBy(x => x)
                .OrderBy(x => x.Key);

            var countOfOnes = allDiceGrouped.SingleOrDefault(x => x.Key == 1)?.Count();

            var listOfAllDiceCounts = allDiceGrouped.Select(x => $"`{x.Count()}` ˣ {x.Key.ToEmoji()}");

            var totals = new List<string>();
            for (int i = 1; i <= 6; i++)
            {
                var countOfX = allDiceGrouped.SingleOrDefault(x => x.Key == i)?.Count();
                var count1 = countOfOnes ?? 0;
                if (i == 1) count1 = 0;
                var countX = countOfX ?? 0;
                totals.Add($"`{count1 + countX }` ˣ {i.ToEmoji()}");
            }

            var builder = new EmbedBuilder()
                .WithTitle($"Round {game.GetCurrentRoundNumber()} Summary")
                .AddField("Players", $"{string.Join("\n", playerDice)}", inline: true)
                .AddField("Dice", $"{string.Join("\n", listOfAllDiceCounts)}", inline: true);

            builder.AddField("Totals", $"{string.Join("\n", totals)}", inline: true);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}