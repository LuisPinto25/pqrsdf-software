import { describe, it, expect } from 'vitest';
import { safeRequest } from '@/shared/api/client';
import { isOk, isErr } from '@/shared/types/result';

describe('Shared API Client safeRequest', () => {
  it('should return Ok with typed data when response is successful (200 OK)', async () => {
    const mockData = [
      {
        id: '1',
        radicadoNumber: '2026-0001',
        type: 'Peticion',
        status: 'Radicado',
        creationDate: '2026-09-18',
      },
    ];

    const result = await safeRequest(async () => ({
      data: mockData,
      error: undefined,
      response: new Response(JSON.stringify(mockData), { status: 200, statusText: 'OK' }),
    }));

    expect(isOk(result)).toBe(true);
    if (isOk(result)) {
      expect(result.value).toEqual(mockData);
      expect(result.value.length).toBe(1);
      expect(result.value[0].radicadoNumber).toBe('2026-0001');
    }
  });

  it('should return Err with ApiError when server returns 404 Not Found', async () => {
    const result = await safeRequest(async () => ({
      data: undefined,
      error: { detail: 'Solicitud no encontrada' },
      response: new Response(null, { status: 404, statusText: 'Not Found' }),
    }));

    expect(isErr(result)).toBe(true);
    if (isErr(result)) {
      expect(result.error.status).toBe(404);
      expect(result.error.message).toBe('Solicitud no encontrada');
    }
  });

  it('should return Err with default Spanish message for 500 without detail', async () => {
    const result = await safeRequest(async () => ({
      data: undefined,
      error: undefined,
      response: new Response(null, { status: 500, statusText: 'Internal Server Error' }),
    }));

    expect(isErr(result)).toBe(true);
    if (isErr(result)) {
      expect(result.error.status).toBe(500);
      expect(result.error.message).toContain('Error interno del servidor');
    }
  });

  it('should return Err with status 0 and NETWORK_OFFLINE on rejected fetch (network error)', async () => {
    const result = await safeRequest(async () => {
      throw new Error('Failed to fetch');
    });

    expect(isErr(result)).toBe(true);
    if (isErr(result)) {
      expect(result.error.status).toBe(0);
      expect(result.error.code).toBe('NETWORK_OFFLINE');
      expect(result.error.message).toContain('No fue posible conectar con el servidor');
    }
  });

  it('should manage auth_token cookie correctly', async () => {
    const { getAuthToken, setAuthToken, removeAuthToken } = await import('@/shared/api/client');

    setAuthToken('test-jwt-token-12345', 3600);
    expect(getAuthToken()).toBe('test-jwt-token-12345');

    removeAuthToken();
    expect(getAuthToken()).toBeFalsy();
  });
});

