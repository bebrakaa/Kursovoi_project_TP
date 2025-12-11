using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.ValueObjects;
using InsuranceAgency.Infrastructure.Persistence;
using InsuranceAgency.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Tests.Integration;

public class InMemoryDbTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ContractRepository _repository;

    public InMemoryDbTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new ContractRepository(_context);
    }

    [Fact]
    public async Task ContractRepository_AddAsync_AddsContractToDatabase()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));
        var agent = new Agent("Test Agent", "agent@example.com");

        _context.Clients.Add(client);
        _context.InsuranceServices.Add(service);
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        var contract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"),
            agent.Id);
        contract.Register("CTR-001", agent.Id);

        // Act
        await _repository.AddAsync(contract);
        await _repository.SaveChangesAsync();

        // Assert
        var savedContract = await _context.Contracts.FindAsync(contract.Id);
        savedContract.Should().NotBeNull();
        savedContract!.Number.Should().Be("CTR-001");
        savedContract.Status.Should().Be(ContractStatus.Registered);
    }

    [Fact]
    public async Task ContractRepository_GetByIdAsync_ReturnsContractWithIncludes()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));
        var agent = new Agent("Test Agent", "agent@example.com");

        _context.Clients.Add(client);
        _context.InsuranceServices.Add(service);
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();

        var contract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"),
            agent.Id);
        contract.Register("CTR-001", agent.Id);

        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(contract.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Client.Should().NotBeNull();
        result.Client!.FullName.Should().Be("Test Client");
        result.Service.Should().NotBeNull();
        result.Service!.Name.Should().Be("Test Service");
        result.Agent.Should().NotBeNull();
        result.Agent!.FullName.Should().Be("Test Agent");
    }

    [Fact]
    public async Task ContractRepository_GetOverdueContractsAsync_ReturnsOverdueContracts()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));

        _context.Clients.Add(client);
        _context.InsuranceServices.Add(service);
        await _context.SaveChangesAsync();

        var overdueContract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-400)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            new Money(10000m, "RUB"));
        overdueContract.Register("CTR-OVERDUE", Guid.NewGuid());

        var activeContract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        activeContract.Register("CTR-ACTIVE", Guid.NewGuid());

        _context.Contracts.Add(overdueContract);
        _context.Contracts.Add(activeContract);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetOverdueContractsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(c => c.Id == overdueContract.Id);
        result.Should().NotContain(c => c.Id == activeContract.Id);
    }

    [Fact]
    public async Task ContractRepository_GetUnpaidContractsAsync_ReturnsUnpaidContracts()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));

        _context.Clients.Add(client);
        _context.InsuranceServices.Add(service);
        await _context.SaveChangesAsync();

        var unpaidContract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        unpaidContract.Register("CTR-UNPAID", Guid.NewGuid());
        // Set CreatedAt to 10 days ago
        _context.Entry(unpaidContract).Property("CreatedAt").CurrentValue = DateTime.UtcNow.AddDays(-10);

        var paidContract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        paidContract.Register("CTR-PAID", Guid.NewGuid());
        paidContract.MarkAsPaid();

        _context.Contracts.Add(unpaidContract);
        _context.Contracts.Add(paidContract);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUnpaidContractsAsync(TimeSpan.FromDays(7));

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(c => c.Id == unpaidContract.Id);
        result.Should().NotContain(c => c.Id == paidContract.Id);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

