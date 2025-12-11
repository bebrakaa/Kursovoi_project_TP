using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.DocumentVerification
{
    public class CreateVerificationDto
    {
        [Required(ErrorMessage = "Тип документа обязателен")]
        [StringLength(100, ErrorMessage = "Тип документа не должен превышать 100 символов")]
        public string DocumentType { get; set; } = null!;

        [Required(ErrorMessage = "Номер документа обязателен")]
        [StringLength(100, ErrorMessage = "Номер документа не должен превышать 100 символов")]
        public string DocumentNumber { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Примечания не должны превышать 500 символов")]
        public string? Notes { get; set; }
    }
}

