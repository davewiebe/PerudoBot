using Discord;
using Discord.Commands;
using PerudoBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
//using Game = PerudoBot.Data.Game;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("new")]
        public async Task NewGameAsync(params string[] options)
        {
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

            game.Start();
            try
            {
                foreach (var playerDice in game.GetPlayerDice())
                {
                    // send dice to each player
                    var message = $"Your dice: {playerDice.Dice}";

                    var user = Context.Guild.Users.Single(x => x.Id == playerDice.UserId);


                    var requestOptions = new RequestOptions()
                    { RetryMode = RetryMode.RetryRatelimit };
                    await user.SendMessageAsync(message, options: requestOptions);
                }
            }
            catch (Exception e)
            {
                var monkey = e.Message;
            }
            await ReplyAsync($"Starting the game!\nUse `!bid 2 2s` or `!exact` or `!liar` to play.");

            await ReplyAsync($"A new round has begun. {game.GetCurrentPlayer().GetMention(Context)} goes first");
        }

        [Command("newround")]
        public async Task newround()
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);
            await ReplyAsync($"A new round has begun. {game.GetCurrentPlayer().GetMention(Context)} goes first");
        }

        [Command("bid")]
        public async Task Bid(params string[] bidText)
        {
            var game = _gameHandler.GetInProgressGame(Context.Channel.Id);

            var currentPlayer = game.GetCurrentPlayer();

            if (Context.User.Id != currentPlayer.UserId) return;

            var quantity = int.Parse(bidText[0]);
            var pips = int.Parse(bidText[1].Trim('s'));

            game.Bid(quantity, pips);


            await ReplyAsync($"{currentPlayer.GetMention(Context)} bids `{quantity}` ˣ { pips }. { game.GetCurrentPlayer().GetMention(Context)} is up.");
        }
    }
}