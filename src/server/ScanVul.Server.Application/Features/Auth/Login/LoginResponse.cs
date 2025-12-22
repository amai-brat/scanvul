namespace ScanVul.Server.Application.Features.Auth.Login;

/// <summary>
/// Response with token
/// </summary>
/// <param name="Token">JWT</param>
public record LoginResponse(string Token);