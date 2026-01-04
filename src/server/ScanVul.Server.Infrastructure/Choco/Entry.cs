using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Application.Helpers;
using ScanVul.Server.Domain.PackageManagers.Services;
using ScanVul.Server.Infrastructure.Choco.Services;

namespace ScanVul.Server.Infrastructure.Choco;

public static class Entry
{
    public static IServiceCollection AddChocoPackageManager(this IServiceCollection services)
    {
        services.AddKeyedScoped<IPackageManager, ChocoPackageManager>(Consts.PackageManagers.Choco);
        
        return services;
    }   
}