using ScanVul.Server.Domain.Cve.Services;

namespace ScanVul.Server.Tests.PackageSearching;

public class SearchTermSanitizerTests
{
    private readonly SearchTermSanitizer _sut = new();

    [Theory(DisplayName = "Package name should be sanitized")]
    [InlineData("   ", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("7-zip 26.00 (x64)", "7-zip")]
    [InlineData("microsoft .net apphost pack - 9.0.10 (x64)", "microsoft .net apphost pack")]
    [InlineData("microsoft visual c++ 2005 redistributable (x64)", "microsoft visual c++ 2005 redistributable")]
    [InlineData("mozilla firefox (x64 en-us)", "mozilla firefox")]
    [InlineData("python 3.13.7 test suite (64-bit)", "python 3.13.7 test suite")]
    [InlineData("notepad++ (32-bit x86)", "notepad++")]
    [InlineData("microsoft.net.workload.mono.toolchain.net8.manifest (x64)", "microsoft.net.workload.mono.toolchain.net8.manifest")]
    [InlineData("microsoft.net.sdk.macos.manifest-9.0.100 (x64)", "microsoft.net.sdk.macos.manifest-9.0.100")]
    [InlineData("dotnet-runtime-8.0", "dotnet-runtime-8.0")]
    public void SanitizePackageName_NameGiven_Sanitized(string? packageName, string expected)
    {
        // act
        var actual = _sut.SanitizePackageName(packageName);
        
        // assert
        Assert.Equal(expected, actual);
    }
}