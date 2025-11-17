using Microsoft.EntityFrameworkCore;

namespace ScanVul.Server.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    
}