namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;

public sealed record MakePqrsdfResponse(
    Guid Id,
    string RadicadoNumber,
    string Type,
    DateTimeOffset CreatedAtUtc,
    DateOnly DueDate,
    int BusinessDaysCount,
    string Status);
