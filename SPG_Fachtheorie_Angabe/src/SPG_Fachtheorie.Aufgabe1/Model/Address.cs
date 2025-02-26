namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Address
    {
        public required string Street { get; set; }
        public required string City { get; set; }
        public required string ZipCode { get; set; }
        public required string Country { get; set; }

        public Address(string street, string city, string zipCode, string country)
        {
            Street = street;
            City = city;
            ZipCode = zipCode;
            Country = country;
        }

        protected Address() { } // Für EF Core
    }


}