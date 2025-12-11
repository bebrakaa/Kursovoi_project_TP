using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _db;

    public ClientRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyCollection<Client>> GetAllAsync()
    {
        var list = await _db.Clients.ToListAsync();
        return list.AsReadOnly();
    }

    public async Task AddAsync(Client client)
    {
        await _db.Clients.AddAsync(client);
    }

    public Task UpdateAsync(Client client)
    {
        _db.Clients.Update(client);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Client client)
    {
        _db.Clients.Remove(client);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
