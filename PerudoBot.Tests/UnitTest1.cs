using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using PerudoBot.Database.Data;
using PerudoBot.EloService;
using PerudoBot.GameService;
using PerudoBotTests;
using System.Linq;

namespace PerudoBot.Tests
{
    public class Tests
    {
        private GameBotDbContextFactory _factory;
        private PerudoBotDbContext _db;
        private GameHandler _gameHandler;

        [SetUp]
        public void Setup()
        {
            _factory = new GameBotDbContextFactory();
            _db = _factory.CreateContext();
            _gameHandler = new GameHandler(_db, new MemoryCache(new MemoryCacheOptions()));
        }

        [Test]
        public void Test1()
        {
            ulong guildId = 111111;
            ulong channelId = 123456;

            var game = new GameObject(_db, channelId, guildId);
            game.CreateGame();

            game.AddPlayer(1, "Dave");
            game.AddPlayer(2, "Courtney");
            game.SetModeSuddenDeath();
            //game.ShufflePlayers();
            game.StartNewRound(); // Rename to "Start Game" and then auto-call this after "Liar" ??
            game.SetPlayerDice(1, "1,2,3,4,5");
            game.SetPlayerDice(2, "2,3,4,5,6");

            game.Bid(1, 10, 6);
            var roundResult = game.Liar(2);
            var liarResult = roundResult.LiarResult;

            var wasEliminated = liarResult.PlayerWhoLostDice.IsEliminated;
            var numberOfDiceRemaining = liarResult.PlayerWhoLostDice.NumberOfDice;

            var gameMode = game.GetGameMode();
            var eloHandler = new EloHandler(_db, guildId, gameMode);
            var gamePlayers = game.GetAllPlayers();

            foreach (var gamePlayer in gamePlayers)
            {
                eloHandler.AddPlayer(gamePlayer.PlayerId, gamePlayer.Rank);
            }
            eloHandler.CalculateAndSaveElo();

            var eloResults = eloHandler.GetEloResults();

            var daveElo = eloResults.Single(x => x.PlayerId == 1);
            var courtneyElo = eloResults.Single(x => x.PlayerId == 2);
            
            Assert.AreEqual(1510, courtneyElo.Elo);
            Assert.AreEqual(1500, courtneyElo.PreviousElo);
            Assert.AreEqual(1490, daveElo.Elo);
            Assert.AreEqual(1500, daveElo.PreviousElo);

            var eloSeason = eloHandler.GetCurrentEloSeason();
            Assert.AreEqual("Season Zero", eloSeason.SeasonName);
        }

        [Test]
        public void Test2()
        {
            ulong guildId = 111111;
            ulong channelId = 123456;

            var game = new GameObject(_db, channelId, guildId);
            game.CreateGame();

            game.AddPlayer(1, "Dave");
            game.AddPlayer(2, "Courtney");
            game.AddPlayer(3, "Em");
            game.AddPlayer(4, "Andrey");
            game.AddPlayer(5, "Josh");
            game.SetModeSuddenDeath();
            //game.ShufflePlayers();
            game.StartNewRound(); // Rename to "Start Game" and then auto-call this after "Liar" ??
            game.SetPlayerDice(1, "2,2,2,2,2");
            game.SetPlayerDice(2, "2,2,2,2,2");
            game.SetPlayerDice(3, "3,3,3,3,3");
            game.SetPlayerDice(4, "4,4,4,4,4");
            game.SetPlayerDice(5, "5,5,5,5,5");

            game.Bid(1, 6, 5); // there's only 5 5's, will lose 1 die
            var roundResult = game.Liar(2);
            var liarResult = roundResult.LiarResult;

            Assert.AreEqual(2, game.GetCurrentPlayer().PlayerId); // player 1 will be out and player 2 should go instead
        }
    }
}