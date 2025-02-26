namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Cashier : Employee
    {
        public Cashier(string registrationNumber, string firstName, string lastName)
            : base(registrationNumber, firstName, lastName) { }

        protected Cashier() { } // EF Core benötigt einen parameterlosen Konstruktor
    }
}
