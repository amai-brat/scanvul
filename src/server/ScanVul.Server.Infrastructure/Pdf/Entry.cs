using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using ScanVul.Server.Domain.Reports.Services;
using ScanVul.Server.Infrastructure.Pdf.Services;

namespace ScanVul.Server.Infrastructure.Pdf;

public static class Entry
{
    public static IServiceCollection AddPdf(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var pdfOptions = configuration
            .GetSection("Pdf")
            .Get<PdfOptions>();
        
        PdfOptions.Validate(pdfOptions);
        
        services.Configure<PdfOptions>(configuration.GetSection("Pdf"));
        
        QuestPDF.Settings.License = LicenseType.Community;

        services.AddTransient<IVulnerabilityScanReportGenerator, QuestPdfVulnerabilityScanReportGenerator>();
        return services;
    }
}