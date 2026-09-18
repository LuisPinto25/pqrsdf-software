/**
 * Standardized HTTP error contract for frontend API interactions.
 * Localized messages guarantee friendly user presentation in Spanish.
 */

export interface ApiError {
  /**
   * HTTP status code (400, 401, 403, 404, 500, etc.).
   * Status 0 indicates client-side network error or timeout.
   */
  readonly status: number;

  /**
   * User-facing error message localized in Spanish.
   */
  readonly message: string;

  /**
   * Machine-readable error code (e.g., 'VALIDATION_ERROR', 'NETWORK_OFFLINE').
   */
  readonly code?: string;

  /**
   * Optional details or validation errors returned from ProblemDetails.
   */
  readonly details?: Record<string, unknown> | unknown[];
}
