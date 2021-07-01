using System;
using System.Linq;
using PerudoBot.Database.Data;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;

namespace PerudoBot.GameService
{
    public class GameHandler
    {
        private readonly PerudoBotDbContext _db;
        private readonly IMemoryCache _cache;
        private ulong _channelId;
        private ulong _guildId;

        /*
*  Do all the user handling and setup for the game here
*  Save it all in cache, 
*  and then use it all when starting the game
*/

        // probably not safe with multiple channels yet

        public GameHandler(PerudoBotDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot)
        {
            var game = GetActiveGame();
            if (game != null) return false;

            var gameplayers = (List<GamePlayerDto>)_cache.Get($"gameplayers{_channelId}");

            if (gameplayers == null) gameplayers = new List<GamePlayerDto>();

            var userExists = gameplayers.Any(x => x.UserId == userId);
            if (userExists) return false;

            gameplayers.Add(new GamePlayerDto { Name = name, UserId = userId, GuildId = guildId, IsBot = isBot });

            _cache.Set($"gameplayers{_channelId}", gameplayers);
            return true;
        }

        public void ClearPlayerList()
        {
            _cache.Remove($"gameplayers{_channelId}");
        }

        public void SetGuild(ulong guildId)
        {
            _guildId = guildId;
        }

        public void SetGameModeSuddenDeath()
        {
            _cache.Set($"gamemode{_channelId}", "suddendeath");
        }
        public void SetGameModeVariable()
        {
            _cache.Set($"gamemode{_channelId}", "variable");
        }

        public GameObject CreateGame()
        {
            var game = GetActiveGame();
            if (game != null) return null;

            game = new GameObject(_db, _channelId);
            game.CreateGame(_guildId);

            var gameplayers = GetSetupPlayers();
            foreach (var player in gameplayers)
            {
                game.AddPlayer(player.UserId, player.GuildId, player.Name, player.IsBot);
            }
            
            var gamemode = (string)_cache.Get($"gamemode{_channelId}");

            if (gamemode == "suddendeath") game.SetModeSuddenDeath();
            if (gamemode == "variable") game.SetModeVariable();

            ClearPlayerList();

            return game;
        }

        public GameObject GetActiveGame()
        {
            var game = new GameObject(_db, _channelId);
            game.LoadActiveGame();
            if (game.IsInProgress()) return game;
            return null;
        }

        public void SetChannel(ulong channelId)
        {
            _channelId = channelId;
        }

        public List<GamePlayerDto> GetSetupPlayers()
        {
            var gamePlayers = (List<GamePlayerDto>)_cache.Get($"gameplayers{_channelId}");
            if (gamePlayers == null) return new List<GamePlayerDto>();
            return (List<GamePlayerDto>)_cache.Get($"gameplayers{_channelId}");
        }

        public void Terminate()
        {
            var inProgressGames = _db.Games.Where(x => x.ChannelId == _channelId)
                .Where(x => x.State == (int)GameState.InProgress);

            foreach (var game in inProgressGames)
            {
                game.State = (int)GameState.Terminated;
            } 
            _db.SaveChanges();
        }

        public string GetMode()
        {
            var gamemode = (string)_cache.Get($"gamemode{_channelId}");

            if (gamemode == "suddendeath") return GameMode.SuddenDeath;
            if (gamemode == "variable") return GameMode.Variable;
            return "";
        }
    }

    public class GamePlayerDto
    {
        public ulong UserId { get; set; }
        public string Name { get; set; }
        public ulong GuildId { get; internal set; }
        public bool IsBot { get; internal set; }
    }
}
