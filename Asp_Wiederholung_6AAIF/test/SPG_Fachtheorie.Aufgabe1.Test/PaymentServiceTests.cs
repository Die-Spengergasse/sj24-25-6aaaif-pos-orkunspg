using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Commands;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    [Collection("Sequential")]
    public class PaymentServiceTests
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

        public static TheoryData<NewPaymentCommand, string> InvalidPaymentData => new()
        {
            {
                new NewPaymentCommand { CashDeskNumber = 999, EmployeeRegistrationNumber = 1, PaymentType = "Cash" },
                "Cash desk not found."
            },
            {
                new NewPaymentCommand { CashDeskNumber = 1, EmployeeRegistrationNumber = 999, PaymentType = "Cash" },
                "Employee not found."
            },
            {
                new NewPaymentCommand { CashDeskNumber = 1, EmployeeRegistrationNumber = 1, PaymentType = "InvalidType" },
                "Payment type not recognized."
            },
            {
                new NewPaymentCommand { CashDeskNumber = 1, EmployeeRegistrationNumber = 2, PaymentType = "CreditCard" },
                "Insufficient rights to create a credit card payment."
            }
        };

        [Theory]
        [MemberData(nameof(InvalidPaymentData))]
        public void CreatePaymentExceptionsTest(NewPaymentCommand cmd, string expectedError)
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var cashier = new Cashier(2, "Cashier", "Test", null, "Spec1");
            db.CashDesks.Add(cashDesk);
            db.Employees.AddRange(manager, cashier);
            db.SaveChanges();

            var service = new PaymentService(db);

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.Equal(expectedError, ex.Message);
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand 
            { 
                CashDeskNumber = 1, 
                EmployeeRegistrationNumber = 1, 
                PaymentType = "CreditCard" 
            };

            // Act
            var payment = service.CreatePayment(cmd);

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(cashDesk.Number, payment.CashDesk.Number);
            Assert.Equal(manager.RegistrationNumber, payment.Employee.RegistrationNumber);
            Assert.Equal(PaymentType.CreditCard, payment.PaymentType);
            Assert.Null(payment.Confirmed);
        }

        [Fact]
        public void ConfirmPaymentSuccessTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash);
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);

            // Act
            service.ConfirmPayment(payment.Id);

            // Assert
            var confirmedPayment = db.Payments.Find(payment.Id);
            Assert.NotNull(confirmedPayment.Confirmed);
        }

        [Fact]
        public void ConfirmPaymentNotFoundTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var service = new PaymentService(db);

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(999));
            Assert.Equal("Payment not found.", ex.Message);
        }

        [Fact]
        public void ConfirmPaymentAlreadyConfirmedTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash)
            {
                Confirmed = DateTime.UtcNow
            };
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(payment.Id));
            Assert.Equal("Payment already confirmed.", ex.Message);
        }

        [Fact]
        public void AddPaymentItemSuccessTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash);
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand
            {
                PaymentId = payment.Id,
                ArticleName = "Test Article",
                Amount = 2,
                Price = 9.99m
            };

            // Act
            service.AddPaymentItem(cmd);

            // Assert
            var paymentWithItems = db.Payments
                .Include(p => p.PaymentItems)
                .First(p => p.Id == payment.Id);
            Assert.Single(paymentWithItems.PaymentItems);
            var item = paymentWithItems.PaymentItems.First();
            Assert.Equal("Test Article", item.ArticleName);
            Assert.Equal(2, item.Amount);
            Assert.Equal(9.99m, item.Price);
        }

        [Fact]
        public void AddPaymentItemPaymentNotFoundTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand
            {
                PaymentId = 999,
                ArticleName = "Test",
                Amount = 1,
                Price = 1.0m
            };

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment not found.", ex.Message);
        }

        [Fact]
        public void AddPaymentItemToConfirmedPaymentTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash)
            {
                Confirmed = DateTime.UtcNow
            };
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.SaveChanges();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand
            {
                PaymentId = payment.Id,
                ArticleName = "Test",
                Amount = 1,
                Price = 1.0m
            };

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal("Payment already confirmed.", ex.Message);
        }

        [Fact]
        public void DeletePaymentSuccessTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash);
            var paymentItem = new PaymentItem("Test", 1, 1.0m, payment);
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.PaymentItems.Add(paymentItem);
            db.SaveChanges();

            var service = new PaymentService(db);

            // Act
            service.DeletePayment(payment.Id, true);

            // Assert
            Assert.Empty(db.Payments);
            Assert.Empty(db.PaymentItems);
        }

        [Fact]
        public void DeletePaymentNotFoundTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var service = new PaymentService(db);

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.DeletePayment(999, true));
            Assert.Equal("Payment not found.", ex.Message);
        }

        [Fact]
        public void DeletePaymentWithItemsWithoutDeleteItemsFlagTest()
        {
            // Arrange
            using var db = GetEmptyDbContext();
            var cashDesk = new CashDesk(1);
            var manager = new Manager(1, "Manager", "Test", null, "Car1");
            var payment = new Payment(cashDesk, DateTime.UtcNow, manager, PaymentType.Cash);
            var paymentItem = new PaymentItem("Test", 1, 1.0m, payment);
            db.CashDesks.Add(cashDesk);
            db.Employees.Add(manager);
            db.Payments.Add(payment);
            db.PaymentItems.Add(paymentItem);
            db.SaveChanges();

            var service = new PaymentService(db);

            // Act & Assert
            var ex = Assert.Throws<PaymentServiceException>(() => service.DeletePayment(payment.Id, false));
            Assert.Equal("Payment has payment items.", ex.Message);
        }
    }
} 