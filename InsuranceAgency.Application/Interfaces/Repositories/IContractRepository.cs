using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(Guid id);
        Task AddAsync(Contract contract);
        Task UpdateAsync(Contract contract);
        Task<IReadOnlyCollection<Contract>> GetAllAsync();
        Task SaveChangesAsync();
        
        // Methods for problematic contracts checking
        Task<IReadOnlyCollection<Contract>> GetOverdueContractsAsync();
        Task<IReadOnlyCollection<Contract>> GetUnpaidContractsAsync(TimeSpan overdueThreshold);
        Task<IReadOnlyCollection<Contract>> GetContractsRequiringRenewalAsync(int daysBeforeExpiration);
        Task<IReadOnlyCollection<Contract>> GetExpiredContractsAsync();
    }
}
