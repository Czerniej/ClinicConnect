using ClinicConnectApi.Data;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ClinicConnectApi.Tests.Infrastructure
{
    public sealed class SqliteTestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _options;
        public SqliteTestDatabase()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;
                using var context = CreateContext();
                context.Database.Migrate();
        }
        public AppDbContext CreateContext() => new(_options);
        public void Dispose() => _connection.Dispose();
    }
}