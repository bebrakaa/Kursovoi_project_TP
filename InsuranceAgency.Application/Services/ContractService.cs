using System;
using InsuranceAgency.Application.Common.Exceptions;
using InsuranceAgency.Application.Common.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using InsuranceAgency.Application.DTOs;
using InsuranceAgency.Application.DTOs.Contract;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Domain.Entities;
using InsuranceAgency.Domain.Exceptions;
using InsuranceAgency.Domain.ValueObjects;

namespace InsuranceAgency.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IInsuranceServiceRepository _serviceRepository;
        private readonly IAgentRepository _agentRepository;
        private readonly IMapper _mapper;

        public ContractService(
            IContractRepository contractRepository,
            IClientRepository clientRepository,
            IInsuranceServiceRepository serviceRepository,
            IAgentRepository agentRepository,
            IMapper mapper)
        {
            _contractRepository = contractRepository;
            _clientRepository = clientRepository;
            _serviceRepository = serviceRepository;
            _agentRepository = agentRepository;
            _mapper = mapper;
        }

        public async Task<ContractDto> CreateContractAsync(CreateContractDto dto, Guid agentId)
        {
            // Проверка существования клиента
            var client = await _clientRepository.GetByIdAsync(dto.ClientId);
            if (client == null)
                throw new NotFoundException($"Client {dto.ClientId} not found");

            // Проверка существования услуги
            var service = await _serviceRepository.GetByIdAsync(dto.ServiceId);
            if (service == null)
                throw new NotFoundException($"Insurance service {dto.ServiceId} not found");

            // Проверка агента
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
                throw new NotFoundException($"Agent {agentId} not found");

            var contract = new Contract(
                clientId: dto.ClientId,
                serviceId: dto.ServiceId,
                DateOnly.FromDateTime(dto.StartDate),
                DateOnly.FromDateTime(dto.EndDate),
                premium: new Money(dto.PremiumAmount, dto.PremiumCurrency),
                agentId: agentId,
                notes: dto.Notes
            );

            // Присвоим номер договора (простая генерация)
            string number = $"CTR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6]}";
            contract.Register(number, agentId);

            await _contractRepository.AddAsync(contract);
            await _contractRepository.SaveChangesAsync();

            return _mapper.Map<ContractDto>(contract);
        }

        public async Task<ContractDto> RegisterContractAsync(Guid contractId, string number, Guid agentId)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new NotFoundException($"Contract {contractId} not found");

            contract.Register(number, agentId);
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            return _mapper.Map<ContractDto>(contract);
        }

        public async Task<IEnumerable<ContractDto>> GetAllAsync()
        {
            var contracts = await _contractRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ContractDto>>(contracts);
        }

        public async Task<ContractDto?> GetByIdAsync(Guid id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            return contract == null ? null : _mapper.Map<ContractDto>(contract);
        }

        public async Task<ContractDto> ActivateContractAsync(Guid contractId, IDocumentVerificationRepository verificationRepository)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new NotFoundException($"Contract {contractId} not found");

            var verifications = (await verificationRepository.GetByClientIdAsync(contract.ClientId)).ToList();

            // Проверяем обязательные персональные данные
            var pendingRequired = verifications
                .Where(v => VerificationRules.IsRequiredType(v.DocumentType) &&
                            v.Status == Domain.Enums.VerificationStatus.Pending)
                .ToList();
            if (pendingRequired.Any())
            {
                var pendingNames = string.Join(", ", pendingRequired.Select(v => v.DocumentType));
                throw new DomainException($"Нельзя активировать договор: ожидает верификации обязательные данные ({pendingNames})");
            }

            var missingRequired = VerificationRules.RequiredPersonalDataTypes
                .Where(requiredType => !verifications.Any(v =>
                    VerificationRules.IsSameType(v.DocumentType, requiredType) &&
                    v.Status == Domain.Enums.VerificationStatus.Approved))
                .ToList();
            if (missingRequired.Any())
            {
                throw new DomainException($"Нельзя активировать договор: нет одобренных обязательных данных ({string.Join(", ", missingRequired)})");
            }

            // Проверяем, что есть хотя бы один одобренный документ (любого типа)
            var approvedVerifications = verifications.Where(v => v.Status == Domain.Enums.VerificationStatus.Approved).ToList();
            if (!approvedVerifications.Any())
            {
                throw new DomainException("Нельзя активировать договор: нет одобренных документов клиента");
            }

            contract.Activate();
            await _contractRepository.UpdateAsync(contract);
            await _contractRepository.SaveChangesAsync();

            return _mapper.Map<ContractDto>(contract);
        }
    }
}
