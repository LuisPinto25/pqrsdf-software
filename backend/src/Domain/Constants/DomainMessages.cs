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
}
