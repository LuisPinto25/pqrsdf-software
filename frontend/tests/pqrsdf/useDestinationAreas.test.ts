import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useDestinationAreas } from '@/app/pqrsdf/hooks/useDestinationAreas';
import * as apiClient from '@/shared/api/client';
import { ok, err } from '@/shared/types/result';

describe('useDestinationAreas', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetches and sets active areas successfully', async () => {
    const mockAreas = [
      { id: '3fa85f64-5717-4562-b3fc-2c963f66afa6', name: 'Atención al Ciudadano', code: 'ATC' },
      { id: '4fa85f64-5717-4562-b3fc-2c963f66afa6', name: 'Oficina Jurídica', code: 'JUR' },
    ];

    vi.spyOn(apiClient, 'safeRequest').mockResolvedValue(ok({ value: mockAreas } as never));

    const { result } = renderHook(() => useDestinationAreas());

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.areas).toHaveLength(2);
    expect(result.current.areas[0].name).toBe('Atención al Ciudadano');
    expect(result.current.error).toBeNull();
  });

  it('handles error state when API request fails', async () => {
    vi.spyOn(apiClient, 'safeRequest').mockResolvedValue(
      err({
        status: 500,
        message: 'No fue posible conectar con el servidor.',
      })
    );

    const { result } = renderHook(() => useDestinationAreas());

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.areas).toEqual([]);
    expect(result.current.error).toBe('No fue posible conectar con el servidor.');
  });
});
