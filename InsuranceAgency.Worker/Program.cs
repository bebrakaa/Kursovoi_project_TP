using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Application.Services;
using InsuranceAgency.Infrastructure.Persistence;
using InsuranceAgency.Infrastructure.Repositories;
using InsuranceAgency.Worker.Services;
using Microsoft.EntityFrameworkCore;
using InsuranceAgency.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IDocumentVerificationRepository, DocumentVerificationRepository>();

// Register Services
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register Worker Services
builder.Services.AddScoped<ProblematicContractsChecker>();

// Register Background Service
builder.Services.AddHostedService<ProblematicContractsWorker>();

var host = builder.Build();
host.Run();
