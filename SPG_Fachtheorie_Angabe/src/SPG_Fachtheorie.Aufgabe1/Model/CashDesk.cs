using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class CashDesk
    {
        public int Id { get; set; } // PK wird von der DB generiert

        public required string Location { get; set; }  // ✅ `required` stellt sicher, dass ein Wert gesetzt werden muss.

        public List<Payment> Payments { get; set; } = new List<Payment>();

        public CashDesk(string location)
        {
            Location = location;
        }

        protected CashDesk() { }  // Für EF Core
    }


}