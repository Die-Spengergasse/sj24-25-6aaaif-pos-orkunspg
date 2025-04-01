using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Commands;
using System;
using Xunit;

namespace SPG_Fachtheorie.Test
{
    public class PaymentServiceTests
    {
        private AppointmentContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new AppointmentContext(options);
            SeedDatabase(context);
            return context;
        }

        private void SeedDatabase(AppointmentContext context)
        {
            // Create test data
            var manager = new Manager
            {
                RegistrationNumber = 1,
                FirstName = "John",
                LastName = "Doe",
                Type = "Manager",
                CarType = "SUV"
            };

            var cashier = new Cashier
            {
                RegistrationNumber = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Type = "Cashier",
                JobSpezialisation = "Main"
            };

            var cashDesk = new CashDesk
            {
                Number = 1
            };

            context.Managers.Add(manager);
            context.Cashiers.Add(cashier);
            context.CashDesks.Add(cashDesk);
            context.SaveChanges();
        }

        [Fact]
        public void CreatePayment_ShouldCreateNewPayment()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            var command = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 1,
                PaymentType = "Cash"
            };

            // Act
            var result = service.CreatePayment(command);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.CashDesk.Number);
            Assert.Equal(1, result.Employee.RegistrationNumber);
            Assert.Equal(PaymentType.Cash, result.PaymentType);
            Assert.Null(result.Confirmed);
        }

        [Fact]
        public void CreatePayment_WithCreditCard_AndNonManager_ShouldThrowException()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            var command = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 2, // Cashier
                PaymentType = "CreditCard"
            };

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(command));
            Assert.Equal("Insufficient rights to create a credit card payment.", exception.Message);
        }

        [Fact]
        public void ConfirmPayment_ShouldSetConfirmedDate()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            
            // Create a payment first
            var command = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 1,
                PaymentType = "Cash"
            };
            var payment = service.CreatePayment(command);

            // Act
            service.ConfirmPayment(payment.Id);

            // Assert
            var confirmedPayment = context.Payments.Find(payment.Id);
            Assert.NotNull(confirmedPayment);
            Assert.NotNull(confirmedPayment.Confirmed);
        }

        [Fact]
        public void ConfirmPayment_AlreadyConfirmed_ShouldThrowException()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            
            // Create a payment first
            var command = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 1,
                PaymentType = "Cash"
            };
            var payment = service.CreatePayment(command);
            
            // Confirm it
            service.ConfirmPayment(payment.Id);

            // Act & Assert
            var exception = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(payment.Id));
            Assert.Equal("Payment already confirmed.", exception.Message);
        }

        [Fact]
        public void AddPaymentItem_ShouldAddItemToPayment()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            
            // Create a payment first
            var paymentCommand = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 1,
                PaymentType = "Cash"
            };
            var payment = service.CreatePayment(paymentCommand);

            var itemCommand = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };

            // Act
            service.AddPaymentItem(itemCommand);

            // Assert
            var updatedPayment = context.Payments
                .Include(p => p.PaymentItems)
                .FirstOrDefault(p => p.Id == payment.Id);
                
            Assert.NotNull(updatedPayment);
            Assert.Single(updatedPayment.PaymentItems);
            Assert.Equal("Test Article", updatedPayment.PaymentItems[0].ArticleName);
            Assert.Equal(2, updatedPayment.PaymentItems[0].Amount);
            Assert.Equal(10.99m, updatedPayment.PaymentItems[0].Price);
        }

        [Fact]
        public void DeletePayment_WithItems_AndDeleteItemsTrue_ShouldDeletePaymentAndItems()
        {
            // Arrange
            var context = GetDbContext();
            var service = new PaymentService(context);
            
            // Create a payment first
            var paymentCommand = new NewPaymentCommand
            {
                CashDeskNumber = 1,
                EmployeeRegistrationNumber = 1,
                PaymentType = "Cash"
            };
            var payment = service.CreatePayment(paymentCommand);

            // Add an item
            var itemCommand = new NewPaymentItemCommand
            {
                ArticleName = "Test Article",
                Amount = 2,
                Price = 10.99m,
                PaymentId = payment.Id
            };
            service.AddPaymentItem(itemCommand);

            // Act
            service.DeletePayment(payment.Id, true);

            // Assert
            var deletedPayment = context.Payments.Find(payment.Id);
            Assert.Null(deletedPayment);
            
            var items = context.PaymentItems.Where(i => i.Payment.Id == payment.Id).ToList();
            Assert.Empty(items);
        }
    }
} 