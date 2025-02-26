using System;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Payment
    {
        public int Id { get; set; }
        public DateTime PaymentDate { get; set; }

        public required CashDesk CashDesk { get; set; }  // Lösung für CS8618
        public List<PaymentItem> Items { get; set; } = new List<PaymentItem>();

        public Payment(CashDesk cashDesk, DateTime paymentDate)
        {
            CashDesk = cashDesk;
            PaymentDate = paymentDate;
        }

        protected Payment() { }  // Für EF Core
    }

}