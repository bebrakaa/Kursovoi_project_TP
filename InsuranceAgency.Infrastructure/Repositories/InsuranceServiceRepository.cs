using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class InsuranceServiceRepository : IInsuranceServiceRepository
{
    private readonly ApplicationDbContext _db;

    public InsuranceServiceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<InsuranceService?> GetByIdAsync(Guid id)
    {
        return await _db.InsuranceServices.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<InsuranceService>> GetAllAsync()
    {
        return await _db.InsuranceServices.ToListAsync();
    }

    public async Task AddAsync(InsuranceService service)
    {
        await _db.InsuranceServices.AddAsync(service);
    }

    public Task UpdateAsync(InsuranceService service)
    {
        _db.InsuranceServices.Update(service);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(InsuranceService service)
    {
        _db.InsuranceServices.Remove(service);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
