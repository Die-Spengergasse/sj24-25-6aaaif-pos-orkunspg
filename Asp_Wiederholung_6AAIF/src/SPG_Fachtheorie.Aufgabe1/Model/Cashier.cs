using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Cashier : Employee
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Cashier() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Cashier(
            int registrationNumber, string firstName, string lastName,
            Address? address, string jobSpezialisation) : base(registrationNumber, firstName, lastName, address)
        {
            JobSpezialisation = jobSpezialisation;
        }

        [MaxLength(255)]
        public string JobSpezialisation { get; set; }
    }
}