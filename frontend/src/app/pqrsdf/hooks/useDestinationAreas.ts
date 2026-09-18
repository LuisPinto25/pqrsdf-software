'use client';

import { useState, useEffect, useCallback } from 'react';
import { safeRequest, rawClient } from '@/shared/api/client';
import type { DestinationAreaDto } from '../types/pqrsdf';

export function useDestinationAreas() {
  const [areas, setAreas] = useState<DestinationAreaDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchAreas = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    const result = await safeRequest<{ value: DestinationAreaDto[] }>(() =>
      rawClient.GET('/api/v1/pqrsdf/areas')
    );

    if (result.ok) {
      const data = result.value;
      const areaList = Array.isArray(data) ? data : data?.value || [];
      setAreas(areaList);
    } else {
      setError(result.error.message || 'Error al cargar las áreas de destino.');
    }

    setIsLoading(false);
  }, []);

  useEffect(() => {
    fetchAreas();
  }, [fetchAreas]);

  return {
    areas,
    isLoading,
    error,
    refetch: fetchAreas,
  };
}
