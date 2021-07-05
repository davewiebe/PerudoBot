using Discord;
using Discord.Commands;
using PerudoBot.Database.Data;
using PerudoBot.EloService.Elo;
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

            game.Start();

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
                await SendMessageAsync($":trophy: {roundStatus.Winner.GetMention()} is the winner with `{roundStatus.Winner.NumberOfDice}` dice remaining! :trophy:");
                await UpdateAvatar("coy.png");

                await CalculateEloAsync(roundStatus.Players);

                return;
            }

            if (roundStatus.Players.Count < 3) await UpdateAvatar("beaten.png");

            await SendNewRoundStatus(roundStatus);
            await SendOutDice(roundStatus.Players);
            await SendMessageAsync($"A new round has begun. {game.GetCurrentPlayer().GetMention()} goes first");
        }

        private async Task CalculateEloAsync(List<PlayerData> playersData)
        {
            var currentSeason = _db.EloSeasons
                .AsQueryable()
                .Where(x => x.GuildId == Context.Guild.Id)
                .OrderBy(x => x.Id)
                .LastOrDefault();

            if (currentSeason == null)
            {
                currentSeason = new EloSeason
                {
                    GuildId = Context.Guild.Id,
                    SeasonName = "Season Zero"
                };
                _db.EloSeasons.Add(currentSeason);
                _db.SaveChanges();
            }

            var eloMatch = new EloMatch();

            foreach (var playerData in playersData)
            {
                var player = _db.Players
                    .AsQueryable()
                    .Where(x => x.GuildId == Context.Guild.Id)
                    .Where(x => x.UserId == playerData.UserId)
                    .Single();

                var elo = _db.PlayerElos
                    .AsQueryable()
                    .Where(x => x.PlayerId == player.Id)
                    .Where(x => x.EloSeason == currentSeason)
                    .SingleOrDefault();

                if (elo == null)
                {
                    elo = new PlayerElo
                    {
                        EloSeason = currentSeason,
                        GameMode = "Oh butts. Add this", // TODO
                        Player = player,
                        Rating = 1500
                    };
                    _db.PlayerElos.Add(elo);
                    _db.SaveChanges();
                }

                eloMatch.AddPlayer(player.Id, playerData.Rank, elo.Rating);
            }


            foreach (var playerData in playersData)
            {
                var player = _db.Players
                    .AsQueryable()
                    .Where(x => x.GuildId == Context.Guild.Id)
                    .Where(x => x.UserId == playerData.UserId)
                    .Single();


                var playerElo = _db.PlayerElos
                    .AsQueryable()
                    .Where(x => x.Player.GuildId == Context.Guild.Id)
                    .Where(x => x.Player.UserId == playerData.UserId)
                    .Where(x => x.EloSeason == currentSeason)
                    .Single();

                var newEloRating = eloMatch.GetEloRating(player.Id);
                var oldElo = playerElo.Rating;
                playerElo.Rating = newEloRating;

                await SendMessageAsync($"`{playerData.Rank}` {playerData.Name} `{oldElo}` (`{newEloRating - oldElo}`)");
            }

            _db.SaveChanges();
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
            var playerDice = game.GetPlayerDice();
            await SendOutDice(playerDice);
        }

        private async Task SendOutDice(List<PlayerData> playerDice)
        {
            foreach (var player in playerDice)
            {
                // send dice to each player
                var dice = player.Dice.Split(",");
                var diceEmojis = dice.Select(x => int.Parse(x).ToEmoji());


                var user = Context.Guild.Users.Single(x => x.Id == player.UserId);

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