using System;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<Payment?> GetByIdAsync(Guid id);
        Task<System.Collections.Generic.List<Payment>> GetByContractIdAsync(Guid contractId);
        Task<System.Collections.Generic.IEnumerable<Payment>> GetAllAsync();
        Task UpdateAsync(Payment payment);
        Task SaveChangesAsync();
    }
}
