using Microsoft.EntityFrameworkCore;
using PerudoBot.Database.Data;
using System;
using System.Data.Common;

namespace PerudoBotTests
{
    public class GameBotDbContextFactory : IDisposable
    {
        private DbConnection _connection;

        private DbContextOptions<PerudoBotDbContext> CreateOptions()
        {
            return new DbContextOptionsBuilder<PerudoBotDbContext>()
                .UseInMemoryDatabase("TestDb").Options;
        }

        public PerudoBotDbContext CreateContext()
        {
            if (_connection == null)
            {
                var options = CreateOptions();
                using (var context = new PerudoBotDbContext(options))
                {
                    context.Database.EnsureCreated();
                }
            }

            return new PerudoBotDbContext(CreateOptions());
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}