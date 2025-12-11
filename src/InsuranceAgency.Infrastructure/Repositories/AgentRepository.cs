using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class AgentRepository : IAgentRepository
{
    private readonly ApplicationDbContext _db;

    public AgentRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Agent?> GetByIdAsync(Guid id)
    {
        return await _db.Agents.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Agent>> GetAllAsync()
    {
        return await _db.Agents.ToListAsync();
    }

    public async Task<Agent?> GetByEmailAsync(string email)
    {
        return await _db.Agents.FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task AddAsync(Agent agent)
    {
        await _db.Agents.AddAsync(agent);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}

