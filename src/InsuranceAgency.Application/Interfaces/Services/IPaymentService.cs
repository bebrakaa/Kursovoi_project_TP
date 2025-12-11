using System;
using System.Threading.Tasks;
using InsuranceAgency.Application.Common;
using InsuranceAgency.Application.DTOs.Payment;

namespace InsuranceAgency.Application.Interfaces.Services
{
    public interface IPaymentService
    {
        Task<Result<PaymentResultDto>> InitiatePaymentAsync(InitiatePaymentDto dto);
    }
}
