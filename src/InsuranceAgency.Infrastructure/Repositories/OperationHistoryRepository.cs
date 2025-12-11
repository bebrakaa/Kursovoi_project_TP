using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class OperationHistoryRepository : IOperationHistoryRepository
{
    private readonly ApplicationDbContext _db;

    public OperationHistoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OperationHistory history)
    {
        await _db.OperationHistories.AddAsync(history);
    }

    public async Task<IEnumerable<OperationHistory>> GetByUserIdAsync(Guid userId)
    {
        return await _db.OperationHistories
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<OperationHistory>> GetAllAsync()
    {
        return await _db.OperationHistories
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}

