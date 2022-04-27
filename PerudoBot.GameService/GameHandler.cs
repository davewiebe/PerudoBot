using System;
using System.Linq;
using PerudoBot.Database.Data;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

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


        public GameHandler(PerudoBotDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public bool AddPlayer(ulong userId, ulong guildId, string name, bool isBot)
        {
            var game = GetActiveGame();
            if (game != null) return false;


            var discordPlayer = CreateAndGetDiscordPlayer(userId, name, isBot);

            var playerIds = (List<int>)_cache.Get($"players{_channelId}");
            if (playerIds == null) playerIds = new List<int>();

            var userExists = playerIds.Any(x => x == discordPlayer.PlayerId);
            if (userExists) return false;

            playerIds.Add(discordPlayer.PlayerId);

            _cache.Set($"players{_channelId}", playerIds);
            return true;
        }

        public DiscordPlayer CreateAndGetDiscordPlayer(ulong userId, string name, bool isBot)
        {
            var discordPlayer = _db.DiscordPlayers
                .Include(x => x.Player)
                .Where(x => x.GuildId == _guildId)
                .SingleOrDefault(x => x.UserId == userId);

            if (discordPlayer != null)
            {
                return discordPlayer;
            }

            discordPlayer = new DiscordPlayer
            {
                GuildId = _guildId,
                UserId = userId,
                IsAdministrator = false,
                Player = new Player
                {
                    Name = name
                },
                IsBot = isBot,
                BotKey = name
            };
            _db.DiscordPlayers.Add(discordPlayer);
            _db.SaveChanges();

            return discordPlayer;
        }

        public void ClearPlayerList()
        {
            _cache.Remove($"players{_channelId}");
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
        public void SetGameModeReverse()
        {
            _cache.Set($"gamemode{_channelId}", "reverse");
        }

        public GameObject CreateGame()
        {
            var game = GetActiveGame();
            if (game != null) return null;

            game = new GameObject(_db, _channelId, _guildId);

            game.CreateGame();

            var gamemode = (string)_cache.Get($"gamemode{_channelId}");

            if (gamemode == "suddendeath") game.SetModeSuddenDeath();
            if (gamemode == "variable") game.SetModeVariable();
            if (gamemode == "reverse") game.SetModeReverse();

            var playerDtos = GetSetupPlayerIds();
            foreach (var playerDto in playerDtos)
            {
                game.AddPlayer(playerDto.PlayerId, playerDto.Name);
            }

            ClearPlayerList();

            return game;
        }

        public GameObject GetActiveGame()
        {
            // TODO: Add all required decorators

            var game = new GameObject(_db, _channelId, _guildId);
            game.LoadActiveGame();
            if (game.IsInProgress()) return game;
            return null;
        }

        public void SetChannel(ulong channelId)
        {
            _channelId = channelId;
        }

        public List<PlayerDto> GetSetupPlayerIds()
        {
            var playerIds = (List<int>)_cache.Get($"players{_channelId}");
            if (playerIds == null) return new List<PlayerDto>();

            var players = _db.Players.Where(x => playerIds.Contains(x.Id))
                .Select(x => new PlayerDto { 
                    PlayerId = x.Id, 
                    Name = x.Name 
                })
                .ToList();

            return players;
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
            if (gamemode == "reverse") return GameMode.Reverse;
            return "";
        }
    }

    public class PlayerDto
    {
        public int PlayerId { get; set; }
        public string Name { get; set; }
    }
}
