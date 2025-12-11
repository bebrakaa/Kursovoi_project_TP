using AutoMapper;
using InsuranceAgency.Application.Common.Exceptions;
using InsuranceAgency.Application.DTOs;
using InsuranceAgency.Application.Interfaces;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Mapping;
using InsuranceAgency.Application.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.Exceptions;
using InsuranceAgency.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace InsuranceAgency.Tests.Unit;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> _contractRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IInsuranceServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IAgentRepository> _agentRepositoryMock;
    private readonly IMapper _mapper;
    private readonly ContractService _service;

    public ContractServiceTests()
    {
        _contractRepositoryMock = new Mock<IContractRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _serviceRepositoryMock = new Mock<IInsuranceServiceRepository>();
        _agentRepositoryMock = new Mock<IAgentRepository>();

        // Setup AutoMapper directly (AutoMapper 15.x compatible)
        // In AutoMapper 15.x, MapperConfiguration constructor signature changed
        // Use Mapper.Initialize or create via ServiceCollection
        // For now, we'll use a workaround with reflection or downgrade AutoMapper
        // Since AutoMapper 15.x has breaking changes, we'll create Mapper manually
        var expression = new MapperConfigurationExpression();
        expression.AddProfile<MappingProfile>();
        // In AutoMapper 15.x, MapperConfiguration doesn't accept MapperConfigurationExpression directly
        // We need to use the static Mapper.Initialize or create via ServiceCollection
        // For tests, let's use ServiceCollection approach
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
        var serviceProvider = services.BuildServiceProvider();
        _mapper = serviceProvider.GetRequiredService<IMapper>();

        _service = new ContractService(
            _contractRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _agentRepositoryMock.Object,
            _mapper);
    }

    [Fact]
    public async Task CreateContractAsync_WithValidData_ReturnsContractDto()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var client = new Client("John Doe", "john@example.com", "+1234567890", "1234567890");
        var service = new InsuranceService("Test Service", new Money(10000m, "RUB"));
        var agent = new Agent("Agent Name", "agent@example.com");

        _clientRepositoryMock.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId))
            .ReturnsAsync(service);
        _agentRepositoryMock.Setup(r => r.GetByIdAsync(agentId))
            .ReturnsAsync(agent);
        _contractRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var dto = new CreateContractDto
        {
            ClientId = clientId,
            ServiceId = serviceId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(365),
            PremiumAmount = 10000m,
            PremiumCurrency = "RUB"
        };

        // Act
        var result = await _service.CreateContractAsync(dto, agentId);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId);
        result.ServiceId.Should().Be(serviceId);
        result.PremiumAmount.Should().Be(10000m);
        result.PremiumCurrency.Should().Be("RUB");
        result.Status.Should().Be(ContractStatus.Registered.ToString());

        _contractRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Contract>()), Times.Once);
        _contractRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateContractAsync_WithNonExistentClient_ThrowsNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        _clientRepositoryMock.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync((Client?)null);

        var dto = new CreateContractDto
        {
            ClientId = clientId,
            ServiceId = serviceId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(365),
            PremiumAmount = 10000m,
            PremiumCurrency = "RUB"
        };

        // Act & Assert
        var act = async () => await _service.CreateContractAsync(dto, agentId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Client {clientId} not found*");
    }

    [Fact]
    public async Task CreateContractAsync_WithNonExistentService_ThrowsNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var agentId = Guid.NewGuid();

        var client = new Client("John Doe", "john@example.com");

        _clientRepositoryMock.Setup(r => r.GetByIdAsync(clientId))
            .ReturnsAsync(client);
        _serviceRepositoryMock.Setup(r => r.GetByIdAsync(serviceId))
            .ReturnsAsync((InsuranceService?)null);

        var dto = new CreateContractDto
        {
            ClientId = clientId,
            ServiceId = serviceId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(365),
            PremiumAmount = 10000m,
            PremiumCurrency = "RUB"
        };

        // Act & Assert
        var act = async () => await _service.CreateContractAsync(dto, agentId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Insurance service {serviceId} not found*");
    }

    [Fact]
    public async Task RegisterContractAsync_WithValidData_ReturnsContractDto()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var contractNumber = "CTR-20241201-123456";

        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RegisterContractAsync(contractId, contractNumber, agentId);

        // Assert
        result.Should().NotBeNull();
        result.Number.Should().Be(contractNumber);
        result.Status.Should().Be(ContractStatus.Registered.ToString());

        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
        _contractRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterContractAsync_WithNonExistentContract_ThrowsNotFoundException()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var contractNumber = "CTR-20241201-123456";

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        var act = async () => await _service.RegisterContractAsync(contractId, contractNumber, agentId);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*Contract {contractId} not found*");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingContract_ReturnsContractDto()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-001", Guid.NewGuid());

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);

        // Act
        var result = await _service.GetByIdAsync(contractId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(contract.Id);
        result.Number.Should().Be(contract.Number);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentContract_ReturnsNull()
    {
        // Arrange
        var contractId = Guid.NewGuid();

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _service.GetByIdAsync(contractId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllContracts()
    {
        // Arrange
        var contracts = new List<Contract>
        {
            new Contract(Guid.NewGuid(), Guid.NewGuid(), 
                DateOnly.FromDateTime(DateTime.UtcNow), 
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
                new Money(10000m, "RUB")),
            new Contract(Guid.NewGuid(), Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
                new Money(15000m, "RUB"))
        };

        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(contracts);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ActivateContractAsync_WithValidVerifications_ActivatesContract()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var contract = new Contract(
            clientId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-001", agentId);
        contract.MarkAsPaid();

        var verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();
        var fullNameVerification = new DocumentVerification(clientId, agentId, "FullName", "John Doe");
        fullNameVerification.Approve(agentId);
        var passportVerification = new DocumentVerification(clientId, agentId, "Passport", "1234 567890");
        passportVerification.Approve(agentId);

        verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId))
            .ReturnsAsync(new List<DocumentVerification> { fullNameVerification, passportVerification });

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ActivateContractAsync(contractId, verificationRepositoryMock.Object);

        // Assert
        result.Should().NotBeNull();
        contract.Status.Should().Be(ContractStatus.Active);
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
    }

    [Fact]
    public async Task ActivateContractAsync_WithMissingRequiredVerification_ThrowsDomainException()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var contract = new Contract(
            clientId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-002", agentId);
        contract.MarkAsPaid();

        var verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();
        // Только FullName, нет Passport
        var fullNameVerification = new DocumentVerification(clientId, agentId, "FullName", "John Doe");
        fullNameVerification.Approve(agentId);

        verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId))
            .ReturnsAsync(new List<DocumentVerification> { fullNameVerification });

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);

        // Act & Assert
        var act = async () => await _service.ActivateContractAsync(contractId, verificationRepositoryMock.Object);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*нет одобренных обязательных данных*");
    }

    [Fact]
    public async Task ActivateContractAsync_WithPendingRequiredVerification_ThrowsDomainException()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var contract = new Contract(
            clientId,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-003", agentId);
        contract.MarkAsPaid();

        var verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();
        // Passport в статусе Pending
        var pendingPassport = new DocumentVerification(clientId, agentId, "Passport", "1234 567890");
        // Status = Pending по умолчанию

        verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId))
            .ReturnsAsync(new List<DocumentVerification> { pendingPassport });

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);

        // Act & Assert
        var act = async () => await _service.ActivateContractAsync(contractId, verificationRepositoryMock.Object);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*ожидает верификации обязательные данные*");
    }
}

