namespace ScanVul.Server.Application.Features.Auth.InitAdmin;

/// <summary>
/// Admin credentials
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Password">Password</param>
public record InitAdminResponse(string Name, string Password);