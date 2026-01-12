using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using ScanVul.Server.Application.Options;

namespace ScanVul.Server.API;

public static class Entry
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
        JwtOptions.Validate(jwtOptions);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions!.Key)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = mrCtx =>
                    {
                        var path = mrCtx.Request.Path.HasValue ? mrCtx.Request.Path.Value : "";
                        var pathBase = mrCtx.Request.PathBase.HasValue ? mrCtx.Request.PathBase.Value : path;
                        var isFromHangFire = path.StartsWith("/hangfire") || pathBase.StartsWith("/hangfire");
                        
                        if (isFromHangFire)
                        {
                            if (mrCtx.Request.Query.ContainsKey("jwt"))
                            {
                                //If we find token add it to the response cookies
                                mrCtx.Token = mrCtx.Request.Query["jwt"];
                                mrCtx.HttpContext.Response.Cookies
                                    .Append("HangFireCookie",
                                        mrCtx.Token!,
                                        new CookieOptions
                                        {
                                            Expires = DateTime.Now.AddMinutes(10),
                                            SameSite = SameSiteMode.None,
                                            HttpOnly = true,
                                            Secure = true,
                                        });
                            }
                            else
                            {
                                //Check if we have a cookie from the previous request.
                                var cookies = mrCtx.Request.Cookies;
                                if (cookies.ContainsKey("HangFireCookie"))
                                    mrCtx.Token = cookies["HangFireCookie"];                
                            }
                        }
                        return Task.CompletedTask;
                    },
                };
            });
        
        return services;
    }
}