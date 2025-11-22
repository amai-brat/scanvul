namespace ScanVul.Server.Application.Features.Agents.Register;

/// <summary>
/// Request to register agent
/// </summary>
/// <param name="OperatingSystemName">NAME from /etc/os-release for linux or e.g. "Windows 10" for windows</param>
/// <param name="OperatingSystemVersion">VERSION_ID from /etc/os-release for linux or value from "Version" column from <![CDATA[<a href="https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions">here</a>]]> for windows</param>
public record RegisterRequest(
    string OperatingSystemName, 
    string? OperatingSystemVersion);