using Discord.Commands;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using PerudoBot.Database.Data;
using PerudoBot.GameService;
using System;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands : ModuleBase<CommandContext>
    {
        private readonly PerudoBotDbContext _db;
        private readonly IMemoryCache _cache;
        private readonly GameHandler _gameHandler;

        public ReactionCommands(IServiceProvider serviceProvider)
        {
            _cache = serviceProvider.GetRequiredService<IMemoryCache>();
            _db = serviceProvider.GetRequiredService<PerudoBotDbContext>();

            _gameHandler = new GameHandler(_db, _cache);
        }
    }
}