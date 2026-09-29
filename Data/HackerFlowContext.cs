using Microsoft.EntityFrameworkCore;
using HackerFlow.Models;

namespace HackerFlow.Data;

public class HackerFlowContext : DbContext
{
    public HackerFlowContext(DbContextOptions<HackerFlowContext> options)
        : base(options) { }

    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();
    public DbSet<Resume> Resumes => Set<Resume>();
}