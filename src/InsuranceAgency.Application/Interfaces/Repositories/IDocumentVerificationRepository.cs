using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Enums;

namespace InsuranceAgency.Application.Interfaces.Repositories
{
    public interface IDocumentVerificationRepository
    {
        Task<DocumentVerification?> GetByIdAsync(Guid id);
        Task<IEnumerable<DocumentVerification>> GetAllAsync();
        Task<IEnumerable<DocumentVerification>> GetByClientIdAsync(Guid clientId);
        Task<IEnumerable<DocumentVerification>> GetByStatusAsync(VerificationStatus status);
        Task AddAsync(DocumentVerification verification);
        Task UpdateAsync(DocumentVerification verification);
        Task SaveChangesAsync();
    }
}

