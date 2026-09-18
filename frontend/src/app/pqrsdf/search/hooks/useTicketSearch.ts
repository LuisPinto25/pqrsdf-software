'use client';

import { useState, useCallback, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { getTicketByRadicado } from '@/shared/api/client';
import type { PublicTicketStatusDto } from '@/app/pqrsdf/types/pqrsdf';

export function useTicketSearch(initialRadicado?: string) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const tErr = useTranslations('PqrsdfSearch.errors');

  const [ticket, setTicket] = useState<PublicTicketStatusDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const radicadoFromUrl = initialRadicado ?? searchParams.get('radicado')?.trim() ?? '';

  const searchTicket = useCallback(
    async (radicadoToSearch: string) => {
      const cleanRadicado = radicadoToSearch.trim();
      if (!cleanRadicado) return;

      setIsLoading(true);
      setError(null);

      // Synchronize URL with ?radicado=...
      const currentParam = searchParams.get('radicado');
      if (currentParam !== cleanRadicado) {
        router.replace(`/pqrsdf/search?radicado=${encodeURIComponent(cleanRadicado)}`, {
          scroll: false,
        });
      }

      const result = await getTicketByRadicado(cleanRadicado);

      if (result.ok) {
        setTicket(result.value);
      } else {
        setTicket(null);
        if (result.error.status === 404) {
          setError(tErr('notFoundMessage'));
        } else if (result.error.status === 429) {
          setError(tErr('rateLimitMessage'));
        } else {
          setError(result.error.message || tErr('generalError'));
        }
      }

      setIsLoading(false);
    },
    [router, searchParams, tErr]
  );

  const clearSearch = useCallback(() => {
    setTicket(null);
    setError(null);
    router.replace('/pqrsdf/search', { scroll: false });
  }, [router]);

  // Execute search automatically if radicado parameter is present in URL
  useEffect(() => {
    if (radicadoFromUrl && (!ticket || ticket.radicadoNumber !== radicadoFromUrl)) {
      searchTicket(radicadoFromUrl);
    }
  }, [radicadoFromUrl, searchTicket, ticket]);

  return {
    ticket,
    isLoading,
    error,
    radicadoFromUrl,
    searchTicket,
    clearSearch,
  };
}
