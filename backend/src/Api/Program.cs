using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pqrsdf.Api.Shared.Middleware;
using Pqrsdf.Application;
using Pqrsdf.Infrastructure;
using Pqrsdf.Infrastructure.Persistence;

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

// Configure JWT Bearer Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secret = jwtSettings["Secret"] ?? "PqrsdfSuperSecretKeyForJwtAuthentication2026!#";
var issuer = jwtSettings["Issuer"] ?? "Pqrsdf.Api";
var audience = jwtSettings["Audience"] ?? "Pqrsdf.Client";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in strict production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// Configure Role-Based Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireFuncionario", policy => policy.RequireRole("Funcionario", "Administrador"));
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrador"));
});

// Configure Swagger / OpenAPI with JWT Bearer support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PQRSDF Management & Public API",
        Version = "v1",
        Description = "API institucional para radicación, consulta y gestión de PQRSDF con autenticación JWT."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese el token JWT Bearer: Bearer {su_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, doc, null), new List<string>() }
    });
});

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

// Configure IP-based Rate Limiting (PublicTracking: 30 req/min, Login: 5 req/min)
const string PublicTrackingPolicy = "PublicTrackingPolicy";
const string LoginRateLimitPolicy = "LoginRateLimit";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var isLogin = context.HttpContext.Request.Path.StartsWithSegments("/api/v1/auth/login") ||
                      context.HttpContext.Request.Path.StartsWithSegments("/api/auth/login");
        var detail = isLogin
            ? "Demasiados intentos de inicio de sesión fallidos. Por favor intente nuevamente en un minuto."
            : "Ha superado el límite de consultas permitidas por minuto. Por favor espere un momento antes de intentar de nuevo.";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc6585#section-4",
            Title = isLogin ? "Demasiados intentos de acceso" : "Too Many Requests",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = detail
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

    options.AddPolicy(LoginRateLimitPolicy, httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIp,
            factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
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

app.UseAuthentication();
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
                IF COL_LENGTH('PqrsdfTickets', 'AssignedToUserId') IS NULL
                BEGIN
                    ALTER TABLE PqrsdfTickets ADD AssignedToUserId uniqueidentifier NULL;
                END
                IF COL_LENGTH('PqrsdfTickets', 'AssignedAtUtc') IS NULL
                BEGIN
                    ALTER TABLE PqrsdfTickets ADD AssignedAtUtc datetime2 NULL;
                END
                IF COL_LENGTH('PqrsdfTickets', 'AssignmentNote') IS NULL
                BEGIN
                    ALTER TABLE PqrsdfTickets ADD AssignmentNote nvarchar(500) NULL;
                END
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
            BEGIN
                CREATE TABLE [dbo].[Users] (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [Email] NVARCHAR(254) NOT NULL,
                    [PasswordHash] NVARCHAR(255) NOT NULL,
                    [FullName] NVARCHAR(150) NOT NULL,
                    [Role] INT NOT NULL,
                    [IsActive] BIT NOT NULL DEFAULT 1,
                    [CreatedAtUtc] DATETIME2 NOT NULL,
                    [UpdatedAtUtc] DATETIME2 NULL,
                    [LastLoginAtUtc] DATETIME2 NULL
                );
                CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users] ([Email] ASC);

                INSERT INTO [dbo].[Users] ([Id], [Email], [PasswordHash], [FullName], [Role], [IsActive], [CreatedAtUtc])
                VALUES 
                ('B81B94C8-E0C7-4187-8A33-89AC54395E01', 'admin@pqrsdf.gov.co', '$2a$11$MJdcdRnVBhcwT5M/6j6VWedZXXxbf2S4xRTp5/JZKUheied8ihHO2', 'Administrador del Sistema', 2, 1, '2026-01-01T00:00:00Z'),
                ('E49D34F1-9BD7-40A5-926B-9548BE740F02', 'funcionario@pqrsdf.gov.co', '$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy', 'Funcionario de PQRSDF', 1, 1, '2026-01-01T00:00:00Z'),
                ('F5A0E6B2-1C3D-428E-874C-9659CF851F03', 'funcionario2@pqrsdf.gov.co', '$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy', 'María Fernanda Gómez', 1, 1, '2026-01-01T00:00:00Z');
            END

            IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Users') AND NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Email] = 'funcionario2@pqrsdf.gov.co')
            BEGIN
                INSERT INTO [dbo].[Users] ([Id], [Email], [PasswordHash], [FullName], [Role], [IsActive], [CreatedAtUtc])
                VALUES ('F5A0E6B2-1C3D-428E-874C-9659CF851F03', 'funcionario2@pqrsdf.gov.co', '$2a$11$fzrWhddqPT09gX.1iZRKf.pOLf4epbqVmw1qUhwF2DBU07Tfi2Vgy', 'María Fernanda Gómez', 1, 1, '2026-01-01T00:00:00Z');
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TicketAssignmentHistories')
            BEGIN
                CREATE TABLE [dbo].[TicketAssignmentHistories] (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TicketId] UNIQUEIDENTIFIER NOT NULL,
                    [PreviousAssignedUserId] UNIQUEIDENTIFIER NULL,
                    [NewAssignedUserId] UNIQUEIDENTIFIER NOT NULL,
                    [AssignedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [AssignedAtUtc] DATETIME2 NOT NULL,
                    [Note] NVARCHAR(500) NULL,
                    [Type] INT NOT NULL,
                    [CreatedAtUtc] DATETIME2 NOT NULL,
                    [UpdatedAtUtc] DATETIME2 NULL
                );
                CREATE NONCLUSTERED INDEX [IX_TicketAssignmentHistories_TicketId] ON [dbo].[TicketAssignmentHistories] ([TicketId]);
                CREATE NONCLUSTERED INDEX [IX_TicketAssignmentHistories_NewAssignedUserId] ON [dbo].[TicketAssignmentHistories] ([NewAssignedUserId]);
            END

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TicketStatusHistories')
            BEGIN
                CREATE TABLE [dbo].[TicketStatusHistories] (
                    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
                    [TicketId] UNIQUEIDENTIFIER NOT NULL,
                    [PreviousStatus] INT NOT NULL,
                    [NewStatus] INT NOT NULL,
                    [ChangedByUserId] UNIQUEIDENTIFIER NOT NULL,
                    [Justification] NVARCHAR(500) NOT NULL,
                    [ChangedAtUtc] DATETIME2 NOT NULL,
                    [CreatedAtUtc] DATETIME2 NOT NULL,
                    [UpdatedAtUtc] DATETIME2 NULL
                );
                CREATE NONCLUSTERED INDEX [IX_TicketStatusHistories_TicketId_ChangedAtUtc] ON [dbo].[TicketStatusHistories] ([TicketId], [ChangedAtUtc] DESC);
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
