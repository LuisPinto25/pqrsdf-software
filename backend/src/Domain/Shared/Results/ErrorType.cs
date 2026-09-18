namespace Pqrsdf.Domain.Shared.Results;

/// <summary>
/// Categorization of domain and application errors.
/// </summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4
}
