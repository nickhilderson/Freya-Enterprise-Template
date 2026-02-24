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

app.MapHealthChecks("/health");
app.MapGet("/ping", (TimeProvider time) => Results.Ok(new { ok = true, utc = time.GetUtcNow() }));

app.Run();
