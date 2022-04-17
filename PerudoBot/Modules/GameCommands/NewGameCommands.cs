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
            SetGuildAndChannel();
            var game = _gameHandler.GetActiveGame();

            if (game != null)
            {
                await SendMessageAsync("Game in progress already");
                return;
            }

            if (DateTime.Now.Hour < 12) _gameHandler.SetGameModeSuddenDeath();
            else _gameHandler.SetGameModeReverse();

            _gameHandler.ClearPlayerList();

            await UpdateAvatar("gamestart.png");

            var commands = "**Commands**\n" +
                $"`!add @player`\n" +
                $"`!option suddendeath\\variable` to change game modes\n" +
                $"`!start`";

            await SendMessageAsync("New game created");
            await SendMessageAsync(commands);

            await DisplaySetupGamePlayers();
        }

        [Command("option")]
        [Alias("options")]
        public async Task Options(params string[] options)
        {
            SetGuildAndChannel();

            if (options.Any(x => x.ToLower() == "suddendeath"))
            {
                _gameHandler.SetGameModeSuddenDeath();
            }
            else if (options.Any(x => x.ToLower() == "variable"))
            {
                _gameHandler.SetGameModeVariable();
            }
            else if (options.Any(x => x.ToLower() == "reverse"))
            {
                _gameHandler.SetGameModeReverse();
            }

            await DisplaySetupGamePlayers();
        }

        [Command("remove")]
        [Alias("r")]
        public async Task RemovePlayer(params string[] users)
        {
            //_gameObject.GetGame(Context.Channel.Id);

            ////if (game == null)
            ////{
            ////    await SendMessageAsync("No game is being set up");
            ////    return;
            ////}

            //foreach (var mentionedUser in Context.Message.MentionedUsers)
            //{
            //    _gameObject.RemovePlayer(mentionedUser.Id);
            //}

            //await DisplaySetupGamePlayers(_gameObject);
        }


        [Command("all")]
        public async Task AddAll(params string[] asdf)
        {
            SetGuildAndChannel();

            var game = _gameHandler.GetActiveGame();
            if (game != null) return;

            var voiceChannel = (Context.User as IVoiceState).VoiceChannel.Id;

            var users = Context.Guild.Channels.First(x => x.Id == voiceChannel).Users;

            foreach (var user in users)
            {
                var guildUser = Context.Guild.GetUser(user.Id);

                if (guildUser == null)
                {
                    await SendMessageAsync($"Unable to get guild info for {Context.User.Username}. Was not able to add user.");
                }
                else
                {
                    _gameHandler.AddPlayer(Context.User.Id, Context.Guild.Id, guildUser.Nickname ?? guildUser.Username, Context.User.IsBot);
                }
            }
            await DisplaySetupGamePlayers();
        }

        [Command("add")]
        [Alias("a")]
        public async Task AddPlayer(params string[] users)
        {
            SetGuildAndChannel();

            var game = _gameHandler.GetActiveGame();
            if (game != null) return;

            foreach (var mentionedUser in Context.Message.MentionedUsers)
            {
                var guildUser = Context.Guild.GetUser(mentionedUser.Id);

                if (guildUser == null)
                {
                    await SendMessageAsync($"Unable to get guild info for {mentionedUser.Username}. Was not able to add user.");
                    continue;
                }
                _gameHandler.AddPlayer(mentionedUser.Id, Context.Guild.Id, guildUser.Nickname ?? guildUser.Username, guildUser.IsBot);
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
                    _gameHandler.AddPlayer(Context.User.Id, Context.Guild.Id, guildUser.Nickname ?? guildUser.Username, Context.User.IsBot);
                }
            }

            await DisplaySetupGamePlayers();
        }

        private async Task DisplaySetupGamePlayers()
        {
            var gamePlayers = _gameHandler.GetSetupPlayerIds();

            var listOfPlayers = gamePlayers
                .Select(x => $"{x.Name}")
                .ToList()
                .OrderBy(x => x);

            var gameType = "Sudden Death";
            if (_gameHandler.GetMode() == GameMode.Variable) gameType = "Variable";
            if (_gameHandler.GetMode() == GameMode.Reverse) gameType = "Reverse";


            var playerListText = string.Join("\n", listOfPlayers);
            if (playerListText == "") playerListText = "No players yet";

            var builder = new EmbedBuilder()
                            .WithTitle($"New Game")
                            .AddField($"Players ({listOfPlayers.Count()})", $"{playerListText}", inline: false)
                            .AddField($"Game Mode", $"{gameType}", inline: false);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("terminate")]
        public async Task Terminate()
        {
            SetGuildAndChannel();
            _gameHandler.Terminate();
            await SendMessageAsync("Game terminated");
        }
    }
}