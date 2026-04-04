using System.IO.Compression;
using Dapper;
using EFCore.BulkExtensions;
using Hangfire;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanVul.Server.Domain.PackageManagers.Entities;
using ScanVul.Server.Infrastructure.Data;
using ScanVul.Server.Infrastructure.Winget.Services;

namespace ScanVul.Server.Infrastructure.Hangfire.Workers;

public class WingetPackagesSyncWorker(
    IServiceScopeFactory serviceScopeFactory, 
    ILogger<WingetPackagesSyncWorker> logger,
    IHttpClientFactory httpClientFactory) : IWorker
{
    private const string SourceUrl = "https://cdn.winget.microsoft.com/cache/source.msix";
    
    [JobDisplayName("Download Winget packages index (SQLite) and save them to Postgres")]
    public async Task RunAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Starting Winget packages sync...");
        
        var tempFile = Path.GetTempFileName();
        var tempDbPath = Path.Combine(Path.GetTempPath(), "winget_index.db");

        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            
            logger.LogInformation("Downloading source.msix...");
            await using (var stream = await httpClient.GetStreamAsync(SourceUrl, ct))
            await using (var fileStream = File.Create(tempFile))
            {
                await stream.CopyToAsync(fileStream, ct);
            }

            logger.LogInformation("Extracting index.db...");
            using (var archive = ZipFile.OpenRead(tempFile))
            {
                var dbEntry = archive.GetEntry("Public/index.db");
                if (dbEntry == null)
                {
                    logger.LogError("index.db not found inside source.msix");
                    return;
                }
                
                if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
                
                dbEntry.ExtractToFile(tempDbPath);
            }

            await SyncDataFromSqliteToPostgres(tempDbPath, ct);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
            if (File.Exists(tempDbPath)) File.Delete(tempDbPath);
        }
    }

    private async Task SyncDataFromSqliteToPostgres(string sqlitePath, CancellationToken ct)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var postgresContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var connectionString = $"Data Source={sqlitePath};Mode=ReadOnly";

        await using var sqliteConnection = new SqliteConnection(connectionString);
        await sqliteConnection.OpenAsync(ct);

        var packages = await sqliteConnection.QueryAsync<WingetPackage>(
            """
            select 
                m.id as IdRowId, 
                i.id as Id,
                m.name as NameRowId, 
                n.name as Name
            from manifest m 
            join names n on m.name == n.rowid 
            join ids i on m.id == i.rowid
            group by i.id
            """);

        var pkgs = packages.ToList();
        await postgresContext.BulkInsertOrUpdateAsync(pkgs, cancellationToken: ct);
        await postgresContext.SaveChangesAsync(ct);
        
        foreach (var package in pkgs)
        {
            var versions = await sqliteConnection.QueryAsync<(long VersionRowId, string Version)>(
                """
                select 
                    m.version as VersionRowId,
                    v.version as Version
                from manifest m
                join versions v on m.version == v.rowid
                where m.id == @idRowId
                """, new { idRowId = package.IdRowId });

            var latestVersion = versions
                .MaxBy(x => x.Version, WingetVersionComparer.Instance);

            await postgresContext.WingetPackages
                .Where(x => x.IdRowId == package.IdRowId)
                .ExecuteUpdateAsync(x => x
                    .SetProperty(p => p.LastVersionRowId, latestVersion.VersionRowId)
                    .SetProperty(p => p.LastVersion, latestVersion.Version), cancellationToken: ct);
        }
    }
}