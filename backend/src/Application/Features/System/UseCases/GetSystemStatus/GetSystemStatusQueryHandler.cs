using MediatR;
using Pqrsdf.Application.Shared.Persistence;
using Pqrsdf.Domain.Constants;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;

/// <summary>
/// Query handler that verifies system readiness and builds the SystemStatusDto result.
/// Co-located strictly under Features/System/UseCases/GetSystemStatus/ per Constitution Principle II.
/// </summary>
public sealed class GetSystemStatusQueryHandler : IRequestHandler<GetSystemStatusQuery, Result<SystemStatusDto>>
{
    private readonly IDatabaseStatusChecker _databaseStatusChecker;

    public GetSystemStatusQueryHandler(IDatabaseStatusChecker databaseStatusChecker)
    {
        _databaseStatusChecker = databaseStatusChecker;
    }

    public async Task<Result<SystemStatusDto>> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        bool isDbConnected = await _databaseStatusChecker.CanConnectAsync(cancellationToken);

        string status = isDbConnected ? "Healthy" : "Degraded";
        string message = isDbConnected ? DomainMessages.System.Operational : DomainMessages.System.DatabaseUnavailable;

        var dto = new SystemStatusDto(
            Status: status,
            Service: "PQRSDF Management API",
            Version: "1.0.0",
            Environment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
            Timestamp: DateTime.UtcNow,
            DatabaseConnected: isDbConnected,
            Message: message
        );

        return Result<SystemStatusDto>.Success(dto);
    }
}
