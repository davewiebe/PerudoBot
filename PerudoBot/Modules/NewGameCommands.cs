using Discord;
using Discord.Commands;
using PerudoBot.GameService;
using PerudoBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewGameAsync(params string[] options)
        {
            await UpdateAvatar();

            var game = _gameHandler.CreateGame(Context.Channel.Id, Context.Guild.Id);

            var commands =
                $"`!add/remove @player` to add/remove players.\n" +
                $"`!start` to start the game.";

            var builder = new EmbedBuilder()
                            .WithTitle($"New game #{game.GetGameNumber()} created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

        }

        private async Task UpdateAvatar()
        {
            var fileStream = new FileStream(Directory.GetCurrentDirectory() + "/Avatars/newAvatar.jpg", FileMode.Open);
            var image = new Image(fileStream);
            await Context.Client.CurrentUser.ModifyAsync(u => u.Avatar = image);
        }

        [Command("add")]
        public async Task AddPlayer(params string[] users)
        {
            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            if (game == null)
            {
                await ReplyAsync("No game is being set up");
                return;
            }

            foreach (var mentionedUser in Context.Message.MentionedUsers)
            {
                game.AddPlayer(mentionedUser.Id, Context.Guild.Id, mentionedUser.Username, Context.Guild.GetUser(mentionedUser.Id)?.Nickname);
            }

            var listOfPlayers = game.GetPlayers();


            var builder = new EmbedBuilder()
                            .WithTitle($"Game set up")
                            .AddField($"Players", $"{string.Join("\n", listOfPlayers)}", inline: false);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("start")]
        public async Task StartGame()
        {
            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            await ReplyAsync($"Starting the game!\nUse `!bid 2 2s` or `!exact` or `!liar` to play.");

            game.Start();

            await StartNewRound(game);
        }

        private async Task StartNewRound(GameObject game)
        {
            var roundStatus = game.StartNewRound();

            if (roundStatus.IsActive == false)
            {
                await ReplyAsync($":trophy: {roundStatus.Winner.GetMention()} is the winner with `{roundStatus.Winner.NumberOfDice}` dice remaining! :trophy:");
                return;
            }

            await SendOutDice(roundStatus.PlayerDice);
            await ReplyAsync($"A new round has begun. {game.GetCurrentPlayer().GetMention()} goes first");
        }

        private async Task SendOutDice(List<PlayerDice> playerDice)
        {
            try
            {
                foreach (var player in playerDice)
                {
                    // send dice to each player
                    var message = $"Your dice: {player.Dice}";

                    var user = Context.Guild.Users.Single(x => x.Id == player.UserId);


                    var requestOptions = new RequestOptions()
                    { RetryMode = RetryMode.RetryRatelimit };
                    await user.SendMessageAsync(message, options: requestOptions);
                }
            }
            catch (Exception e)
            {
                var monkey = e.Message; // probably a bot
            }
        }

        [Command("bid")]
        public async Task Bid(params string[] bidText)
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

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
                await ReplyAsync($"{e.Message}");
                return;
            }

            await ReplyAsync($"{currentPlayer.GetMention()} bids `{quantity}` ˣ { pips }. { game.GetCurrentPlayer().GetMention()} is up.");
        }

        [Command("liar")]
        public async Task Liar()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            var currentPlayer = game.GetCurrentPlayer();

            if (Context.User.Id != currentPlayer.UserId) return;

            var liarResult = game.Liar();

            await ReplyAsync($"{liarResult.PlayerWhoCalledLiar.Name} called **liar** on `{liarResult.BidQuantity}` ˣ {liarResult.BidPips}.");

            // for the dramatic affect
            Thread.Sleep(4000);

            await ReplyAsync($"There was actually `{liarResult.ActualQuantity}` {liarResult.BidPips}. :fire: {liarResult.PlayerWhoLostDice.Name} loses {liarResult.DiceLost} dice. :fire:");

            await SendRoundSummary();

            await StartNewRound(game);
        }

        private async Task SendRoundSummary()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            game.GetPlayerDice();

            var players = game.GetPlayerDice();
            var playerDice = players.Select(x => $"{x.Name}: {string.Join(" ", x.Dice.Split(",").Select(x => int.Parse(x)))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            var allDiceGrouped = allDice
                .GroupBy(x => x)
                .OrderBy(x => x.Key);

            var countOfOnes = allDiceGrouped.SingleOrDefault(x => x.Key == 1)?.Count();

            var listOfAllDiceCounts = allDiceGrouped.Select(x => $"`{x.Count()}` ˣ {x.Key}");

            List<string> totals = new List<string>();
            for (int i = 1; i <= 6; i++)
            {
                var countOfX = allDiceGrouped.SingleOrDefault(x => x.Key == i)?.Count();
                var count1 = countOfOnes ?? 0;
                if (i == 1) count1 = 0;
                var countX = countOfX ?? 0;
                totals.Add($"`{count1 + countX }` ˣ {i}");
            }

            var builder = new EmbedBuilder()
                .WithTitle($"Round {game.GetCurrentRoundNumber()} Summary")
                .AddField("Players", $"{string.Join("\n", playerDice)}", inline: true)
                .AddField("Dice", $"{string.Join("\n", listOfAllDiceCounts)}", inline: true);

            //if (game.CurrentRound is StandardRound)
            builder.AddField("Totals", $"{string.Join("\n", totals)}", inline: true);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }
    }
}