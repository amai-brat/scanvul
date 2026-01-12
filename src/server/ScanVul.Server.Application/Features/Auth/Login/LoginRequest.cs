namespace ScanVul.Server.Application.Features.Auth.Login;

/// <summary>
/// Login credentials
/// </summary>
/// <param name="Name">Name</param>
/// <param name="Password">Password</param>
public record LoginRequest(string Name, string Password);