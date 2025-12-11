using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.InsuranceService
{
    public class CreateServiceDto
    {
        [Required(ErrorMessage = "Название услуги обязательно")]
        [StringLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Name { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Описание не должно превышать 1000 символов")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Премия по умолчанию обязательна")]
        [Range(0.01, 100000000, ErrorMessage = "Премия должна быть больше 0")]
        public decimal DefaultPremium { get; set; }
    }
}

