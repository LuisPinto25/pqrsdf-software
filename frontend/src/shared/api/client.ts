import createClient from 'openapi-fetch';
import type { paths } from '@/shared/api/generated/schema';
import { ok, err, Result } from '@/shared/types/result';
import type { ApiError } from '@/shared/types/api';

const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5023';

/**
 * Underlying typed openapi-fetch client instance.
 */
export const rawClient = createClient<paths>({
  baseUrl,
});

// Configure auth header injection and 401 interceptor for openapi-fetch
rawClient.use({
  onRequest({ request }) {
    const token = getAuthToken();
    if (token) {
      request.headers.set('Authorization', `Bearer ${token}`);
    }
    return request;
  },
  onResponse({ response }) {
    if (response.status === 401 && typeof window !== 'undefined') {
      removeAuthToken();
      try {
        localStorage.removeItem('auth_user');
      } catch {
        // Ignore
      }
      const currentPath = window.location.pathname + window.location.search;
      if (!currentPath.startsWith('/auth')) {
        const returnUrl = encodeURIComponent(currentPath);
        window.location.href = `/auth?expired=true&returnUrl=${returnUrl}`;
      }
    }
    return response;
  },
});


/**
 * Maps HTTP status codes to friendly user-facing messages in Spanish.
 */
function mapStatusToMessage(status: number, detail?: string): string {
  if (detail) {
    return detail;
  }
  switch (status) {
    case 400:
      return 'Solicitud incorrecta. Verifique los datos enviados.';
    case 401:
      return 'No autorizado para acceder a este recurso.';
    case 403:
      return 'Acceso restringido. No tiene permisos suficientes.';
    case 404:
      return 'El recurso solicitado no fue encontrado.';
    case 500:
    case 502:
    case 503:
      return 'Error interno del servidor. Por favor intente más tarde.';
    default:
      return 'Ha ocurrido un error al procesar la solicitud.';
  }
}

/**
 * Executes an openapi-fetch request function safely, capturing HTTP errors
 * and network rejections into a Result<T, ApiError> without throwing unhandled exceptions.
 */
export async function safeRequest<T>(
  requestFn: () => Promise<{ data?: T; error?: unknown; response: Response }>
): Promise<Result<T, ApiError>> {
  try {
    const { data, error, response } = await requestFn();

    if (response.ok && data !== undefined) {
      return ok(data);
    }

    const status = response.status || 500;
    const detail =
      error && typeof error === 'object' && 'detail' in error
        ? String((error as Record<string, unknown>).detail)
        : undefined;

    return err({
      status,
      message: mapStatusToMessage(status, detail),
      details:
        typeof error === 'object' && error !== null
          ? (error as Record<string, unknown>)
          : undefined,
    });
  } catch (caughtError) {
    return err({
      status: 0,
      message: 'No fue posible conectar con el servidor. Compruebe su conexión de red.',
      code: 'NETWORK_OFFLINE',
      details: caughtError instanceof Error ? { message: caughtError.message } : undefined,
    });
  }
}

/**
 * Retrieves public tracking details for a ticket by radicado number.
 */
export async function getTicketByRadicado(
  radicado: string
): Promise<Result<import('@/app/pqrsdf/types/pqrsdf').PublicTicketStatusDto, ApiError>> {
  return safeRequest<import('@/app/pqrsdf/types/pqrsdf').PublicTicketStatusDto>(async () => {
    const url = `${baseUrl}/api/v1/pqrsdf/${encodeURIComponent(radicado.trim())}`;
    const res = await fetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail = json?.error?.message ?? json?.detail ?? mapStatusToMessage(res.status);
    return { error: { detail }, response: res };
  });
}

export interface SharedDestinationAreaDto {
  id: string;
  name: string;
  code: string;
  isActive: boolean;
}

/**
 * Retrieves active organizational destination areas.
 */
export async function getActiveDestinationAreas(): Promise<Result<SharedDestinationAreaDto[], ApiError>> {
  return safeRequest<SharedDestinationAreaDto[]>(async () => {
    const url = `${baseUrl}/api/v1/pqrsdf/areas`;
    const res = await fetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: Array.isArray(payload) ? payload : payload?.value || [], response: res };
    }

    const detail = json?.error?.message ?? json?.detail ?? mapStatusToMessage(res.status);
    return { error: { detail }, response: res };
  });
}

/**
 * Reads the active JWT token from the client document cookie.
 */
export function getAuthToken(): string | null {
  if (typeof document === 'undefined') return null;
  const match = document.cookie.match(new RegExp('(^| )auth_token=([^;]+)'));
  return match ? decodeURIComponent(match[2]) : null;
}

/**
 * Sets the authentication JWT token cookie for Next.js middleware and API requests.
 */
export function setAuthToken(token: string, maxAgeSeconds: number = 28800): void {
  if (typeof document === 'undefined') return;
  document.cookie = `auth_token=${encodeURIComponent(token)}; Path=/; Max-Age=${maxAgeSeconds}; SameSite=Lax`;
}

/**
 * Removes the authentication JWT token cookie.
 */
export function removeAuthToken(): void {
  if (typeof document === 'undefined') return;
  document.cookie = 'auth_token=; Path=/; Max-Age=0; SameSite=Lax';
}

/**
 * Authenticates internal staff credentials against POST /api/v1/auth/login.
 */
