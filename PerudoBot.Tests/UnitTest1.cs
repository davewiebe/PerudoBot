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

            game.Bid(10, 6);
            var liarResult = game.Liar();

            // complicated. because they weren't eliminated during this round, so it looks like they still have dice
            // at this point -- and not eliminated.
            var wasEliminated = liarResult.PlayerWhoLostDice.IsEliminated;

            game.GetPlayers();
            
            game.OnEndOfRound(); // what does this do?
        }
    }
}