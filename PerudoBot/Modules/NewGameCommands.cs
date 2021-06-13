using Discord;
using Discord.Commands;
using PerudoBot.GameService;
using PerudoBot.Extensions;
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
        [Command("version")]
        public async Task Version(params string[] options)
        {
            var commands =
                $"`!exact` not implemented\n" +
                $"`!liar (out of turn)` not implemented\n" +
                $"`!deal` not implemented\n" +
                $"`!rattles` not implemented\n" +
                $"`!bid on 1s` not implemented\n" +
                $"`Palifico rounds` not implemented\n" +
                $"`!note` not implemented\n" +
                $"`!elo` not implemented\n\n" +
                "*who says you have to have new features to be 2.0?*";

            var builder = new EmbedBuilder()
                            .WithTitle($"PerudoBot2.0 - Version 2.0")
                            .AddField("Less features like...", commands, inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("bug")]
        public async Task Bug(params string[] options)
        {
            
            var responses = new List<string>();
            responses.Add("It's not a bug, it's a feature!");
            responses.Add("I think it's actually Discord that's having issues.");
            responses.Add("We don't know if that's actually a bug yet...");
            responses.Add("I think this one just comes down to user error.");
            responses.Add("Wow. Shots fired.");
            responses.Add($"I can track who's submitting these. Ready for some garbage hands {Context.User.Username}?.");

            await ReplyAsync(responses.OrderBy(x => Guid.NewGuid()).First());
        }

        [Command("option")]
        [Alias("options")]
        public async Task Options(params string[] options)
        {
            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            if (options.Any(x => x.ToLower() == "suddendeath"))
            {
                game.SetModeSuddenDeath();
            }
            else if (options.Any(x => x.ToLower() == "variable"))
            {
                game.SetModeVariable();
            }

            await DisplaySetupGamePlayers(game);
        }

        [Command("new")]
        public async Task NewGameAsync(params string[] options)
        {
            await UpdateAvatar("gamestart.png");

            GameObject game;

            if (DateTime.Now.Hour < 12) game = _gameHandler.CreateSuddenDeathGame(Context.Channel.Id, Context.Guild.Id);
            else game = _gameHandler.CreateVariableGame(Context.Channel.Id, Context.Guild.Id);

            var commands =
                $"`!add @player`\n" +
                $"`!start`\n" +
                $"`!option suddendeath\\variable` to change game modes";


            var builder = new EmbedBuilder()
                            .WithTitle($"New game created")
                            .AddField("Commands", commands, inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            await DisplaySetupGamePlayers(game);

        }

        private async Task UpdateAvatar(string avatarName)
        {
            try
            {
                var fileStream = new FileStream(Directory.GetCurrentDirectory() + $"/Avatars/{avatarName}", FileMode.Open);
                var image = new Image(fileStream);
                await Context.Client.CurrentUser.ModifyAsync(u => u.Avatar = image);
            }
            catch{ }
        }

        [Command("add")]
        [Alias("a")]
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

            if (Context.Message.MentionedUsers.Count == 0)
            {
                game.AddPlayer(Context.User.Id, Context.Guild.Id, Context.User.Username, Context.Guild.GetUser(Context.User.Id)?.Nickname);
            }


            await DisplaySetupGamePlayers(game);
        }

        private async Task DisplaySetupGamePlayers(GameObject game)
        {
            var listOfPlayers = game.GetPlayers();

            var gameType = "Sudden Death";
            if (game.GetMode() == GameMode.Variable) gameType = "Variable";


            var playerListText = string.Join("\n", listOfPlayers);
            if (playerListText == "") playerListText = "No players yet";

            var builder = new EmbedBuilder()
                            .WithTitle($"{gameType} game set up")
                            .AddField($"Players", $"{playerListText}", inline: false);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("terminate")]
        public async Task Terminate()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);
            game.Terminate();
            await ReplyAsync("Game Terminated");
        }

        [Command("start")]
        public async Task StartGame()
        {
            await UpdateAvatar("wink.png");

            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            await ReplyAsync($"Starting the game!\nUse `!bid 2 2s` or `!liar` to play.");

            game.Start();

            await StartNewRound(game);
        }

        private async Task StartNewRound(GameObject game)
        {
             var roundStatus = game.StartNewRound();

            if (roundStatus.IsActive == false)
            {
                await ReplyAsync($":trophy: {roundStatus.Winner.GetMention()} is the winner with `{roundStatus.Winner.NumberOfDice}` dice remaining! :trophy:");
                await UpdateAvatar("coy.png");
                return;
            }

            if (roundStatus.PlayerDice.Count() < 3) await UpdateAvatar("beaten.png");

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
                    var message = $"Your dice: {player.Dice.Split(",").Select(x => int.Parse(x).ToEmoji())}";

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
        [Alias("b")]
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

            await ReplyAsync($"{currentPlayer.GetMention()} bids `{quantity}` ˣ { pips.ToEmoji() }. { game.GetCurrentPlayer().GetMention()} is up.");
        }

        [Command("liar")]
        public async Task Liar()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            var currentPlayer = game.GetCurrentPlayer();

            if (Context.User.Id != currentPlayer.UserId) return;

            var liarResult = game.Liar();

            await ReplyAsync($"{liarResult.PlayerWhoCalledLiar.Name} called **liar** on `{liarResult.BidQuantity}` ˣ {liarResult.BidPips.ToEmoji()}.");

            // for the dramatic affect
            Thread.Sleep(3000);

            await ReplyAsync($"There was actually `{liarResult.ActualQuantity}` dice. :fire: {liarResult.PlayerWhoLostDice.GetMention()} loses {liarResult.DiceLost} dice. :fire:");

            await SendRoundSummary();

            await StartNewRound(game);
        }

        private async Task SendRoundSummary()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            game.GetPlayerDice();

            var players = game.GetPlayerDice();
            var playerDice = players.Select(x => $"{x.Name}: {string.Join(" ", x.Dice.Split(",").Select(x => int.Parse(x).ToEmoji()))}".TrimEnd());

            var allDice = players.SelectMany(x => x.Dice.Split(",").Select(x => int.Parse(x)));
            var allDiceGrouped = allDice
                .GroupBy(x => x)
                .OrderBy(x => x.Key);

            var countOfOnes = allDiceGrouped.SingleOrDefault(x => x.Key == 1)?.Count();

            var listOfAllDiceCounts = allDiceGrouped.Select(x => $"`{x.Count()}` ˣ {x.Key.ToEmoji()}");

            List<string> totals = new List<string>();
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