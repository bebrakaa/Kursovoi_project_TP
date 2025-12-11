using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Application.Interfaces.External;
using InsuranceAgency.Application.Mapping;
using InsuranceAgency.Application.Services;
using InsuranceAgency.Infrastructure.External.PaymentGateway;
using InsuranceAgency.Infrastructure.Persistence;
using InsuranceAgency.Infrastructure.Repositories;
using InsuranceAgency.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Insurance Agency API",
        Version = "v1",
        Description = "API для управления страховым агентством",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Insurance Agency",
            Email = "support@insuranceagency.com"
        }
    });

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

// Register Repositories
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IInsuranceServiceRepository, InsuranceServiceRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IAgentRepository, AgentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOperationHistoryRepository, OperationHistoryRepository>();
builder.Services.AddScoped<IContractApplicationRepository, ContractApplicationRepository>();
builder.Services.AddScoped<IDocumentVerificationRepository, DocumentVerificationRepository>();

// Register Services
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Register External Services
builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();

// Configure Authentication & Authorization (JWT)
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection.GetValue<string>("Key") ?? "VeryStrongDevelopmentKey_ChangeInProduction_12345";
var issuer = jwtSection.GetValue<string>("Issuer") ?? "InsuranceAgency";
var audience = jwtSection.GetValue<string>("Audience") ?? "InsuranceAgencyClient";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Insurance Agency API v1");
        // Swagger будет доступен по /swagger, а корень сайта займут MVC-страницы
        c.RoutePrefix = "swagger";
    });
}

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

app.UseHttpsRedirection();

// Статические файлы (CSS, JS, изображения)
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// MVC маршрутизация для обычного сайта
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
