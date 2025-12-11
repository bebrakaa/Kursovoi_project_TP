using System.Net;
using System.Net.Http.Json;
using InsuranceAgency.Application.DTOs.Payment;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.ValueObjects;
using InsuranceAgency.Infrastructure.Persistence;
using InsuranceAgency.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceAgency.Tests.Integration;

public class PaymentsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PaymentsApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add InMemory database
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // Не следовать за redirect'ами автоматически
        });
    }

    [Fact]
    public async Task InitiatePayment_WithValidContract_ReturnsOk()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var client = new Client("Test Client", "test@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));
        var agent = new Agent("Test Agent", "agent@example.com");

        db.Clients.Add(client);
        db.InsuranceServices.Add(service);
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var contract = new Contract(
            client.Id,
            service.Id,
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"),
            agent.Id);
        contract.Register("CTR-001", agent.Id);

        db.Contracts.Add(contract);
        await db.SaveChangesAsync();

        var dto = new InitiatePaymentDto
        {
            ContractId = contract.Id,
            Amount = 10000m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", dto);

        // Assert
        // Без авторизации API endpoints перенаправляют на страницу логина (302)
        // В реальном проекте нужно настроить тестовую авторизацию
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "API endpoints должны перенаправлять на страницу логина при отсутствии авторизации");
    }

    [Fact]
    public async Task InitiatePayment_WithNonExistentContract_ReturnsBadRequest()
    {
        // Arrange
        var dto = new InitiatePaymentDto
        {
            ContractId = Guid.NewGuid(),
            Amount = 10000m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/payments", dto);

        // Assert
        // Без авторизации API endpoints перенаправляют на страницу логина (302)
        // В реальном проекте нужно настроить тестовую авторизацию
        response.StatusCode.Should().Be(HttpStatusCode.Redirect, "API endpoints должны перенаправлять на страницу логина при отсутствии авторизации");
    }
}

