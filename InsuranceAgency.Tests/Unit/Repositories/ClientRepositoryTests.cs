using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using InsuranceAgency.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsuranceAgency.Tests.Unit.Repositories
{
    public class ClientRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClientRepository _repository;

        public ClientRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ClientRepository(_context);
        }

        [Fact]
        public async Task AddAsync_AddsClientToDatabase()
        {
            // Arrange
            var client = new Client("Test Client", "test@test.com", "123456789", "1234567890");

            // Act
            await _repository.AddAsync(client);
            await _repository.SaveChangesAsync();

            // Assert
            var result = await _context.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);
            Assert.NotNull(result);
            Assert.Equal("Test Client", result.FullName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsClient_WhenExists()
        {
            // Arrange
            var client = new Client("Test Client", "test@test.com", "123456789", "1234567890");
            await _repository.AddAsync(client);
            await _repository.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(client.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(client.Id, result.Id);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllClients()
        {
            // Arrange
            var client1 = new Client("Client 1", "client1@test.com", "111", "1111");
            var client2 = new Client("Client 2", "client2@test.com", "222", "2222");
            await _repository.AddAsync(client1);
            await _repository.AddAsync(client2);
            await _repository.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.True(result.Count >= 2);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}

