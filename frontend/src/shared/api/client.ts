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
