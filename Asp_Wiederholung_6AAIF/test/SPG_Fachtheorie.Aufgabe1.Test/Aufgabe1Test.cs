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
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1, "fn", "ln", null, "Wursttheke");
            db.Cashiers.Add(cashier);
            
            // ACT
            db.SaveChanges();   // erst hier wird das INSERT INTO gesendet.

            // ASSERT
            db.ChangeTracker.Clear();
            var cashierFromDb = db.Cashiers.First();
            Assert.True(cashierFromDb.RegistrationNumber == 1);
        }

        [Fact]
        public void AddPaymentSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var employee = new Cashier(1001, "fn", "ln", null, "Kassier");
            var cashDesk = new CashDesk(1);
            var payment = new Payment(
                cashDesk, new DateTime(2025, 2, 21), employee, PaymentType.Cash);
            db.Payments.Add(payment);

            // ACT
            db.SaveChanges();

            // ASSERT
            db.ChangeTracker.Clear();
            var paymentFromDb = db.Payments.First();
            Assert.True(paymentFromDb.Id != 0);
        }

        [Fact]
        public void EmployeeDiscriminatorSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var manager = new Manager(1001, "fn", "ln", null, "BMW X7");
            db.Managers.Add(manager);

            // ACT
            db.SaveChanges();

            // ASSERT
            db.ChangeTracker.Clear();
            var managerFromDb = db.Managers.First();
            Assert.True(managerFromDb.Type == "Manager");
        }
    }
}