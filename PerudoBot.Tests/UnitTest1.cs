using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using PerudoBot.Database.Data;
using PerudoBot.GameService;
using PerudoBotTests;

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
            _gameHandler.AddPlayer(1, 222, "Dave", false);
            _gameHandler.AddPlayer(2, 222, "Courtney", false);
            _gameHandler.SetGameModeSuddenDeath();
            var game = _gameHandler.CreateGame();
            game.Start();
            game.StartNewRound();
            game.Bid(10, 6);
            game.Liar();
            game.OnEndOfRound();
        }
    }
}