using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using PerudoBot.EloService.Elo;
using PerudoBot.EloService;
using PerudoBot.Extensions;
using PerudoBot.GameService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            game.ShufflePlayers();

            await StartNewRound(game);
        }

        private void SetGuildAndChannel()
        {
            _gameHandler.SetChannel(Context.Channel.Id);
            _gameHandler.SetGuild(Context.Guild.Id);
        }

        private async Task StartNewRound(IGameObject game)
        {
            var roundStatus = game.StartNewRound();

            if (roundStatus.IsActive == false)
            {
                await SendMessageAsync($":trophy: {roundStatus.Winner.GetMention(_db)} is the winner with `{roundStatus.Winner.NumberOfDice}` dice remaining! :trophy:");
                await UpdateAvatar("coy.png");

                await CalculateEloAsync(game);

                return;
            }

            if (roundStatus.Players.Count < 3) await UpdateAvatar("beaten.png");

            await SendNewRoundStatus(roundStatus);
            await SendOutDice(roundStatus.Players);
            await SendMessageAsync($"A new round has begun. {game.GetCurrentPlayer().GetMention(_db)} goes first");
        }

        private async Task CalculateEloAsync(IGameObject game)
        {
            var gameMode = game.GetGameMode();
            var eloHandler = new EloHandler(_db, Context.Guild.Id, gameMode);
            var gamePlayers = game.GetPlayers();

            foreach (var gamePlayer in gamePlayers)
            {
                eloHandler.AddPlayer(gamePlayer.PlayerId, gamePlayer.Rank);
            }
            eloHandler.CalculateAndSaveElo();

            var eloResults = eloHandler.GetEloResults();

            foreach (var gamePlayer in gamePlayers)
            {
                var eloResult = eloResults.Single(x => x.PlayerId == gamePlayer.PlayerId);
                await SendMessageAsync($"{gamePlayer.Rank} {gamePlayer.Name} {eloResult.PreviousElo} => {eloResult.Elo} ({eloResult.Elo - eloResult.PreviousElo})");
            }
        }


        private async Task SendNewRoundStatus(RoundStatus roundStatus)
        {
            var totalDice = roundStatus.Players.Sum(x => x.NumberOfDice);

            var players = roundStatus.ActivePlayers
                            .OrderBy(x => x.TurnOrder)
                            .Select(x => $"`{x.NumberOfDice}` {x.Name}");

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

        [Command("resenddice")]
        public async Task ResendDice()
        {
            _gameHandler.SetChannel(Context.Channel.Id);
            var game = _gameHandler.GetActiveGame();
            var playerDice = game.GetPlayers();
            await SendOutDice(playerDice);
        }

        private async Task SendOutDice(List<PlayerData> playerDice)
        {
            var playerIds = playerDice.Select(x => x.PlayerId).ToList();

            var players = _db.Players.AsQueryable()
                .Include(x => x.DiscordPlayer)
                .Where(x => playerIds.Contains(x.Id)).ToList();

            foreach (var player in playerDice)
            {
                // send dice to each player
                var dice = player.Dice.Split(",");
                var diceEmojis = dice.Select(x => int.Parse(x).ToEmoji());

                var userId = players.Single(x => x.Id == player.PlayerId).DiscordPlayer.UserId;
                var user = Context.Guild.Users.Single(x => x.Id == userId);

                if (user.IsBot)
                {
                    await SendMessageAsync($"{player.Name}'s Dice: {string.Join(" ", diceEmojis)}");
                }
                else
                {
                    var message = $"Your dice: {string.Join(" ", diceEmojis)}";
                    var requestOptions = new RequestOptions() { RetryMode = RetryMode.RetryRatelimit };
                    await user.SendMessageAsync(message, options: requestOptions);
                }
            }
        }
    }
}