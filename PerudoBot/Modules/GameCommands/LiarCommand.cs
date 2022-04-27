using Discord;
using Discord.Commands;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System;
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

            var playerWhoLastSentMessage = GetPlayerId(Context.User.Id, Context.Guild.Id);
            var currentPlayer = game.GetCurrentPlayer();
            var playerWhoCalledLiar = 0;
            
            if(game.IsPlayerAutoLiar(currentPlayer.PlayerId) == true)
            {
                playerWhoCalledLiar = currentPlayer.PlayerId;
            }
            else
            {
                playerWhoCalledLiar = playerWhoLastSentMessage;
            }

            var roundResult = game.Liar(playerWhoCalledLiar);
            if (roundResult == null) return;

            var liarResult = roundResult.LiarResult;

            DeleteCommandFromDiscord();

            await SendMessageAsync($"{liarResult.PlayerWhoCalledLiar.Name} called **liar** on `{liarResult.BidQuantity}` ˣ {liarResult.BidPips.ToEmoji()}.");

            // for dramatic effect
            Thread.Sleep(3000);

            var losesOrGains = liarResult.DiceLost > 0 ? "loses" : "gains"; 
            await SendMessageAsync($"There was actually `{liarResult.ActualQuantity}` dice. :fire: {liarResult.PlayerWhoLostDice.GetMention(_db)} {losesOrGains} {Math.Abs(liarResult.DiceLost)} dice. :fire:");
            
            if (liarResult.ActualQuantity == liarResult.BidQuantity)
            {
                var tauntRattle = liarResult.PlayerWhoBidLast.GetPlayerMetadata("tauntrattle");
                if (tauntRattle != null)
                {
                    if (tauntRattle.ToLower().StartsWith("!gif "))
                    {
                        await SendTempMessageAsync(tauntRattle);
                        Thread.Sleep(1500);
                    }
                    else await SendMessageAsync(tauntRattle);
                }
            }

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

            await SendBetResuls(roundResult.BetResults);
            await SendRoundSummary();

            await StartNewRound(game);
        }

        private async Task SendBetResuls(List<BetResult> betResults)
        {
            foreach (var betResult in betResults)
            {
                var pointsUsed = betResult.BetAmount;
                var pointsGained = betResult.IsSuccessful ? pointsUsed * 2 : 0;
                if (betResult.BetType == BetType.Exact) pointsGained *= 2;

                AddUsedPoints(betResult.BettingPlayer.Id, pointsUsed);
                AddTotalPoints(betResult.BettingPlayer.Id, pointsGained);

                var winsOrLoses = betResult.IsSuccessful ? "wins" : "loses";
                var pointChange = Math.Abs(pointsGained - pointsUsed);
                await SendMessageAsync($":dollar: {betResult.BettingPlayer.Name} **{winsOrLoses}** {pointChange} points betting {betResult.BetType.ToLower()} on `{betResult.BetQuantity}` ˣ {betResult.BetPips.ToEmoji()}.");
            }
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