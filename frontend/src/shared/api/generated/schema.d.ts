/**
 * Baseline OpenAPI TypeScript schema definition.
 * Auto-generated or refreshed via `pnpm generate-api`.
 */

export interface paths {
  '/api/pqrsdf': {
    get: {
      parameters?: {
        query?: {
          status?: string;
        };
      };
      responses: {
        200: {
          content: {
            'application/json': Array<{
              id: string;
              radicadoNumber: string;
              type: string;
              status: string;
              creationDate: string;
            }>;
          };
        };
        400: {
          content: {
            'application/json': {
              title?: string;
              detail?: string;
              status?: number;
            };
          };
        };
        500: {
          content: {
            'application/json': {
              title?: string;
              detail?: string;
              status?: number;
            };
          };
        };
      };
    };
  };
}

export interface webhooks {}

export interface components {
  schemas: {
    PqrsdfSummaryDto: {
      id: string;
      radicadoNumber: string;
      type: string;
      status: string;
      creationDate: string;
    };
    ProblemDetails: {
      title?: string;
      detail?: string;
      status?: number;
      instance?: string;
    };
  };
}

export interface operations {}
