using Microsoft.EntityFrameworkCore;
using MicroserviceBook;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

// Настройка БД (SQL Server LocalDB)
builder.Services.AddDbContext<LibraryContext>(opt =>
    opt.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=LibraryDB;Integrated Security=True"));

var app = builder.Build();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
