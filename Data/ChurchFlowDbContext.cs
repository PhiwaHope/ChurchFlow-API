using ChurchFlow.api.Models;
using Microsoft.EntityFrameworkCore;

public class ChurchFlowDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contribution>()
            .HasOne(c => c.Event)
            .WithMany(e => e.Contributions)
            .HasForeignKey(c => c.EventId)
            .OnDelete(DeleteBehavior.Restrict); //Restrict deleting Contribution

        modelBuilder.Entity<Contribution>()
            .HasOne(c => c.Member)
            .WithMany(m => m.Contributions)
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Committee>()
            .HasOne(c => c.Member)
            .WithMany()
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Store Transaction's enums as readable strings in the database
        // instead of raw integers, so they're legible in pgAdmin.
        modelBuilder.Entity<Transaction>()
            .Property(t => t.Type)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.PaymentMethod)
            .HasConversion<string>();

        modelBuilder.Entity<Transaction>()
            .Property(t => t.RefType)
            .HasConversion<string>();
    }

    public ChurchFlowDbContext(DbContextOptions<ChurchFlowDbContext> options) : base(options)
    {

    }
    public DbSet<Event> Events { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Contribution> Contributions { get; set; }
    public DbSet<Committee> Committees { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
}