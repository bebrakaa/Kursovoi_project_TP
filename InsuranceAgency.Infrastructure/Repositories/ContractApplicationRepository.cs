using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class ContractApplicationRepository : IContractApplicationRepository
{
    private readonly ApplicationDbContext _db;

    public ContractApplicationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ContractApplication?> GetByIdAsync(Guid id)
    {
        return await _db.ContractApplications
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Include(a => a.ProcessedByAgent)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<ContractApplication>> GetAllAsync()
    {
        return await _db.ContractApplications
            .Include(a => a.Client)
            .Include(a => a.Service)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractApplication>> GetByClientIdAsync(Guid clientId)
    {
        return await _db.ContractApplications
            .Include(a => a.Service)
            .Where(a => a.ClientId == clientId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ContractApplication>> GetByStatusAsync(ApplicationStatus status)
    {
        return await _db.ContractApplications
            .Include(a => a.Client)
            .Include(a => a.Service)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(ContractApplication application)
    {
        await _db.ContractApplications.AddAsync(application);
    }

    public Task UpdateAsync(ContractApplication application)
    {
        _db.ContractApplications.Update(application);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}

