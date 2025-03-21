using Bogus;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Linq;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class AppointmentContext : DbContext
    {
        public DbSet<CashDesk> CashDesks => Set<CashDesk>();
        public DbSet<Cashier> Cashiers => Set<Cashier>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Manager> Managers => Set<Manager>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentItem> PaymentItems => Set<PaymentItem>();

        public AppointmentContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Add your configuration here
            modelBuilder.Entity<Employee>().OwnsOne(e => e.Address);
            modelBuilder.Entity<Employee>().HasDiscriminator(e => e.Type);
            modelBuilder.Entity<Payment>().Property(p => p.PaymentType)
                .HasConversion<string>();
        }

        public void Seed()
        {
            Randomizer.Seed = new System.Random(1925);
            int registrationNumber = 1000;
            var cashiers = new Faker<Cashier>("de")
                .CustomInstantiator(f =>
                {
                    return new Cashier(
                        registrationNumber++,
                        f.Name.FirstName(), f.Name.LastName(),
                        new Address(
                            f.Address.StreetName(), f.Random.Int(1000, 9999).ToString(), f.Address.City()),
                        f.Lorem.Sentence(2))
                    { LastLogin = f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1))
                        .OrNull(f, 0.5f) 
                    };
                })
                .Generate(10)
                .ToList();
            Cashiers.AddRange(cashiers);
            SaveChanges();

            var managers = new Faker<Manager>("de")
                .CustomInstantiator(f =>
                {
                    return new Manager(
                        registrationNumber++,
                        f.Name.FirstName(), f.Name.LastName(),
                        new Address(
                            f.Address.StreetName(), f.Random.Int(1000, 9999).ToString(), f.Address.City()),
                        f.Commerce.ProductAdjective())
                    {
                        LastLogin = f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1))
                        .OrNull(f, 0.5f)
                    };
                })
                .Generate(3)
                .ToList();
            Managers.AddRange(managers);
            SaveChanges();

            var cashDesks = Enumerable.Range(1,5)
                .Select(i=>new CashDesk(i)).ToList();
            CashDesks.AddRange(cashDesks);
            SaveChanges();

            var payments = new Faker<Payment>("de")
                .CustomInstantiator(f =>
                {
                    var cashDesk = f.Random.ListItem(cashDesks);
                    var employee = f.Random.ListItem(cashiers);
                    var paymentDateTime = f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 1, 1));
                    var paymentType = f.Random.Enum<PaymentType>();
                    var payment = new Payment(cashDesk, paymentDateTime, employee, paymentType);
                    var paymentItems = new Faker<PaymentItem>("de")
                        .CustomInstantiator(f =>
                        {
                            return new PaymentItem(
                                f.Commerce.Product(), f.Random.Int(1, 5),
                                Math.Round(f.Random.Decimal(10, 100), 2),
                                payment);
                        })
                        .Generate(f.Random.Int(1, 10))
                        .ToList();
                    payment.PaymentItems.AddRange(paymentItems);
                    return payment;
                })
                .Generate(20)
                .ToList();
            Payments.AddRange(payments);
            SaveChanges();
        }
    }
}