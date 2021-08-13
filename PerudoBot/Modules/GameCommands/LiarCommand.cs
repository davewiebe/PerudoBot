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
        [Alias("iiar")]
        public async Task Liar()
        {
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            var playerId = GetPlayerId(Context.User.Id, Context.Guild.Id);

            var liarResult = game.Liar(playerId);
            if (liarResult == null) return;

            DeleteCommandFromDiscord();

            await SendMessageAsync($"{liarResult.PlayerWhoCalledLiar.Name} called **liar** on `{liarResult.BidQuantity}` ˣ {liarResult.BidPips.ToEmoji()}.");

            // for dramatic effect
            Thread.Sleep(3000);

            await SendMessageAsync($"There was actually `{liarResult.ActualQuantity}` dice. :fire: {liarResult.PlayerWhoLostDice.GetMention(_db)} loses {liarResult.DiceLost} dice. :fire:");

            if (liarResult.PlayerWhoLostDice.IsEliminated)
            {
                await SendMessageAsync($":fire::skull::fire: {liarResult.PlayerWhoLostDice.Name} defeated :fire::skull::fire:");

                var deathRattle = liarResult.PlayerWhoLostDice.GetPlayerMetadata("deathrattle");
                if (deathRattle != null)
                {
                    if (deathRattle.ToLower().StartsWith("!gif "))
                    {
                        await SendTempMessageAsync(deathRattle);
                        Thread.Sleep(1500);
                    }
                    else await SendMessageAsync(deathRattle);
                }
            }

            await SendRoundSummary();

            await StartNewRound(game);
        }

        private async Task SendRoundSummary()
        {
            var game = _gameHandler.GetActiveGame();

            var players = game.GetAllPlayers()
                .Where(x => x.Dice.Count > 0)
                .OrderBy(x => x.TurnOrder);

            var playerDice = players.Select(x => $"{x.Name}: {string.Join(" ", x.Dice.Select(x => x.ToEmoji()))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice);
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