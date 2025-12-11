using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IContractApplicationRepository
    {
        Task<ContractApplication?> GetByIdAsync(Guid id);
        Task<IEnumerable<ContractApplication>> GetAllAsync();
        Task<IEnumerable<ContractApplication>> GetByClientIdAsync(Guid clientId);
        Task<IEnumerable<ContractApplication>> GetByStatusAsync(ApplicationStatus status);
        Task AddAsync(ContractApplication application);
        Task UpdateAsync(ContractApplication application);
        Task SaveChangesAsync();
    }
}

