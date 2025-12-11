using InsuranceAgency.Application.Services;

namespace InsuranceAgency.Tests.Unit.Services;

public class NotificationServiceTests
{
    [Fact]
    public async Task SendAsync_WithValidParameters_CompletesSuccessfully()
    {
        // Arrange
        var service = new NotificationService();
        var email = "test@example.com";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        var act = async () => await service.SendAsync(email, subject, body);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_WithEmptyEmail_CompletesSuccessfully()
    {
        // Arrange
        var service = new NotificationService();
        var email = "";
        var subject = "Test Subject";
        var body = "Test Body";

        // Act
        var act = async () => await service.SendAsync(email, subject, body);

        // Assert
        // Сервис не валидирует email (это делается на уровне использования)
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_WithNullBody_CompletesSuccessfully()
    {
        // Arrange
        var service = new NotificationService();
        var email = "test@example.com";
        var subject = "Test Subject";
        string? body = null;

        // Act
        var act = async () => await service.SendAsync(email, subject, body!);

        // Assert
        // Сервис не валидирует body (это делается на уровне использования)
        await act.Should().NotThrowAsync();
    }
}


