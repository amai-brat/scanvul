namespace ScanVul.Server.Domain.Cve.Services;

public interface ISearchTermSanitizer
{
    /// <summary>
    /// Sanitize package name to normalize it (to make it searchable in CVE affected product name)
    /// </summary>
    /// <param name="name">Package name</param>
    /// <returns>Sanitized package name</returns>
    string SanitizePackageName(string? name);
}