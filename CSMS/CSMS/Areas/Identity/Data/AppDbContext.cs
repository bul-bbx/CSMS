using CSMS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CSMS.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<ServiceType> ServiceTypes { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<RepairOrder> RepairOrders { get; set; }
    public DbSet<RepairHistory> RepairHistories { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<RepairPart> RepairParts { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<EmailNotification> EmailNotifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RepairPart composite key
        modelBuilder.Entity<RepairPart>()
            .HasKey(rp => new { rp.RepairOrderId, rp.PartId });

        // Unique constraints
        modelBuilder.Entity<Car>()
            .HasIndex(c => c.Vin)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.InvoiceNumber)
            .IsUnique();

        // Relationships

        // Car -> Appointments (Restrict to avoid cascade conflicts)
        modelBuilder.Entity<Car>()
            .HasMany(c => c.Appointments)
            .WithOne(a => a.Car)
            .HasForeignKey(a => a.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment -> Mechanic
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Mechanic)
            .WithMany()
            .HasForeignKey(a => a.MechanicId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment -> ServiceType
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.ServiceType)
            .WithMany(st => st.Appointments)
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // RepairOrder -> Appointment (Restrict to avoid multiple cascade paths)
        modelBuilder.Entity<RepairOrder>()
            .HasOne(ro => ro.Appointment)
            .WithOne(a => a.RepairOrder)
            .HasForeignKey<RepairOrder>(ro => ro.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // RepairHistory -> RepairOrder
        modelBuilder.Entity<RepairHistory>()
            .HasOne(rh => rh.RepairOrder)
            .WithOne(ro => ro.RepairHistory)
            .HasForeignKey<RepairHistory>(rh => rh.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // RepairPart
        modelBuilder.Entity<RepairPart>()
            .HasOne(rp => rp.RepairOrder)
            .WithMany(ro => ro.RepairParts)
            .HasForeignKey(rp => rp.RepairOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RepairPart>()
            .HasOne(rp => rp.Part)
            .WithMany(p => p.RepairParts)
            .HasForeignKey(rp => rp.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice -> RepairOrder (Restrict)
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.RepairOrder)
            .WithOne(ro => ro.Invoice)
            .HasForeignKey<Invoice>(i => i.RepairOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Invoice -> Customer (Restrict)
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Customer)
            .WithMany()
            .HasForeignKey(i => i.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // InvoiceItem -> Invoice
        modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // EmailNotification -> User
        modelBuilder.Entity<EmailNotification>()
            .HasOne(en => en.User)
            .WithMany()
            .HasForeignKey(en => en.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditLog -> User
        modelBuilder.Entity<AuditLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
