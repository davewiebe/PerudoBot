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

            var game = new GameObject(_db, channelId);

            game.CreateGame(guildId);
            game.AddPlayer(1, guildId, "Dave", isBot: false);
            game.AddPlayer(2, guildId, "Courtney", isBot: false);
            //game.SetPlayerOrder(order here....somehow); // Need to add this soon. Will help with testing.
            game.SetModeSuddenDeath();

            game.Start();
            game.StartNewRound(); // I don't think i should have to call this...
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