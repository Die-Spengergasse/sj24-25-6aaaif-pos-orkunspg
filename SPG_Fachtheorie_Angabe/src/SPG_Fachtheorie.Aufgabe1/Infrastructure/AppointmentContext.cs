using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Model;

namespace SPG_Fachtheorie.Aufgabe1.Infrastructure
{
    public class AppointmentContext : DbContext
    {
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<CashDesk> CashDesks => Set<CashDesk>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<PaymentItem> PaymentItems => Set<PaymentItem>();


        // TODO: Add your DbSets here
        public AppointmentContext(DbContextOptions options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Add your configuration here
            modelBuilder.Entity<Employee>().HasKey(e => e.RegistrationNumber);
            modelBuilder.Entity<Employee>().HasDiscriminator<string>("Type")
            .HasValue<Manager>("Manager")
            .HasValue<Cashier>("Cashier");
            base.OnModelCreating(modelBuilder);
        }
    }
}