using Pqrsdf.Api.Shared.Middleware;
using Pqrsdf.Application;
using Pqrsdf.Infrastructure;
using Pqrsdf.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure centralized RFC 7807 ProblemDetails and Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure CORS for frontend access
const string CorsPolicy = "FrontendPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return uri.Host == "localhost" || uri.Host == "127.0.0.1";
                }
                return false;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Configure Health Checks with SQL Server DbContext check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PqrsdfDbContext>("sqlserver");

// Configure IP-based Rate Limiting (30 requests/minute)
const string PublicTrackingPolicy = "PublicTrackingPolicy";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = "Too Many Requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Ha superado el límite de consultas permitidas por minuto. Por favor espere un momento antes de intentar de nuevo."
        };
        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: token);
    };

    options.AddPolicy(PublicTrackingPolicy, httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp,
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

var app = builder.Build();

// Global Exception Handler must be the first middleware in the pipeline
app.UseExceptionHandler();

// Enable CORS for frontend requests
app.UseCors(CorsPolicy);

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PQRSDF API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Ensure database exists and schema is up to date on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PqrsdfDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Ensure schema evolution for existing database instances
        await context.Database.ExecuteSqlRawAsync(@"
            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PqrsdfTickets')
            BEGIN
                IF COL_LENGTH('PqrsdfTickets', 'ResponseDateUtc') IS NULL
                BEGIN
                    ALTER TABLE PqrsdfTickets ADD ResponseDateUtc datetime2 NULL;
                END
                IF COL_LENGTH('PqrsdfTickets', 'ResponseText') IS NULL
                BEGIN
                    ALTER TABLE PqrsdfTickets ADD ResponseText nvarchar(4000) NULL;
                END
            END
        ");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "No fue posible verificar o crear la base de datos en el inicio. Verifique la conexión a SQL Server.");
    }
}

app.Run();

// Required for integration testing
public partial class Program { }
