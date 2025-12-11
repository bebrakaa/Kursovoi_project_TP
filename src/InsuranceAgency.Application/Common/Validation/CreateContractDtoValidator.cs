using FluentValidation;
using InsuranceAgency.Application.DTOs;

namespace InsuranceAgency.Application.Common.Validation
{
    public class CreateContractDtoValidator : AbstractValidator<CreateContractDto>
    {
        public CreateContractDtoValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId обязателен.");

            RuleFor(x => x.ServiceId)
                .NotEmpty().WithMessage("ServiceId обязателен.");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Дата начала должна быть раньше даты окончания.");

            RuleFor(x => x.PremiumAmount)
                .GreaterThan(0).WithMessage("Страховая премия должна быть больше нуля.");

            RuleFor(x => x.PremiumCurrency)
                .NotEmpty().WithMessage("Валюта обязательна.");
        }
    }
}
