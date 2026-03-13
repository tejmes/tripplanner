using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripPlanner.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using TripPlanner.Domain.Entities;

namespace TripPlanner.Infrastructure.Database
{
    public class ApplicationDbContext : IdentityUserContext<User, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Trip> Trips { get; set; }
        public DbSet<TripUsers> TripUsers { get; set; }
        public DbSet<TripDay> TripDays { get; set; }
        public DbSet<Place> Places { get; set; }
        public DbSet<Accomodation> Accomodations { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Location> Locations { get; set; }

        public DbSet<Checklist> Checklists { get; set; }
        public DbSet<ChecklistItem> ChecklistItems { get; set; }

        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseShare> ExpenseShares { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Trip → User
            builder.Entity<Trip>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .IsRequired();

            // TripUsers → User
            builder.Entity<TripUsers>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(tu => tu.UserId)
                .IsRequired();

            // TripUsers → Trip (Cascade)
            builder.Entity<TripUsers>()
                .HasOne(tu => tu.Trip)
                .WithMany(t => t.TripUsers)
                .HasForeignKey(tu => tu.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique index on (TripId, UserId)
            builder.Entity<TripUsers>()
                .HasIndex(tu => new { tu.TripId, tu.UserId })
                .IsUnique();

            // TripDay → Trip (Cascade)
            builder.Entity<TripDay>()
                .HasOne(td => td.Trip)
                .WithMany()
                .HasForeignKey(td => td.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Place → Trip (Cascade)
            builder.Entity<Place>()
                .HasOne(p => p.Trip)
                .WithMany()
                .HasForeignKey(p => p.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Place → TripDay (Restrict)
            builder.Entity<Place>()
                .HasOne(p => p.TripDay)
                .WithMany()
                .HasForeignKey(p => p.TripDayId)
                .OnDelete(DeleteBehavior.Restrict);

            // Place → Location (Restrict)
            builder.Entity<Place>()
                .HasOne(p => p.Location)
                .WithMany()
                .HasForeignKey(p => p.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Destination → Location (Restrict)
            builder.Entity<Destination>()
                .HasOne(d => d.Location)
                .WithMany()
                .HasForeignKey(d => d.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Trip → Destination (no cascade)
            builder.Entity<Trip>()
                .HasOne(t => t.Destination)
                .WithMany()
                .HasForeignKey(t => t.DestinationId)
                .IsRequired();

            // Accomodation → Trip (1:1 Cascade)
            builder.Entity<Accomodation>()
                .HasOne(a => a.Trip)
                .WithOne(t => t.Accomodation)
                .HasForeignKey<Accomodation>(a => a.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accomodation → Location (Restrict)
            builder.Entity<Accomodation>()
                .HasOne(a => a.Location)
                .WithMany()
                .HasForeignKey(a => a.LocationId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Trip 1:N Checklist
            builder.Entity<Checklist>()
                .HasOne(c => c.Trip)
                .WithMany(t => t.Checklists)
                .HasForeignKey(c => c.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Checklist → Items
            builder.Entity<ChecklistItem>()
                .HasOne(i => i.Checklist)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.ChecklistId)
                .OnDelete(DeleteBehavior.Cascade);

            // Expense → Trip
            builder.Entity<Expense>()
                .HasOne(e => e.Trip)
                .WithMany(t => t.Expenses)
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // Expense → PaidByUserId
            builder.Entity<Expense>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.PaidByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExpenseShare → Expense
            builder.Entity<ExpenseShare>()
                .HasOne(es => es.Expense)
                .WithMany(e => e.Shares)
                .HasForeignKey(es => es.ExpenseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExpenseShare → User
            builder.Entity<ExpenseShare>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
