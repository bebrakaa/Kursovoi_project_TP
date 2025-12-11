using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly ApplicationDbContext _db;

    public ContractRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Contract?> GetByIdAsync(Guid id)
    {
        return await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyCollection<Contract>> GetAllAsync()
    {
        var list = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task AddAsync(Contract contract)
    {
        await _db.Contracts.AddAsync(contract);
    }

    public Task UpdateAsync(Contract contract)
    {
        _db.Contracts.Update(contract);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<Contract>> GetOverdueContractsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var list = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .Where(c => c.EndDate < today 
                && c.Status != ContractStatus.Expired 
                && c.Status != ContractStatus.Cancelled
                && c.Status != ContractStatus.Completed)
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Contract>> GetUnpaidContractsAsync(TimeSpan overdueThreshold)
    {
        var thresholdDate = DateTime.UtcNow - overdueThreshold;
        var list = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .Where(c => !c.IsPaid 
                && (c.Status == ContractStatus.Registered || c.Status == ContractStatus.PendingPayment)
                && c.CreatedAt < thresholdDate)
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Contract>> GetContractsRequiringRenewalAsync(int daysBeforeExpiration)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expirationDate = today.AddDays(daysBeforeExpiration);
        
        var list = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .Where(c => c.EndDate >= today 
                && c.EndDate <= expirationDate
                && c.Status == ContractStatus.Active
                && !c.IsFlaggedProblem)
            .ToListAsync();

        return list.AsReadOnly();
    }

    public async Task<IReadOnlyCollection<Contract>> GetExpiredContractsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var list = await _db.Contracts
            .Include(c => c.Client)
            .Include(c => c.Agent)
            .Include(c => c.Service)
            .Where(c => c.EndDate < today 
                && c.Status != ContractStatus.Expired
                && c.Status != ContractStatus.Cancelled
                && c.Status != ContractStatus.Completed)
            .ToListAsync();

        return list.AsReadOnly();
    }
}
