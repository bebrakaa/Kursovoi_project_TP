using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _db.Payments
            .Include(p => p.Contract)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Payment payment)
    {
        await _db.Payments.AddAsync(payment);
    }

    public async Task<List<Payment>> GetByContractIdAsync(Guid contractId)
    {
        return await _db.Payments
            .Where(p => p.ContractId == contractId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _db.Payments
            .Include(p => p.Contract)
            .ToListAsync();
    }

    public Task UpdateAsync(Payment payment)
    {
        _db.Payments.Update(payment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
