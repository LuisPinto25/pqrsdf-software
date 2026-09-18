using Pqrsdf.Api.Shared.Middleware;
using Pqrsdf.Application;
using Pqrsdf.Infrastructure;
using Pqrsdf.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure centralized RFC 7807 ProblemDetails and Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Configure Health Checks with SQL Server DbContext check
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PqrsdfDbContext>("sqlserver");

var app = builder.Build();

// Global Exception Handler must be the first middleware in the pipeline
app.UseExceptionHandler();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

// Ensure database exists on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<PqrsdfDbContext>();
        await context.Database.EnsureCreatedAsync();
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
