using System;
using System.Threading.Tasks;
using InsuranceAgency.Application.Common;
using InsuranceAgency.Application.DTOs.Payment;
using InsuranceAgency.Application.Interfaces.External;
using InsuranceAgency.Application.Interfaces.Repositories;
using InsuranceAgency.Application.Interfaces.Services;
using InsuranceAgency.Application.Common.Validation;
using InsuranceAgency.Domain.Entities;
using System.Linq;

namespace InsuranceAgency.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _payments;
        private readonly IContractRepository _contracts;
        private readonly IPaymentGateway _gateway;
        private readonly IDocumentVerificationRepository _verificationRepository;

        public PaymentService(
            IPaymentRepository payments,
            IContractRepository contracts,
            IPaymentGateway gateway,
            IDocumentVerificationRepository verificationRepository)
        {
            _payments = payments;
            _contracts = contracts;
            _gateway = gateway;
            _verificationRepository = verificationRepository;
        }

        public async Task<Result<PaymentResultDto>> InitiatePaymentAsync(InitiatePaymentDto dto)
        {
            var contract = await _contracts.GetByIdAsync(dto.ContractId);
            if (contract == null)
                return Result<PaymentResultDto>.Fail("Contract not found");

            var payment = new Payment(contract.Id, dto.Amount);
            await _payments.AddAsync(payment);
            await _payments.SaveChangesAsync();

            payment.MarkProcessing();
            await _payments.UpdateAsync(payment);
            await _payments.SaveChangesAsync();

            var result = await _gateway.ProcessPaymentAsync(
                dto.Amount,
                "RUB",
                payment.Id.ToString());

            if (!result.success)
            {
                payment.MarkFailed(result.error);
                await _payments.UpdateAsync(payment);
                await _payments.SaveChangesAsync();
                return Result<PaymentResultDto>.Fail(result.error ?? "Payment failed");
            }

            payment.MarkConfirmed(result.transactionId!);
            await _payments.UpdateAsync(payment);
            await _payments.SaveChangesAsync();

            contract.MarkAsPaid();
            
            // Активируем договор после оплаты, если дата начала уже наступила
            if (contract.StartDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                if (await MandatoryDataVerified(contract.ClientId))
            {
                contract.Activate();
                }
            }
            
            await _contracts.UpdateAsync(contract);
            await _contracts.SaveChangesAsync();

            return Result<PaymentResultDto>.Ok(new PaymentResultDto
            {
                Success = true,
                TransactionId = result.transactionId
            });
        }

        private async Task<bool> MandatoryDataVerified(Guid clientId)
        {
            var verifications = (await _verificationRepository.GetByClientIdAsync(clientId) ?? Enumerable.Empty<DocumentVerification>()).ToList();

            var missingRequired = VerificationRules.RequiredPersonalDataTypes
                .Where(requiredType => !verifications.Any(v =>
                    VerificationRules.IsSameType(v.DocumentType, requiredType) &&
                    v.Status == Domain.Enums.VerificationStatus.Approved))
                .ToList();

            return !missingRequired.Any();
        }
    }
}
