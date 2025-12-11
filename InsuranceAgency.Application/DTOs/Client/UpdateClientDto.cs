using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.Client
{
    public class UpdateClientDto
    {
        [Required(ErrorMessage = "ФИО обязательно")]
        [StringLength(200, ErrorMessage = "ФИО не должно превышать 200 символов")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        public string Email { get; set; } = null!;

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "Паспорт не должен превышать 50 символов")]
        public string? Passport { get; set; }
    }
}

