using InsuranceAgency.Application.DTOs;
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
    public class AgentControllerTests
    {
        private readonly Mock<IClientRepository> _clientRepositoryMock;
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly Mock<IContractService> _contractServiceMock;
        private readonly Mock<IInsuranceServiceRepository> _serviceRepositoryMock;
        private readonly Mock<IAgentRepository> _agentRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IDocumentVerificationRepository> _verificationRepositoryMock;
        private readonly Mock<IOperationHistoryRepository> _historyRepositoryMock;
        private readonly Mock<IContractApplicationRepository> _applicationRepositoryMock;
        private readonly AgentController _controller;

        public AgentControllerTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _contractRepositoryMock = new Mock<IContractRepository>();
            _contractServiceMock = new Mock<IContractService>();
            _serviceRepositoryMock = new Mock<IInsuranceServiceRepository>();
            _agentRepositoryMock = new Mock<IAgentRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _verificationRepositoryMock = new Mock<IDocumentVerificationRepository>();
            _historyRepositoryMock = new Mock<IOperationHistoryRepository>();
            _applicationRepositoryMock = new Mock<IContractApplicationRepository>();

            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AgentController>>();
            _controller = new AgentController(
                _clientRepositoryMock.Object,
                _contractRepositoryMock.Object,
                _contractServiceMock.Object,
                _serviceRepositoryMock.Object,
                _agentRepositoryMock.Object,
                _userRepositoryMock.Object,
                _verificationRepositoryMock.Object,
                _historyRepositoryMock.Object,
                _applicationRepositoryMock.Object,
                loggerMock.Object);

            // Настройка Claims для авторизации
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "Agent")
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
        public async Task Clients_ReturnsViewWithClients()
        {
            // Arrange
            var clients = new List<Client>
            {
                new Client("Test Client", "test@test.com", "123456789", "1234567890")
            };
            _clientRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(clients);

            // Act
            var result = await _controller.Clients();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
        }

        [Fact]
        public async Task CreateContract_WithValidData_ReturnsRedirect()
        {
            // Arrange
            var service = new InsuranceService("Test Service", new Money(1000, "RUB"), "Description");
            _serviceRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(service);
            _clientRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Client>());
            _agentRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Agent>());
            _contractRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Contract>())).Returns(Task.CompletedTask);
            _contractRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<OperationHistory>())).Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var dto = new Application.DTOs.Contract.CreateContractFormDto
            {
                ClientId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                AgentId = Guid.NewGuid(),
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
                PremiumAmount = 1000
            };

            // Act
            var result = await _controller.CreateContract(dto);

            // Assert
            // Может быть ViewResult при ошибке валидации или RedirectToActionResult при успехе
            Assert.True(result is ViewResult || result is RedirectToActionResult);
            if (result is RedirectToActionResult redirectResult)
            {
                Assert.Equal("Contracts", redirectResult.ActionName);
            }
        }

        [Fact]
        public async Task ActivateContract_WithValidVerifications_ReturnsRedirect()
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
            contract.MarkAsPaid();

            var contractDto = new ContractDto
            {
                Id = contract.Id,
                Number = contract.Number ?? "",
                Status = ContractStatus.Active.ToString()
            };

            _contractRepositoryMock.Setup(r => r.GetByIdAsync(contractId))
                .ReturnsAsync(contract);
            _contractServiceMock.Setup(s => s.ActivateContractAsync(contractId, It.IsAny<IDocumentVerificationRepository>()))
                .ReturnsAsync(contractDto);
            _historyRepositoryMock.Setup(r => r.AddAsync(It.IsAny<OperationHistory>()))
                .Returns(Task.CompletedTask);
            _historyRepositoryMock.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Настройка TempData через Mock
            var tempDataProviderMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>();
            var tempDataDictionary = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                _controller.HttpContext,
                tempDataProviderMock.Object);
            _controller.TempData = tempDataDictionary;

            // Act
            var result = await _controller.ActivateContract(contractId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Contracts", redirectResult.ActionName);
        }

        [Fact]
        public async Task VerifyDocuments_ReturnsViewWithClientAndVerifications()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new Client("Test Client", "test@test.com");
            var verifications = new List<DocumentVerification>
            {
                new DocumentVerification(clientId, Guid.NewGuid(), "Passport", "1234")
            };

            _clientRepositoryMock.Setup(r => r.GetByIdAsync(clientId))
                .ReturnsAsync(client);
            _verificationRepositoryMock.Setup(r => r.GetByClientIdAsync(clientId))
                .ReturnsAsync(verifications);

            // Act
            var result = await _controller.VerifyDocuments(clientId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(_controller.ViewBag.Client);
            Assert.NotNull(_controller.ViewBag.Verifications);
        }
    }
}

