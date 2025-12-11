using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Tests.Unit.Domain;

public class DocumentVerificationTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesVerification()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var documentType = "Passport";
        var documentNumber = "1234 567890";

        // Act
        var verification = new DocumentVerification(
            clientId,
            agentId,
            documentType,
            documentNumber,
            "Test notes");

        // Assert
        verification.Should().NotBeNull();
        verification.ClientId.Should().Be(clientId);
        verification.VerifiedByAgentId.Should().Be(agentId);
        verification.DocumentType.Should().Be(documentType);
        verification.DocumentNumber.Should().Be(documentNumber);
        verification.Status.Should().Be(VerificationStatus.Pending);
        verification.Notes.Should().Be("Test notes");
    }

    [Fact]
    public void Constructor_WithEmptyClientId_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => new DocumentVerification(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*ClientId is required*");
    }

    [Fact]
    public void Constructor_WithoutAgentId_CreatesVerification()
    {
        // Arrange
        var clientId = Guid.NewGuid();

        // Act
        var verification = new DocumentVerification(clientId);

        // Assert
        verification.Should().NotBeNull();
        verification.VerifiedByAgentId.Should().BeNull();
        verification.Status.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public void Approve_WithValidAgentId_UpdatesStatusToApproved()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var verification = new DocumentVerification(clientId, null, "Passport", "1234");

        // Act
        verification.Approve(agentId, "Approved by agent");

        // Assert
        verification.Status.Should().Be(VerificationStatus.Approved);
        verification.VerifiedByAgentId.Should().Be(agentId);
        verification.Notes.Should().Contain("Approved by agent");
    }

    [Fact]
    public void Approve_WithEmptyAgentId_ThrowsArgumentException()
    {
        // Arrange
        var verification = new DocumentVerification(Guid.NewGuid());

        // Act & Assert
        var act = () => verification.Approve(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AgentId is required*");
    }

    [Fact]
    public void Approve_WithExistingNotes_AppendsNotes()
    {
        // Arrange
        var verification = new DocumentVerification(
            Guid.NewGuid(),
            null,
            "Passport",
            "1234",
            "Initial note");
        var agentId = Guid.NewGuid();

        // Act
        verification.Approve(agentId, "Additional note");

        // Assert
        verification.Notes.Should().Contain("Initial note");
        verification.Notes.Should().Contain("Additional note");
    }

    [Fact]
    public void Reject_WithValidReason_UpdatesStatusToRejected()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var verification = new DocumentVerification(clientId, null, "Passport", "1234");
        var reason = "Document expired";

        // Act
        verification.Reject(agentId, reason);

        // Assert
        verification.Status.Should().Be(VerificationStatus.Rejected);
        verification.VerifiedByAgentId.Should().Be(agentId);
        verification.Notes.Should().Contain($"Rejected: {reason}");
    }

    [Fact]
    public void Reject_WithEmptyReason_ThrowsArgumentException()
    {
        // Arrange
        var verification = new DocumentVerification(Guid.NewGuid());
        var agentId = Guid.NewGuid();

        // Act & Assert
        var act = () => verification.Reject(agentId, string.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Rejection reason is required*");
    }

    [Fact]
    public void Reject_WithEmptyAgentId_ThrowsArgumentException()
    {
        // Arrange
        var verification = new DocumentVerification(Guid.NewGuid());

        // Act & Assert
        var act = () => verification.Reject(Guid.Empty, "Reason");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AgentId is required*");
    }

    [Fact]
    public void AssignAgent_SetsAgentId()
    {
        // Arrange
        var verification = new DocumentVerification(Guid.NewGuid());
        var agentId = Guid.NewGuid();

        // Act
        verification.AssignAgent(agentId);

        // Assert
        verification.VerifiedByAgentId.Should().Be(agentId);
    }

    [Fact]
    public void AssignAgent_WithEmptyAgentId_ThrowsArgumentException()
    {
        // Arrange
        var verification = new DocumentVerification(Guid.NewGuid());

        // Act & Assert
        var act = () => verification.AssignAgent(Guid.Empty);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*AgentId is required*");
    }
}


