import { describe, it, expect } from 'vitest';
import { ok, err, isOk, isErr, Result } from '@/shared/types/result';

describe('Result Pattern', () => {
  it('should create an Ok result and satisfy type guards', () => {
    const value = { id: 'pqrsdf-001', radicado: '2026-0001' };
    const result: Result<typeof value, string> = ok(value);

    expect(result.ok).toBe(true);
    expect(isOk(result)).toBe(true);
    expect(isErr(result)).toBe(false);

    if (isOk(result)) {
      expect(result.value.id).toBe('pqrsdf-001');
      expect(result.value.radicado).toBe('2026-0001');
    }
  });

  it('should create an Err result and satisfy type guards', () => {
    const error = new Error('Invalid input');
    const result: Result<string, Error> = err(error);

    expect(result.ok).toBe(false);
    expect(isOk(result)).toBe(false);
    expect(isErr(result)).toBe(true);

    if (isErr(result)) {
      expect(result.error.message).toBe('Invalid input');
    }
  });

  it('should handle custom error objects as Err type', () => {
    const customError = { status: 404, message: 'Recurso no encontrado' };
    const result: Result<unknown, typeof customError> = err(customError);

    expect(isErr(result)).toBe(true);
    if (isErr(result)) {
      expect(result.error.status).toBe(404);
      expect(result.error.message).toBe('Recurso no encontrado');
    }
  });
});
