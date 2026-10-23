using ClinicConnectApi.Data;
using ClinicConnectApi.Models;
using ClinicConnectApi.Tests.Infrastructure;

using Microsoft.EntityFrameworkCore;

namespace ClinicConnectApi.Tests.Data
{
    public class DbUpdateExceptionExtensionsTests : IDisposable
    {
        private readonly SqliteTestDatabase _database = new();
        private readonly AppDbContext _db;
        public DbUpdateExceptionExtensionsTests()
        {
            _db = _database.CreateContext();
        }
        public void Dispose()
        {
            _db.Dispose();
            _database.Dispose();
        }
        [Fact]
        public async Task IsUniqueConstraintViolation_DuplicatePesel_ReturnsTrue()
        {
            _db.Patients.Add(new Patient { FirstName = "Henry", LastName = "Skalitz", Pesel = "44051401359" });
            await _db.SaveChangesAsync();

            _db.Patients.Add(new Patient { FirstName = "Jan", LastName = "Hus", Pesel = "44051401359" });
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _db.SaveChangesAsync());

            Assert.True(exception.IsUniqueConstraintViolation());
        }
    }
}