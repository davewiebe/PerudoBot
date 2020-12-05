using Discord.Commands;
using PerudoBot.Database.Sqlite.Data;

namespace PerudoBot.Modules
{
    public partial class ReactionCommands : ModuleBase<CommandContext>
    {
        private readonly PerudoBotDbContext _db;

        public ReactionCommands()
        {
            //TODO: Let DI handle instantiation
            _db = new PerudoBotDbContext();
        }
    }
}