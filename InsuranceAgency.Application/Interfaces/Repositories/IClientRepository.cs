using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IClientRepository
    {
        Task<Client?> GetByIdAsync(Guid id);
        Task<IReadOnlyCollection<Client>> GetAllAsync();
        Task AddAsync(Client client);
        Task UpdateAsync(Client client);
        Task DeleteAsync(Client client);
        Task SaveChangesAsync();
    }
}
