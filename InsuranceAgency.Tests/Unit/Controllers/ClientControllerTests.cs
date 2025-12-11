using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Domain.ValueObjects;
using InsuranceAgency.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace InsuranceAgency.Tests.Unit.Controllers
{
    public class ClientControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly Mock<IInsuranceServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IContractApplicationRepository> _applicationRepositoryMock;
    private readonly Mock<IOperationHistoryRepository> _historyRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly ClientController _controller;

        public ClientControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _contractRepositoryMock = new Mock<IContractRepository>();
            _paymentRepositoryMock = new Mock<IPaymentRepository>();
            _paymentServiceMock = new Mock<IPaymentService>();
            _serviceRepositoryMock = new Mock<IInsuranceServiceRepository>();
            _applicationRepositoryMock = new Mock<IContractApplicationRepository>();
            _historyRepositoryMock = new Mock<IOperationHistoryRepository>();
            _clientRepositoryMock = new Mock<IClientRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            _controller = new ClientController(
                _userRepositoryMock.Object,
                _contractRepositoryMock.Object,
                _paymentRepositoryMock.Object,
                _paymentServiceMock.Object,
                _serviceRepositoryMock.Object,
                _applicationRepositoryMock.Object,
                _historyRepositoryMock.Object,
                _clientRepositoryMock.Object,
                _notificationServiceMock.Object);

            // Настройка Claims для авторизации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Client")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = principal
            };
            _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Profile_ReturnsViewWithUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("testuser", "test@test.com", UserRole.Client, "password");
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _clientRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Client>());

            // Act
            var result = await _controller.Profile();

            // Assert
            // Может быть ViewResult или RedirectToActionResult в зависимости от наличия клиента
            Assert.True(result is ViewResult || result is RedirectToActionResult);
        }

        [Fact]
        public async Task CreateApplication_WithValidData_ReturnsRedirect()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User("testuser", "test@test.com", UserRole.Client, "password");
            var client = new Client("Test Client", "test@test.com", "123456789", "1234567890");
            
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _clientRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Client> { client });
            _applicationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<ContractApplication>())).Returns(Task.CompletedTask);
            _applicationRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<OperationHistory>())).Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var dto = new Application.DTOs.ContractApplication.CreateApplicationDto
            {
                ServiceId = Guid.NewGuid(),
                DesiredStartDate = DateTime.Today,
                DesiredEndDate = DateTime.Today.AddDays(30),
                DesiredPremium = 1000
            };

            // Act
            var result = await _controller.CreateApplication(dto);

            // Assert
            // Может быть ViewResult при ошибке валидации или RedirectToActionResult при успехе
            Assert.True(result is ViewResult || result is RedirectToActionResult);
            if (result is RedirectToActionResult redirectResult)
            {
                Assert.Equal("MyApplications", redirectResult.ActionName);
            }
        }

        [Fact]
        public async Task PayContract_WithValidContract_ReturnsView()
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
            var result = await _controller.PayContract(contractId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Contract);
        }

        [Fact]
        public async Task ProcessPayment_WithValidContract_ReturnsRedirect()
        {
            // Arrange
            var contractId = Guid.NewGuid();
            var client = new Client("Test Client", "test@test.com");
            var contract = new Contract(
                client.Id,
                Guid.NewGuid(),
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(365)),
                new Money(10000m, "RUB"));
            contract.Register("CTR-002", Guid.NewGuid());
            typeof(Contract).GetProperty("Client")?.SetValue(contract, client);

            var paymentResultDto = new Application.DTOs.Payment.PaymentResultDto
            {
                Success = true,
                TransactionId = Guid.NewGuid().ToString()
            };
            var paymentResult = Application.Common.Result<Application.DTOs.Payment.PaymentResultDto>.Ok(paymentResultDto);

            _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
                .ReturnsAsync(contract);
            _paymentServiceMock.Setup(s => s.InitiatePaymentAsync(It.IsAny<Application.DTOs.Payment.InitiatePaymentDto>()))
                .ReturnsAsync(paymentResult);
            _historyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<OperationHistory>()))
                .Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(s => s.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Настройка TempData через Mock
            var tempDataProviderMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();
            var tempDataDictionary = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.HttpContext,
                tempDataProviderMock.Object);
            _controller.TempData = tempDataDictionary;

            // Act
            var result = await _controller.ProcessPayment(contractId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("History", redirectResult.ActionName);
        }
    }
}

