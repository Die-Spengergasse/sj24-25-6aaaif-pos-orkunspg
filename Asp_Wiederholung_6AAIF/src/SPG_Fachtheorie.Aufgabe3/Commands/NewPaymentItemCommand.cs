using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public class NewPaymentItemCommand
    {
        [Required]
        [MinLength(1, ErrorMessage = "Article name cannot be empty")]
        public string ArticleName { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public int Amount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Payment ID must be greater than 0")]
        public int PaymentId { get; set; }
    }
} 