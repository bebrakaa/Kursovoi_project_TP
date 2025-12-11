using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InsuranceAgency.Application.DTOs;
using InsuranceAgency.Application.Interfaces.Repositories;

namespace InsuranceAgency.Application.Interfaces.Services
{
    public interface IContractService
    {
        Task<ContractDto> CreateContractAsync(CreateContractDto dto, Guid agentId);
        Task<ContractDto> RegisterContractAsync(Guid contractId, string number, Guid agentId);
        Task<ContractDto> ActivateContractAsync(Guid contractId, IDocumentVerificationRepository verificationRepository);

        Task<IEnumerable<ContractDto>> GetAllAsync();
        Task<ContractDto?> GetByIdAsync(Guid id);
    }
}
