using InsuranceAgency.Application.Common.Validation;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.ValueObjects;
using InsuranceAgency.Worker.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceAgency.Tests.Unit.Services;

public class ProblematicContractsCheckerTests
{
    private readonly Mock<IContractRepository> _contractRepositoryMock;
    private readonly Mock<IDocumentVerificationRepository> _verificationRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<ILogger<ProblematicContractsChecker>> _loggerMock;
    private readonly ProblematicContractsChecker _checker;

    public ProblematicContractsCheckerTests()
    {
        _contractRepositoryMock = new Mock<IContractRepository>();
        _verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<ProblematicContractsChecker>>();

        _checker = new ProblematicContractsChecker(
            _contractRepositoryMock.Object,
            _verificationRepositoryMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithOverdueContracts_MarksThemAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var overdueContract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-400)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            new Money(10000m, "RUB"));
        overdueContract.Register("CTR-001", Guid.NewGuid());
        // Устанавливаем Client через рефлексию для теста
        typeof(Contract).GetProperty("Client")?.SetValue(overdueContract, client);

        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract> { overdueContract });
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>()); // Пустой список, чтобы избежать NullReferenceException
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        // Контракт сначала помечается как Overdue, затем как Problematic
        overdueContract.IsFlaggedProblem.Should().BeTrue();
        overdueContract.Status.Should().BeOneOf(ContractStatus.Overdue, ContractStatus.Problematic);
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithUnpaidContracts_MarksThemAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var unpaidContract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        unpaidContract.Register("CTR-002", Guid.NewGuid());
        typeof(Contract).GetProperty("Client")?.SetValue(unpaidContract, client);

        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract> { unpaidContract });
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        unpaidContract.IsFlaggedProblem.Should().BeTrue();
        unpaidContract.Status.Should().Be(ContractStatus.Problematic);
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithMissingRequiredVerification_MarksAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var contract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-INVALID", Guid.NewGuid());
        contract.MarkAsPaid();

        var allContracts = new List<Contract> { contract };
        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allContracts);
        _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id))
            .ReturnsAsync(new List<DocumentVerification>()); // Нет обязательных верификаций
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        contract.IsFlaggedProblem.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Problematic);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithMissingVerification_MarksAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var contract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-003", Guid.NewGuid());
        contract.MarkAsPaid();
        typeof(Contract).GetProperty("Client")?.SetValue(contract, client);

        var allContracts = new List<Contract> { contract };
        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allContracts);
        _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id))
            .ReturnsAsync(new List<DocumentVerification>()); // Нет верификаций
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        contract.IsFlaggedProblem.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Problematic);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithPendingRequiredVerification_MarksAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var contract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-004", Guid.NewGuid());
        contract.MarkAsPaid();
        typeof(Contract).GetProperty("Client")?.SetValue(contract, client);

        var pendingVerification = new DocumentVerification(
            client.Id,
            null,
            "FullName",
            "John Doe");
        // Status = Pending по умолчанию

        var allContracts = new List<Contract> { contract };
        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allContracts);
        _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id))
            .ReturnsAsync(new List<DocumentVerification> { pendingVerification });
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        contract.IsFlaggedProblem.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Problematic);
    }

    [Fact]
    public async Task CheckAndProcessProblematicContractsAsync_WithValidContract_DoesNotMarkAsProblematic()
    {
        // Arrange
        var client = new Client("Test Client", "test@example.com");
        var contract = new Contract(
            client.Id,
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
            new Money(10000m, "RUB"));
        contract.Register("CTR-005", Guid.NewGuid());
        contract.MarkAsPaid();
        typeof(Contract).GetProperty("Client")?.SetValue(contract, client);

        var approvedFullName = new DocumentVerification(client.Id, Guid.NewGuid(), "FullName", "John Doe");
        approvedFullName.Approve(Guid.NewGuid());
        var approvedPassport = new DocumentVerification(client.Id, Guid.NewGuid(), "Passport", "1234 567890");
        approvedPassport.Approve(Guid.NewGuid());

        var allContracts = new List<Contract> { contract };
        _contractRepositoryMock.Setup(r => r.GetOverdueContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetUnpaidContractsAsync(It.IsAny<TimeSpan>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetContractsRequiringRenewalAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetExpiredContractsAsync())
            .ReturnsAsync(new List<Contract>());
        _contractRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(allContracts);
        _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(client.Id))
            .ReturnsAsync(new List<DocumentVerification> { approvedFullName, approvedPassport });

        // Act
        await _checker.CheckAndProcessProblematicContractsAsync();

        // Assert
        contract.IsFlaggedProblem.Should().BeFalse();
        contract.Status.Should().Be(ContractStatus.Paid);
    }
}

