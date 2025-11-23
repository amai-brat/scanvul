using Microsoft.EntityFrameworkCore;
using ScanVul.Server.Domain.Entities;

namespace ScanVul.Server.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<PackageInfo> PackageInfos => Set<PackageInfo>();
    public DbSet<Computer> Computers => Set<Computer>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}