namespace ScanVul.Server.Infrastructure.Pdf;

public class PdfOptions
{
    public string FrontendAgentPageUrl { get; set; } = null!;

    public static void Validate(PdfOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        
        ArgumentException.ThrowIfNullOrWhiteSpace(options.FrontendAgentPageUrl);
    }
}