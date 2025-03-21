using System;
using System.ComponentModel.DataAnnotations;

public class NewPaymentCommand
{
    [Required]
    public int CashDeskNumber { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PaymentDateTime { get; set; }

    [Required]
    public string PaymentType { get; set; }

    [Required]
    public int EmployeeRegistrationNumber { get; set; }
}
