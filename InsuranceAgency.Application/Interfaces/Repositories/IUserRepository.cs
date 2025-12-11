using System;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories;

/// <summary>
/// Репозиторий для работы с пользователями системы.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task SaveChangesAsync();
}


