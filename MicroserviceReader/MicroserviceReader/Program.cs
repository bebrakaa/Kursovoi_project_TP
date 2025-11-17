using Microsoft.EntityFrameworkCore;
using MicroserviceReader.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddDbContext<ReaderContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=readers_db;Username=postgres;Password=qazzaq123321"));

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
