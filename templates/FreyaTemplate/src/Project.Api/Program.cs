using Project.Api.Extensions;
using Project.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddTransient<CorrelationIdMiddleware>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseHsts();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseFreyaEnterpriseSecurityHeaders(app.Environment, _ =>
{
    // If you serve Swagger UI, you may want to relax CSP ONLY for swagger endpoints (see below).
    // Otherwise, keep strict defaults.
});

// If Swagger is enabled, and you want Swagger UI to work, relax CSP for /swagger only.
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments("/swagger"),
    branch => branch.Use(async (context, next) =>
    {
        // Swagger UI needs scripts/styles. Keep it scoped to /swagger only.
        context.Response.Headers.ContentSecurityPolicy =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; base-uri 'none';";
        await next().ConfigureAwait(false);
    })
);

app.MapHealthChecks("/health");
app.MapGet("/ping", (TimeProvider time) => Results.Ok(new { ok = true, utc = time.GetUtcNow() }));

app.Run();