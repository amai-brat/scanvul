using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using ScanVul.Server.Domain.Reports.Services;
using ScanVul.Server.Infrastructure.Pdf.Services;

namespace ScanVul.Server.Infrastructure.Pdf;

public static class Entry
{
    public static IServiceCollection AddPdf(this IServiceCollection services)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddTransient<IVulnerabilityScanReportGenerator, QuestPdfVulnerabilityScanReportGenerator>();
        return services;
    }
}