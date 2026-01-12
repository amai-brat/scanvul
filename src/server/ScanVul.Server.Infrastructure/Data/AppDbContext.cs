using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.UserAggregate.Entities;

namespace ScanVul.Server.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<PackageInfo> PackageInfos => Set<PackageInfo>();
    public DbSet<VulnerablePackage> VulnerablePackages => Set<VulnerablePackage>();
    public DbSet<Computer> Computers => Set<Computer>();

    public DbSet<User> Users => Set<User>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}