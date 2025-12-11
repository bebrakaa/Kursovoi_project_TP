using InsuranceAgency.Application.Common;
using InsuranceAgency.Application.DTOs.Payment;
using InsuranceAgency.Application.Interfaces.External;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.ValueObjects;
using Moq;

namespace InsuranceAgency.Tests.Unit;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<IContractRepository> _contractRepositoryMock;
    private readonly Mock<IPaymentGateway> _gatewayMock;
    private readonly Mock<IDocumentVerificationRepository> _verificationRepositoryMock;
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _contractRepositoryMock = new Mock<IContractRepository>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();

        _service = new PaymentService(
            _paymentRepositoryMock.Object,
            _contractRepositoryMock.Object,
            _gatewayMock.Object,
            _verificationRepositoryMock.Object);

        _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid clientId) => new List<DocumentVerification>
            {
                ApprovedVerification(clientId, "FullName"),
                ApprovedVerification(clientId, "Passport")
            });
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithValidData_ReturnsSuccess()
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

        var transactionId = Guid.NewGuid().ToString();
        var dto = new InitiatePaymentDto
        {
            ContractId = contractId,
            Amount = 10000m
        };

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);
        _paymentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);
        _paymentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);
        _paymentRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);
        _contractRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _gatewayMock.Setup(g => g.ProcessPaymentAsync(10000m, "RUB", It.IsAny<string>()))
            .ReturnsAsync((true, transactionId, null));

        // Act
        var result = await _service.InitiatePaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Success.Should().BeTrue();
        result.Value.TransactionId.Should().Be(transactionId);

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Once);
        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Exactly(2));
        _contractRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Contract>()), Times.Once);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithNonExistentContract_ReturnsFailure()
    {
        // Arrange
        var contractId = Guid.NewGuid();
        var dto = new InitiatePaymentDto
        {
            ContractId = contractId,
            Amount = 10000m
        };

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync((Contract?)null);

        // Act
        var result = await _service.InitiatePaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Contract not found");

        _paymentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithFailedGateway_ReturnsFailure()
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

        var dto = new InitiatePaymentDto
        {
            ContractId = contractId,
            Amount = 10000m
        };

        _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
            .ReturnsAsync(contract);
        _paymentRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);
        _paymentRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);
        _paymentRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _gatewayMock.Setup(g => g.ProcessPaymentAsync(10000m, "RUB", It.IsAny<string>()))
            .ReturnsAsync((false, (string?)null, "Insufficient funds"));

        // Act
        var result = await _service.InitiatePaymentAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Insufficient funds");

        _paymentRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Payment>()), Times.Exactly(2));
    }

    private static DocumentVerification ApprovedVerification(Guid clientId, string documentType)
    {
        var verification = new DocumentVerification(
            clientId,
            Guid.NewGuid(),
            documentType,
            "value",
            null);
        verification.Approve(Guid.NewGuid());
        return verification;
    }
}

