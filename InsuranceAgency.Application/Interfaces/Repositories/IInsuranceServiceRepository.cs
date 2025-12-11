using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IInsuranceServiceRepository
    {
        Task<InsuranceService?> GetByIdAsync(Guid id);
        Task<List<InsuranceService>> GetAllAsync();
        Task AddAsync(InsuranceService service);
        Task UpdateAsync(InsuranceService service);
        Task DeleteAsync(InsuranceService service);
        Task SaveChangesAsync();
    }
}
