using System.Net;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using ScanVul.Server.Domain.Common;
using ScanVul.Server.Domain.UserAggregate.Entities;
using ScanVul.Server.Domain.UserAggregate.Repositories;
using ScanVul.Server.Domain.UserAggregate.Services;

namespace ScanVul.Server.Application.Features.Auth.InitAdmin;

public class InitAdminEndpoint(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
    : EndpointWithoutRequest<Results<Ok<InitAdminResponse>, ProblemDetails>>
{
    public override void Configure()
    {
        Version(1);
        Post("api/{apiVersion}/auth/init");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Init admin user";
            s.Description = "Init admin user. Can be called only once";
        });
        Description(x => x
            .WithTags("Auth")
            .Produces<ProblemDetails>(403, "application/json+problem"));
    }

    public override async Task<Results<Ok<InitAdminResponse>, ProblemDetails>> ExecuteAsync(
        CancellationToken ct)
    {
        var exists = await userRepository.IsActiveAdminExistsAsync(ct);
        if (exists)
        {
            AddError("Active admin already exists. Unable to create new one");
            return new ProblemDetails(ValidationFailures, statusCode: (int)HttpStatusCode.Forbidden);
        }

        var name = Guid.NewGuid().ToString("N");
        var password = Guid.NewGuid().ToString("N");
        
        var admin = new User(name, passwordHasher.Hash(password));
        await userRepository.AddAsync(admin, ct);
        
        await unitOfWork.SaveChangesAsync(ct);
        
        return TypedResults.Ok(new InitAdminResponse(name, password));
    }
}