namespace Pqrsdf.Domain.Enums;

/// <summary>
/// Official types of PQRSDF requests according to Colombian administrative regulations.
/// </summary>
public enum PqrsdfType
{
    Petition = 1,     // Petición (15 business days)
    Complaint = 2,    // Queja (15 business days)
    Claim = 3,        // Reclamo (15 business days)
    Suggestion = 4,   // Sugerencia (15 business days, anonymous allowed)
    Denunciation = 5, // Denuncia (30 business days, anonymous allowed)
    Compliment = 6    // Felicitación (15 business days)
}

/// <summary>
/// Identification document types recognized in Colombia.
/// </summary>
public enum IdentificationType
{
    CC = 1,   // Cédula de Ciudadanía
    CE = 2,   // Cédula de Extranjería
    TI = 3,   // Tarjeta de Identidad
    PA = 4,   // Pasaporte
    NIT = 5   // Número de Identificación Tributaria
}

/// <summary>
/// Lifecycle status of a PQRSDF ticket.
/// </summary>
public enum TicketStatus
{
    Registered = 1,   // Registrado
    Assigned = 2,     // Asignado
    InReview = 3,     // En trámite
    Answered = 4,     // Respondido
    Closed = 5        // Cerrado
}
