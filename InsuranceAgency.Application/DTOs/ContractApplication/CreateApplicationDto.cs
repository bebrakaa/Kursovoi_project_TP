using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.ContractApplication
{
    public class CreateApplicationDto
    {
        [Required(ErrorMessage = "Услуга обязательна")]
        public Guid ServiceId { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        [DataType(DataType.Date)]
        public DateTime DesiredStartDate { get; set; }

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [DataType(DataType.Date)]
        public DateTime DesiredEndDate { get; set; }

        [Required(ErrorMessage = "Премия обязательна")]
        [Range(0.01, 100000000, ErrorMessage = "Премия должна быть больше 0")]
        public decimal DesiredPremium { get; set; }

        [StringLength(1000, ErrorMessage = "Примечания не должны превышать 1000 символов")]
        public string? Notes { get; set; }
    }
}

