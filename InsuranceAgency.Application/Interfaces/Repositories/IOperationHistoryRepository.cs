using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IOperationHistoryRepository
    {
        Task AddAsync(OperationHistory history);
        Task<IEnumerable<OperationHistory>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<OperationHistory>> GetAllAsync();
        Task SaveChangesAsync();
    }
}

