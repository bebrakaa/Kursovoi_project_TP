using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        _db.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user)
    {
        _db.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}


