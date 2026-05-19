using System.Text.Json;
using FastEndpoints;

namespace ScanVul.Server.Application.Helpers;

public static class EndpointExtensions
{
    public static Task SendCustom<TResult>(this IEndpoint ep, 
        TResult result, 
        int statusCode = 200, 
        JsonSerializerOptions? serializerOptions = null, 
        CancellationToken ct = default)
    {
        ep.HttpContext.MarkResponseStart();
        ep.HttpContext.Response.StatusCode = statusCode;
        ep.HttpContext.Response.ContentType = "application/json";
        return JsonSerializer.SerializeAsync(ep.HttpContext.Response.Body, result, options: serializerOptions, cancellationToken: ct);
    }
}