using FluentValidation;
using InsuranceAgency.Application.DTOs;

namespace InsuranceAgency.Application.Common.Validation
{
    public class RegisterContractDtoValidator : AbstractValidator<RegisterContractDto>
    {
        public RegisterContractDtoValidator()
        {
            RuleFor(x => x.ContractId)
                .NotEmpty().WithMessage("ContractId обязателен.");

            RuleFor(x => x.Number)
                .NotEmpty().WithMessage("Номер договора обязателен.");

            RuleFor(x => x.AgentId)
                .NotEmpty().WithMessage("AgentId обязателен.");
        }
    }
}
