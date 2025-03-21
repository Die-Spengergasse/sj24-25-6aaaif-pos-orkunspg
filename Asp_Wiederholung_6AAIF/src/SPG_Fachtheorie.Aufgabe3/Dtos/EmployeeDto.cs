using SPG_Fachtheorie.Aufgabe1.Model;

namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    public record EmployeeDto(
        int RegistrationNumber, 
        string Type,
        string LastName, string FirstName);

    public record EmployeeDetailDto(
        int RegistrationNumber, string LastName, string FirstName,
        Address? Address,
        int PaymentCount);

}
