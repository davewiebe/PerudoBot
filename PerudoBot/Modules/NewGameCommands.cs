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
        [Command("new")]
        public async Task NewGameAsync(params string[] options)
        {

            GameObject game;

            if (DateTime.Now.Hour < 12) game = _gameHandler.CreateSuddenDeathGame(Context.Channel.Id, Context.Guild.Id);
            else game = _gameHandler.CreateVariableGame(Context.Channel.Id, Context.Guild.Id);

            if(game == null)
            {
                await SendMessageAsync("Game in progress already");
                return;
            }

            await UpdateAvatar("gamestart.png");

            var commands = "**Commands**\n" +
                $"`!add @player`\n" +
                $"`!option suddendeath\\variable` to change game modes\n" +
                $"`!start`";

            await SendMessageAsync("New game created");
            await SendMessageAsync(commands);

            await DisplaySetupGamePlayers(game);
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

        [Command("remove")]
        [Alias("r")]
        public async Task RemovePlayer(params string[] users)
        {
            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            if (game == null)
            {
                await SendMessageAsync("No game is being set up");
                return;
            }

            foreach (var mentionedUser in Context.Message.MentionedUsers)
            {
                game.RemovePlayer(mentionedUser.Id);
            }

            await DisplaySetupGamePlayers(game);
        }

        [Command("add")]
        [Alias("a")]
        public async Task AddPlayer(params string[] users)
        {
            var game = _gameHandler.GetSettingUpGame(Context.Channel.Id);

            if (game == null)
            {
                await SendMessageAsync("No game is being set up");
                return;
            }

            foreach (var mentionedUser in Context.Message.MentionedUsers)
            {
                var guildUser = Context.Guild.GetUser(mentionedUser.Id);

                if (guildUser == null)
                {
                    await SendMessageAsync($"Unable to get guild info for {mentionedUser.Username}. Was not able to add user.");
                    continue;
                }
                game.AddPlayer(mentionedUser.Id, Context.Guild.Id, guildUser.Nickname ?? guildUser.Username, guildUser.IsBot);
            }

            if (Context.Message.MentionedUsers.Count == 0)
            {
                var guildUser = Context.Guild.GetUser(Context.User.Id);

                if (guildUser == null)
                {
                    await SendMessageAsync($"Unable to get guild info for {Context.User.Username}. Was not able to add user.");
                }
                else
                {
                    game.AddPlayer(Context.User.Id, Context.Guild.Id, guildUser.Nickname ?? guildUser.Username, Context.User.IsBot);
                }
            }

            await DisplaySetupGamePlayers(game);
        }

        private async Task DisplaySetupGamePlayers(GameObject game)
        {
            var listOfPlayers = game.GetPlayers().Select(x => $"{x.Name}{(x.IsBot ? " :robot:" : "")}").ToList();

            var gameType = "Sudden Death";
            if (game.GetMode() == GameMode.Variable) gameType = "Variable";


            var playerListText = string.Join("\n", listOfPlayers);
            if (playerListText == "") playerListText = "No players yet";

            var builder = new EmbedBuilder()
                            .WithTitle($"New Game")
                            .AddField($"Players", $"{playerListText}", inline: false)
                            .AddField($"Game Mode", $"{gameType}", inline: false);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("terminate")]
        public async Task Terminate()
        {
            var game = _gameHandler.GetActiveGame(Context.Channel.Id);

            if (game == null)
            {
                await SendMessageAsync("No game to terminate");
                return;
            }

            game.Terminate();
            await SendMessageAsync("Game terminated");
        }


        [Command("exact")]
        public async Task Exact(params string[] bidText)
        {
            await SendMessageAsync("Exact not implemented.\nIf you can't call liar and can't go up, time to start bluffing!");
        }
    }
}