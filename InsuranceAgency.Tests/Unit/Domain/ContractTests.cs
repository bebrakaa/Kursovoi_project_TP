using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.Exceptions;
using InsuranceAgency.Domain.ValueObjects;

namespace InsuranceAgency.Tests.Unit.Domain;

public class ContractTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesContract()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(365);
        var premium = new Money(10000m, "RUB");

        // Act
        var contract = new Contract(clientId, serviceId, startDate, endDate, premium);

        // Assert
        contract.Should().NotBeNull();
        contract.ClientId.Should().Be(clientId);
        contract.ServiceId.Should().Be(serviceId);
        contract.StartDate.Should().Be(startDate);
        contract.EndDate.Should().Be(endDate);
        contract.Premium.Should().Be(premium);
        contract.Status.Should().Be(ContractStatus.Draft);
        contract.IsPaid.Should().BeFalse();
        contract.IsFlaggedProblem.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ThrowsValidationException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(-1);
        var premium = new Money(10000m, "RUB");

        // Act & Assert
        var act = () => new Contract(clientId, serviceId, startDate, endDate, premium);
        act.Should().Throw<ValidationException>()
            .WithMessage("*EndDate must be after StartDate*");
    }

    [Fact]
    public void Constructor_WithEmptyClientId_ThrowsValidationException()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(365);
        var premium = new Money(10000m, "RUB");

        // Act & Assert
        var act = () => new Contract(Guid.Empty, serviceId, startDate, endDate, premium);
        act.Should().Throw<ValidationException>()
            .WithMessage("*ClientId is required*");
    }

    [Fact]
    public void Register_WithValidNumber_UpdatesStatusToRegistered()
    {
        // Arrange
        var contract = CreateValidContract();
        var number = "CTR-20241201-123456";
        var agentId = Guid.NewGuid();

        // Act
        contract.Register(number, agentId);

        // Assert
        contract.Number.Should().Be(number);
        contract.Status.Should().Be(ContractStatus.Registered);
        contract.AgentId.Should().Be(agentId);
    }

    [Fact]
    public void Register_WithEmptyNumber_ThrowsValidationException()
    {
        // Arrange
        var contract = CreateValidContract();
        var agentId = Guid.NewGuid();

        // Act & Assert
        var act = () => contract.Register(string.Empty, agentId);
        act.Should().Throw<ValidationException>()
            .WithMessage("*Contract number cannot be empty*");
    }

    [Fact]
    public void MarkAsPaid_UpdatesIsPaidAndStatus()
    {
        // Arrange
        var contract = CreateValidContract();
        contract.Register("CTR-001", Guid.NewGuid());

        // Act
        contract.MarkAsPaid();

        // Assert
        contract.IsPaid.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Paid);
    }

    [Fact]
    public void Activate_FromRegisteredStatus_ThrowsDomainException()
    {
        // Arrange
        var contract = CreateValidContract();
        contract.Register("CTR-001", Guid.NewGuid());

        // Act & Assert
        var act = () => contract.Activate();
        act.Should().Throw<DomainException>()
            .WithMessage("*Contract must be Paid*");
    }

    [Fact]
    public void Activate_FromDraftStatus_ThrowsDomainException()
    {
        // Arrange
        var contract = CreateValidContract();

        // Act & Assert
        var act = () => contract.Activate();
        act.Should().Throw<DomainException>()
            .WithMessage("*Cannot activate contract from state Draft*");
    }

    [Fact]
    public void MarkOverdue_UpdatesStatusToOverdue()
    {
        // Arrange
        var contract = CreateValidContract();

        // Act
        contract.MarkOverdue();

        // Assert
        contract.Status.Should().Be(ContractStatus.Overdue);
    }

    [Fact]
    public void MarkProblematic_SetsIsFlaggedProblemAndStatus()
    {
        // Arrange
        var contract = CreateValidContract();
        var reason = "Payment overdue";

        // Act
        contract.MarkProblematic(reason);

        // Assert
        contract.IsFlaggedProblem.Should().BeTrue();
        contract.Status.Should().Be(ContractStatus.Problematic);
        contract.Notes.Should().Contain(reason);
    }

    [Fact]
    public void Expire_UpdatesStatusToExpired()
    {
        // Arrange
        var contract = CreateValidContract();

        // Act
        contract.Expire();

        // Assert
        contract.Status.Should().Be(ContractStatus.Expired);
    }

    [Fact]
    public void Renew_WithValidDates_UpdatesContract()
    {
        // Arrange
        var contract = CreateValidContract();
        contract.MarkProblematic("Test");
        var newStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));
        var newEnd = newStart.AddDays(365);
        var newPremium = new Money(15000m, "RUB");

        // Act
        contract.Renew(newStart, newEnd, newPremium);

        // Assert
        contract.StartDate.Should().Be(newStart);
        contract.EndDate.Should().Be(newEnd);
        contract.Premium.Should().Be(newPremium);
        contract.Status.Should().Be(ContractStatus.Registered);
        contract.IsFlaggedProblem.Should().BeFalse();
    }

    [Fact]
    public void Cancel_UpdatesStatusToCancelled()
    {
        // Arrange
        var contract = CreateValidContract();
        var reason = "Client request";

        // Act
        contract.Cancel(reason);

        // Assert
        contract.Status.Should().Be(ContractStatus.Cancelled);
        contract.Notes.Should().Contain(reason);
    }

    [Fact]
    public void AddPayment_AddsPaymentToCollection()
    {
        // Arrange
        var contract = CreateValidContract();
        var payment = new Payment(contract.Id, 10000m);

        // Act
        contract.AddPayment(payment);

        // Assert
        contract.Payments.Should().Contain(payment);
        contract.Payments.Count.Should().Be(1);
    }

    [Fact]
    public void AddPayment_WithNullPayment_ThrowsValidationException()
    {
        // Arrange
        var contract = CreateValidContract();

        // Act & Assert
        var act = () => contract.AddPayment(null!);
        act.Should().Throw<ValidationException>()
            .WithMessage("*payment is required*");
    }

    private static Contract CreateValidContract()
    {
        var clientId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = startDate.AddDays(365);
        var premium = new Money(10000m, "RUB");

        return new Contract(clientId, serviceId, startDate, endDate, premium);
    }
}

