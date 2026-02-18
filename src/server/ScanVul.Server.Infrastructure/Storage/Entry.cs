using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Infrastructure.Storage.Services;

namespace ScanVul.Server.Infrastructure.Storage;

public static class Entry
{
    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var storageOptions = configuration
            .GetSection("Storage")
            .Get<StorageOptions>();
        
        StorageOptions.Validate(storageOptions);
        
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        services.AddScoped<IFileStorage, SystemFileStorage>();
        
        return services;
    }
}