var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/api/v1/agents/register", () => Results.Ok(new { token = Guid.CreateVersion7() }));

app.Run();