export async function loginStaff(
  credentials: import('@/app/auth/types/auth.types').LoginCredentials
): Promise<Result<import('@/app/auth/types/auth.types').LoginResponse, ApiError>> {
  return safeRequest<import('@/app/auth/types/auth.types').LoginResponse>(async () => {
    const url = `${baseUrl}/api/v1/auth/login`;
    const res = await fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify(credentials),
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Custom fetch wrapper attaching Authorization: Bearer <token> from cookie
 * and intercepting 401 Unauthorized responses to perform session expiration handling.
 */
export async function authenticatedFetch(
  input: RequestInfo | URL,
  init?: RequestInit
): Promise<Response> {
  const token = getAuthToken();
  const headers = new Headers(init?.headers);

  if (token && !headers.has('Authorization')) {
    headers.set('Authorization', `Bearer ${token}`);
  }

  const response = await fetch(input, {
    ...init,
    headers,
  });

  if (response.status === 401 && typeof window !== 'undefined') {
    removeAuthToken();
    try {
      localStorage.removeItem('auth_user');
    } catch {
      // Ignore
    }
    const currentPath = window.location.pathname + window.location.search;
    if (!currentPath.startsWith('/auth')) {
      const returnUrl = encodeURIComponent(currentPath);
      window.location.href = `/auth?expired=true&returnUrl=${returnUrl}`;
    }
  }

  return response;
}

/**
 * Retrieves the currently authenticated staff profile from GET /api/v1/auth/me.
 */
export async function getCurrentUserProfile(): Promise<
  Result<import('@/app/auth/types/auth.types').UserProfile, ApiError>
> {
  return safeRequest<import('@/app/auth/types/auth.types').UserProfile>(async () => {
    const url = `${baseUrl}/api/v1/auth/me`;
    const res = await authenticatedFetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Retrieves unassigned tickets list with optional filters.
 */
export async function getUnassignedTickets(filters?: {
  type?: number;
  destinationAreaId?: string;
  search?: string;
}): Promise<Result<import('@/app/dashboard/assignments/types/assignment.types').UnassignedTicketDto[], ApiError>> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').UnassignedTicketDto[]>(async () => {
    const params = new URLSearchParams();
    if (filters?.type !== undefined && filters.type !== null && !isNaN(filters.type)) {
      params.append('type', filters.type.toString());
    }
    if (filters?.destinationAreaId) {
      params.append('destinationAreaId', filters.destinationAreaId);
    }
    if (filters?.search) {
      params.append('search', filters.search);
    }
    const query = params.toString() ? `?${params.toString()}` : '';
    const url = `${baseUrl}/api/v1/assignments/unassigned${query}`;
    const res = await authenticatedFetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Retrieves officials list with active workloads.
 */
export async function getAssignableOfficials(): Promise<
  Result<import('@/app/dashboard/assignments/types/assignment.types').AssignableOfficialDto[], ApiError>
> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').AssignableOfficialDto[]>(async () => {
    const url = `${baseUrl}/api/v1/assignments/officials`;
    const res = await authenticatedFetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Assigns a registered ticket to an official.
 */
export async function assignTicket(
  radicado: string,
  data: import('@/app/dashboard/assignments/types/assignment.types').AssignTicketRequest
): Promise<Result<import('@/app/dashboard/assignments/types/assignment.types').AssignTicketResponse, ApiError>> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').AssignTicketResponse>(async () => {
    const url = `${baseUrl}/api/v1/assignments/${encodeURIComponent(radicado.trim())}/assign`;
    const res = await authenticatedFetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify(data),
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Retrieves in-review assigned tickets for administrative oversight.
 */
export async function getAssignedTickets(filters?: {
  destinationAreaId?: string;
  officialId?: string;
  search?: string;
}): Promise<Result<import('@/app/dashboard/assignments/types/assignment.types').AssignedTicketDto[], ApiError>> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').AssignedTicketDto[]>(async () => {
    const params = new URLSearchParams();
    if (filters?.destinationAreaId) {
      params.append('destinationAreaId', filters.destinationAreaId);
    }
    if (filters?.officialId) {
      params.append('officialId', filters.officialId);
    }
    if (filters?.search) {
      params.append('search', filters.search);
    }
    const query = params.toString() ? `?${params.toString()}` : '';
    const url = `${baseUrl}/api/v1/assignments/assigned${query}`;
    const res = await authenticatedFetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Reassigns an in-review ticket to a different official with justification.
 */
export async function reassignTicket(
  radicado: string,
  data: import('@/app/dashboard/assignments/types/assignment.types').ReassignTicketRequest
): Promise<Result<import('@/app/dashboard/assignments/types/assignment.types').ReassignTicketResponse, ApiError>> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').ReassignTicketResponse>(async () => {
    const url = `${baseUrl}/api/v1/assignments/${encodeURIComponent(radicado.trim())}/reassign`;
    const res = await authenticatedFetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify(data),
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}

/**
 * Retrieves the current official's active assigned inbox tickets.
 */
export async function getOfficialInbox(): Promise<
  Result<import('@/app/dashboard/assignments/types/assignment.types').OfficialInboxResponse, ApiError>
> {
  return safeRequest<import('@/app/dashboard/assignments/types/assignment.types').OfficialInboxResponse>(async () => {
    const url = `${baseUrl}/api/v1/assignments/inbox`;
    const res = await authenticatedFetch(url, {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    });

    const json = await res.json().catch(() => undefined);

    if (res.ok && json) {
      const payload = json.value ?? json;
      return { data: payload, response: res };
    }

    const detail =
      json?.error?.message ??
      json?.detail ??
      mapStatusToMessage(res.status);

    return { error: { detail }, response: res };
  });
}



