using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;
using Xunit;



namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class Aufgabe1Test
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(@"Data Source=cash.db")
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        // Creates an empty DB in Debug\net8.0\cash.db
        [Fact]
        public void CreateDatabaseTest()
        {
            using var db = GetEmptyDbContext();
        }

        [Fact]
        public void AddCashierSuccessTest()
        {
            var options = new DbContextOptionsBuilder<AppointmentContext>()
     .UseSqlite("DataSource=:memory:")  // Nutze SQLite in-memory falls dein Prof das vorgibt
     .Options;


            using (var db = new AppointmentContext(options))
            {
                var cashier = new Cashier("12345", "John", "Doe");
                db.Employees.Add(cashier);
                db.SaveChanges();

                var savedCashier = db.Employees.OfType<Cashier>().FirstOrDefault(e => e.RegistrationNumber == "12345");
                Assert.NotNull(savedCashier);
            }

        }

        [Fact]
        public void AddPaymentSuccessTest()
        {

        }

        [Fact]
        public void EmployeeDiscriminatorSuccessTest()
        {

        }
    }
}