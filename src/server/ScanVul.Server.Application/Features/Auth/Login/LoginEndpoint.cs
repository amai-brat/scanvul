using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.UserAggregate.Repositories;
using ScanVul.Server.Domain.UserAggregate.Services;

namespace ScanVul.Server.Application.Features.Auth.Login;

public class LoginEndpoint(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtGenerator jwtGenerator)
    : Endpoint<LoginRequest, Results<Ok<LoginResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/auth/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Login";
            s.Description = "Login";
            s.ExampleRequest = new LoginRequest("name", "password");
        });
        Description(x => x.WithTags("Auth"));
    }

    public override async Task<Results<Ok<LoginResponse>, ProblemDetails>> ExecuteAsync(
        LoginRequest req,
        CancellationToken ct)
    {
        var user = await userRepository.GetByNameAsync(req.Name, ct);
        if (user == null || !passwordHasher.Verify(req.Password, user.Password))
        {
            AddError("Incorrect name or password");
            return new ProblemDetails(ValidationFailures, statusCode: (int)HttpStatusCode.Unauthorized);

        }

        var token = jwtGenerator.GenerateToken(user);
        return TypedResults.Ok(new LoginResponse(token));
    }
}