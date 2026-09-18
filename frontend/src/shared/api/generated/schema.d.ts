/**
 * Baseline OpenAPI TypeScript schema definition.
 * Auto-generated or refreshed via `pnpm generate-api`.
 */

export interface paths {
  '/api/v1/pqrsdf/areas': {
    get: {
      responses: {
        200: {
          content: {
            'application/json': {
              value: Array<{
                id: string;
                name: string;
                code: string;
              }>;
            };
          };
        };
        400: {
          content: {
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
        500: {
          content: {
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
      };
    };
  };
  '/api/v1/pqrsdf': {
    post: {
      requestBody: {
        content: {
          'application/json': components['schemas']['MakePqrsdfRequest'];
        };
      };
      responses: {
        201: {
          content: {
            'application/json': {
              value: components['schemas']['MakePqrsdfResponse'];
            };
          };
        };
        400: {
          content: {
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
        500: {
          content: {
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
      };
    };
  };
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
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
        500: {
          content: {
            'application/json': components['schemas']['ProblemDetails'];
          };
        };
      };
    };
  };
}

export interface webhooks {}

export interface components {
  schemas: {
    MakePqrsdfRequest: {
      type: 'Petition' | 'Complaint' | 'Claim' | 'Suggestion' | 'Denunciation' | 'Compliment';
      destinationAreaId: string;
      isAnonymous: boolean;
      applicant?: {
        fullName: string;
        identificationType: 'CC' | 'CE' | 'TI' | 'PA' | 'NIT';
        identificationNumber: string;
        email: string;
        phoneNumber?: string | null;
      } | null;
      subject: string;
      description: string;
    };
    MakePqrsdfResponse: {
      id: string;
      radicadoNumber: string;
      type: 'Petition' | 'Complaint' | 'Claim' | 'Suggestion' | 'Denunciation' | 'Compliment';
      createdAtUtc: string;
      dueDate: string;
      businessDaysCount: number;
      status: string;
    };
    DestinationAreaDto: {
      id: string;
      name: string;
      code: string;
    };
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
