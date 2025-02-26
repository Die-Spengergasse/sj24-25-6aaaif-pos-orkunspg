using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public abstract class Employee
    {
        [Key]
        public string RegistrationNumber { get; private set; }  // Primärschlüssel
        public string Firstname { get; private set; }
        public string Lastname { get; private set; }

        // Hauptkonstruktor
        protected Employee(string registrationNumber, string firstname, string lastname)
        {
            RegistrationNumber = registrationNumber ?? throw new ArgumentNullException(nameof(registrationNumber));
            Firstname = firstname ?? throw new ArgumentNullException(nameof(firstname));
            Lastname = lastname ?? throw new ArgumentNullException(nameof(lastname));
        }

        // EF Core benötigt einen parameterlosen Konstruktor:
        protected Employee()
        {
            RegistrationNumber = "00000";  // Standardwert setzen
            Firstname = "Unknown";  // Standardwert setzen
            Lastname = "Unknown";  // Standardwert setzen
        }
    }
}
