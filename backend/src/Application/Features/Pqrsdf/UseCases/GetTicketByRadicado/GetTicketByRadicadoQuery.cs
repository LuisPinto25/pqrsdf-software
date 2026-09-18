using MediatR;
using Pqrsdf.Domain.Shared.Results;

namespace Pqrsdf.Application.Features.Pqrsdf.UseCases.GetTicketByRadicado;

/// <summary>
/// Query to retrieve public tracking information for a ticket by its radicado number.
/// </summary>
public sealed record GetTicketByRadicadoQuery(string Radicado) : IRequest<Result<PublicTicketStatusDto>>;
