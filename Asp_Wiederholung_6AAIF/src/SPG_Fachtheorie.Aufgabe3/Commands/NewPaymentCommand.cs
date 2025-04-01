using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public class NewPaymentCommand
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Cash desk number must be greater than 0")]
        public int CashDeskNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Employee registration number must be greater than 0")]
        public int EmployeeRegistrationNumber { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty;
    }
}
