using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace PerudoBot.Database.Data
{ 
    public class PerudoBotDbContext : DbContext
    {
        public PerudoBotDbContext() : base()
        {
            Database.Migrate();
        }

        public PerudoBotDbContext(DbContextOptions<PerudoBotDbContext> options) : base(options)
        {
        }

        public DbSet<Game> Games { get; set; }

        public DbSet<Player> Players { get; set; }
        public DbSet<DiscordPlayer> DiscordPlayers { get; set; }
        public DbSet<GamePlayer> GamePlayers { get; set; }

        public DbSet<GamePlayerRound> GamePlayerRounds { get; set; }

        public DbSet<Action> Actions { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<LiarCall> LiarCalls { get; set; }
        public DbSet<ExactCall> ExactCalls { get; set; }

        public DbSet<Bet> Bets { get; set; }

        public DbSet<Round> Rounds { get; set; }

        public DbSet<StandardRound> StandardRounds { get; }
        public DbSet<PlayerElo> PlayerElos { get; set; }
        public DbSet<EloSeason> EloSeasons { get; set; }

        public DbSet<Metadata> Metadata { get; set; }
        //public DbSet<FaceoffRound> FaceoffRounds { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Action>()
                .ToTable("Actions")
                .HasDiscriminator<string>("ActionType");

            modelBuilder.Entity<Round>()
                .ToTable("Rounds")
                .HasDiscriminator<string>("RoundType");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false)
                    .Build();

                var connectionString = configuration.GetConnectionString("PerudoBotDb");
                optionsBuilder
                    //.UseLazyLoadingProxies()
                    .UseSqlite(connectionString);
            }
        }
    }
}