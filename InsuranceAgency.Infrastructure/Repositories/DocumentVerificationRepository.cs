using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;
using InsuranceAgency.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceAgency.Infrastructure.Repositories;

public class DocumentVerificationRepository : IDocumentVerificationRepository
{
    private readonly ApplicationDbContext _db;

    public DocumentVerificationRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<DocumentVerification?> GetByIdAsync(Guid id)
    {
        return await _db.DocumentVerifications
            .Include(v => v.Client)
            .Include(v => v.VerifiedByAgent)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<DocumentVerification>> GetAllAsync()
    {
        return await _db.DocumentVerifications
            .Include(v => v.Client)
            .Include(v => v.VerifiedByAgent)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentVerification>> GetByClientIdAsync(Guid clientId)
    {
        return await _db.DocumentVerifications
            .Include(v => v.VerifiedByAgent)
            .Where(v => v.ClientId == clientId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<DocumentVerification>> GetByStatusAsync(VerificationStatus status)
    {
        return await _db.DocumentVerifications
            .Include(v => v.Client)
            .Include(v => v.VerifiedByAgent)
            .Where(v => v.Status == status)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(DocumentVerification verification)
    {
        await _db.DocumentVerifications.AddAsync(verification);
    }

    public Task UpdateAsync(DocumentVerification verification)
    {
        _db.DocumentVerifications.Update(verification);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}

