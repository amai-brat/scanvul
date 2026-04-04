using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.AgentAggregate.Entities;
using ScanVul.Server.Domain.PackageManagers.Entities;
using ScanVul.Server.Domain.Reports.Entities;
using ScanVul.Server.Domain.UserAggregate.Entities;

namespace ScanVul.Server.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<PackageInfo> PackageInfos => Set<PackageInfo>();
    public DbSet<VulnerablePackage> VulnerablePackages => Set<VulnerablePackage>();
    public DbSet<BduVulnerablePackage> BduVulnerablePackages => Set<BduVulnerablePackage>();
    public DbSet<Computer> Computers => Set<Computer>();
    public DbSet<VulnerabilityScanReport> VulnerabilityScanReports => Set<VulnerabilityScanReport>();

    public DbSet<User> Users => Set<User>();
    
    public DbSet<WingetPackage> WingetPackages => Set<WingetPackage>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}