using Microsoft.EntityFrameworkCore;
using HackerFlow.Models;

namespace HackerFlow.Data;

public class HackerFlowContext : DbContext
{
    public HackerFlowContext(DbContextOptions<HackerFlowContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Deleting someone who introduced others: sets their IntroducedById to null
        modelBuilder.Entity<Connection>()
            .HasOne(c => c.IntroducedBy)
            .WithMany(c => c.IntroducedTo)
            .HasForeignKey(c => c.IntroducedById)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Connection>()
            .HasMany(c => c.Companies)
            .WithMany(c => c.Connections)
            .UsingEntity<Dictionary<string, object>>(
                "ConnectionCompanies",
                // Company side: block deleting a company that has connections
                j => j.HasOne<Company>().WithMany()
                    .HasForeignKey("CompaniesId")
                    .OnDelete(DeleteBehavior.Restrict),
                // Connection side: deleting a connection removes its links
                j => j.HasOne<Connection>().WithMany()
                    .HasForeignKey("ConnectionsId")
                    .OnDelete(DeleteBehavior.Cascade));

        // Block deleting a company that still has applications (default is cascade)
        modelBuilder.Entity<JobApplication>()
            .HasOne(a => a.Company)
            .WithMany(c => c.Applications)   
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Deleting a connection keeps the application and clears its ConnectionId
        modelBuilder.Entity<JobApplication>()
            .HasOne(a => a.Connection)
            .WithMany()
            .HasForeignKey(a => a.ConnectionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Unique company names, since the resolve methods match by name
        modelBuilder.Entity<Company>()
            .HasIndex(c => c.Name)
            .IsUnique();
    }

    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<CoverLetter> CoverLetters => Set<CoverLetter>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<Connection> Connections => Set<Connection>();
    public DbSet<Company> Companies => Set<Company>();
}