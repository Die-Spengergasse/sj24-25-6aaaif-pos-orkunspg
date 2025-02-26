namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Manager : Employee
    {
        public Manager(string registrationNumber, string firstName, string lastName)
       : base(registrationNumber, firstName, lastName) { }

        protected Manager() { }
    }
}