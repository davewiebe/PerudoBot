using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;

namespace PerudoBot.Modules
{
    public partial class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("start")]
        public async Task StartGame()
        {
            SetGuildAndChannel();

            await UpdateAvatar("wink.png");

            var game = _gameHandler.CreateGame();

            await SendMessageAsync($"Starting the game!\nUse `!bid 2 2s` or `!liar` to play.");

            if (game.HasBots())
            {
                var updateMessage = await SendMessageAsync("||{}||");
                game.SetMetadata("BotUpdateMessageId", updateMessage.Id.ToString());
            }

            game.ShufflePlayers();

            await StartNewRound(game);
        }

        private async Task StartNewRound(GameObject game)
        {
            var roundStatus = game.StartNewRound();            

            if (roundStatus.IsActive == false)
            {
                await SendMessageAsync($":trophy: {roundStatus.Winner.GetMention(_db)} is the winner with `{roundStatus.Winner.NumberOfDice}` dice remaining! :trophy:");

                var winrattle = roundStatus.Winner.GetPlayerMetadata("winrattle");

                if (winrattle != null)
                {
                    if (winrattle.ToLower().StartsWith("!gif "))
                    {
                        await SendTempMessageAsync(winrattle);
                        Thread.Sleep(1500);
                    }
                    else await SendMessageAsync(winrattle);
                }
                await UpdateAvatar("coy.png");

                await CalculateEloAsync(game);
                await AwardPoints(game);

                return;
            }

            if (roundStatus.Players.Count < 3) await UpdateAvatar("beaten.png");

            await SendCurrentRoundStatus(roundStatus);
            await SendOutDice(roundStatus.Players);

            var nextPlayer = game.GetCurrentPlayer();
            var updateMessage = $"A new round has begun. {nextPlayer.GetMention(_db)} goes first";

            if (game.HasBots())
            {
                var botMessage = new
                {
                    nextPlayer = nextPlayer.GetDiscordId(_db),
                    gameDice = game.GetAllDice().Count,
                    round = game.GetCurrentRoundNumber()
                };

                await Context.Message.Channel.ModifyMessageAsync(ulong.Parse(game.GetMetadata("BotUpdateMessageId")),
                    x => x.Content = $"||`{JsonConvert.SerializeObject(botMessage)}`||");

                updateMessage += $" ||`@bots update {game.GetMetadata("BotUpdateMessageId")}`||";
            }

            await SendMessageAsync(updateMessage);
        }

        public async Task SendCurrentRoundStatus(RoundStatus roundStatus)
        {
            var totalDice = roundStatus.Players.Sum(x => x.Dice.Count);

            var players = roundStatus.ActivePlayers
                            .OrderBy(x => x.TurnOrder)
                            .Select(x => $"`{x.Dice.Count}` {x.Name}");

            var playerList = string.Join("\n", players);

            var probability = 3.0;

            var quickmaths = $"Quick maths: {totalDice}/{probability:F0} = `{totalDice / probability:F2}`";

            var builder = new EmbedBuilder()
                .WithTitle($"Round {roundStatus.RoundNumber}")
                .AddField("Players", $"{playerList}\n\nTotal dice left: `{totalDice}`\n{quickmaths}", inline: false);
            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(
                embed: embed)
                .ConfigureAwait(false);
        }

        private async Task SendEncryptedDiceAsync(PlayerData player)
        {
            var mention = player.GetMention(_db);
            var diceText = string.Join(" ", player.Dice);
            var encoded = SimpleAES.AES256.Encrypt(diceText, player.GetBotKey(_db));
            await SendMessageAsync($"{mention} ||`deal {encoded}`||");
        }

        private async Task SendOutDice(List<PlayerData> playerDice, bool isUpdate = false)
        {
            var playerIds = playerDice.Select(x => x.PlayerId).ToList();

            var players = _db.Players.AsQueryable()
                .Include(x => x.DiscordPlayer)
                .Where(x => playerIds.Contains(x.Id)).ToList();

            foreach (var player in playerDice)
            {
                // send dice to each player
                if (player.NumberOfDice == 0) continue;
                var diceEmojis = player.GetDiceEmojis();

                var userId = players.Single(x => x.Id == player.PlayerId).DiscordPlayer.UserId;
                var user = Context.Guild.Users.Single(x => x.Id == userId);

                if (user.IsBot)
                {
                    await SendEncryptedDiceAsync(player);
                }
                else
                {
                    var prefix = isUpdate ? "         :" : "Your dice:";
                    var message = $"`{prefix}` {string.Join(" ", diceEmojis)}";
                    var requestOptions = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit };
                    await user.SendMessageAsync(message, options: requestOptions);
                }
            }
        }
    }
}