namespace Pqrsdf.Domain.Constants;

/// <summary>
/// Centralized catalog of user-facing domain messages, labels, and feedback in Spanish,
/// adhering strictly to Constitution Principle VII.
/// </summary>
public static class DomainMessages
{
    public static class General
    {
        public const string UnexpectedError = "Ha ocurrido un error inesperado al procesar su solicitud. Por favor intente más tarde.";
        public const string ValidationFailed = "La información enviada no cumple con las validaciones de negocio requeridas.";
        public const string NotFound = "El recurso solicitado no fue encontrado.";
        public const string Conflict = "Existe un conflicto con el estado actual del recurso.";
        public const string Unauthorized = "No cuenta con los permisos necesarios para realizar esta acción.";
    }

    public static class System
    {
        public const string Operational = "El sistema se encuentra operativo y listo para recibir solicitudes.";
        public const string Degraded = "El sistema se encuentra operando de forma degradada.";
        public const string DatabaseUnavailable = "No fue posible establecer conexión con la base de datos SQL Server.";
        public const string HealthCheckOk = "Salud del servicio confirmada exitosamente.";
    }

    public static class TicketManagement
    {
        public const string InvalidResponseText = "El texto de la respuesta institucional debe tener entre 10 y 4000 caracteres.";
        public const string AlreadyClosed = "La solicitud ya se encuentra en estado cerrado.";
        public const string JustificationRequired = "Debe ingresar una justificación obligatoria de al menos 10 caracteres.";
        public const string JustificationTooLong = "La justificación no puede exceder los 500 caracteres.";
        public const string Forbidden = "No tiene autorización para gestionar esta solicitud.";
        public const string NotFound = "No se encontró ninguna solicitud con el radicado especificado.";
        public const string ResponseSuccess = "La respuesta final ha sido registrada y el radicado se encuentra cerrado exitosamente.";
        public const string StatusChangeSuccess = "El estado y la justificación operativa han sido registrados exitosamente.";
    }
}
