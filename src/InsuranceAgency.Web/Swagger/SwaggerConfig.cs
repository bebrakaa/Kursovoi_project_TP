using Microsoft.OpenApi.Models;

namespace InsuranceAgency.Web.Swagger;

public static class SwaggerConfig
{
    public static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Insurance Agency API",
                Version = "v1",
                Description = "API для управления страховым агентством. " +
                              "Позволяет создавать договоры, управлять клиентами, обрабатывать платежи и управлять страховыми услугами.",
                Contact = new OpenApiContact
                {
                    Name = "Insurance Agency Support",
                    Email = "support@insuranceagency.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            // Add JWT Bearer authentication if needed in the future
            // c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            // {
            //     Description = "JWT Authorization header using the Bearer scheme",
            //     Name = "Authorization",
            //     In = ParameterLocation.Header,
            //     Type = SecuritySchemeType.ApiKey,
            //     Scheme = "Bearer"
            // });

            // Include XML comments
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
    }
}

