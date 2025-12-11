using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        [StringLength(100, ErrorMessage = "Email не должен превышать 100 символов")]
        public string Email { get; set; } = null!;

        [StringLength(200, ErrorMessage = "ФИО не должно превышать 200 символов")]
        [Display(Name = "ФИО")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Некорректный формат телефона")]
        [StringLength(20, ErrorMessage = "Телефон не должен превышать 20 символов")]
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }

        [StringLength(50, ErrorMessage = "Паспорт не должен превышать 50 символов")]
        [Display(Name = "Паспорт")]
        public string? Passport { get; set; }
    }
}

