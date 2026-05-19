using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenSearch.Client;
using OpenSearch.Net;
using ScanVul.Server.Domain.Cve.Repositories;
using ScanVul.Server.Infrastructure.OpenSearch.Helpers;
using ScanVul.Server.Infrastructure.OpenSearch.Repositories;
using ScanVul.Server.Infrastructure.OpenSearch.Services;

namespace ScanVul.Server.Infrastructure.OpenSearch;

public static class Entry
{
    public static IServiceCollection AddOpenSearch(
        this IServiceCollection services, 
        IWebHostEnvironment environment,
        OpenSearchOptions? options)
    {
        if (options == null || string.IsNullOrEmpty(options.Url) || 
            string.IsNullOrEmpty(options.Username) || 
            string.IsNullOrEmpty(options.Password)) 
            throw new InvalidOperationException("OpenSearch settings not set");

        var pool = new SingleNodeConnectionPool(new Uri(options.Url));
        var settings = new ConnectionSettings(pool, sourceSerializer: SystemTextJsonSerializer.Default)
            .BasicAuthentication(options.Username, options.Password);

        if (environment.IsDevelopment())
        {
            settings = settings.ServerCertificateValidationCallback((_, _, _, _) => true);
            settings = settings.DisableDirectStreaming();
            settings = settings.RequestTimeout(TimeSpan.FromMinutes(30));
        }
        
        services.AddSingleton<IOpenSearchClient>(_ => new OpenSearchClient(settings));
        services.AddScoped<IOpenSearchFiller, OpenSearchFiller>();
        services.AddScoped<ICveRepository, CveRepositoryV2>();
        services.AddScoped<IBduRepository, BduRepository>();
        
        return services;
    }
}