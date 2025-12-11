using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IAgentRepository
    {
        Task<Agent?> GetByIdAsync(Guid id);
        Task<IEnumerable<Agent>> GetAllAsync();
        Task<Agent?> GetByEmailAsync(string email);
        Task AddAsync(Agent agent);
        Task SaveChangesAsync();
    }
}
