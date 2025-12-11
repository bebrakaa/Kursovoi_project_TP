using System.ComponentModel.DataAnnotations;

namespace InsuranceAgency.Application.DTOs.Contract
{
    public class CreateContractFormDto
    {
        [Required(ErrorMessage = "Клиент обязателен")]
        public Guid ClientId { get; set; }

        [Required(ErrorMessage = "Услуга обязательна")]
        public Guid ServiceId { get; set; }

        [Required(ErrorMessage = "Агент обязателен")]
        public Guid AgentId { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна")]
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Дата окончания обязательна")]
        [DataType(DataType.Date)]
        public DateOnly EndDate { get; set; }

        [Required(ErrorMessage = "Премия обязательна")]
        [Range(0.01, 100000000, ErrorMessage = "Премия должна быть больше 0")]
        public decimal PremiumAmount { get; set; }

        [StringLength(1000, ErrorMessage = "Примечания не должны превышать 1000 символов")]
        public string? Notes { get; set; }
    }
}

