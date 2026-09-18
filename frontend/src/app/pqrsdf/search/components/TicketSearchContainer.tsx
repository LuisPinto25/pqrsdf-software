'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import { AlertCircle } from 'lucide-react';
import { useTicketSearch } from '../hooks/useTicketSearch';
import { TicketSearchBox } from './TicketSearchBox';
import { TicketStatusHeader } from './TicketStatusHeader';
import { TicketTimeline } from './TicketTimeline';
import { TicketDetailCard } from './TicketDetailCard';
import { TicketResolutionCard } from './TicketResolutionCard';

export interface TicketSearchContainerProps {
  initialRadicado?: string;
}

export function TicketSearchContainer({ initialRadicado }: TicketSearchContainerProps = {}) {
  const t = useTranslations('PqrsdfSearch');
  const { ticket, isLoading, error, radicadoFromUrl, searchTicket, clearSearch } =
    useTicketSearch(initialRadicado);

  return (
    <div className="space-y-6">
      {/* Search Input Box */}
      <TicketSearchBox
        initialRadicado={radicadoFromUrl}
        isLoading={isLoading}
        onSearch={searchTicket}
        onClear={clearSearch}
      />

      {/* Error Alert Message */}
      {error && !isLoading && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-5 text-red-800 flex items-start space-x-3 shadow-sm animate-fade-in">
          <AlertCircle className="w-5 h-5 text-red-600 mt-0.5 flex-shrink-0" />
          <div className="space-y-1">
            <h3 className="text-sm font-bold text-red-900">{t('errors.notFoundTitle')}</h3>
            <p className="text-sm text-red-700">{error}</p>
          </div>
        </div>
      )}

      {/* Ticket Details & Timeline View */}
      {ticket && !isLoading && (
        <div className="space-y-6 animate-fade-in">
          <TicketStatusHeader ticket={ticket} />

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 space-y-6">
              {/* If resolution is present (Answered or Closed), display prominently */}
              {(ticket.status === 'Closed' || ticket.status === 'Answered' || ticket.resolution) && (
                <TicketResolutionCard resolution={ticket.resolution} />
              )}

              {/* Subject & Description Content */}
              <TicketDetailCard subject={ticket.subject} description={ticket.description} />
            </div>

            {/* Timeline Column */}
            <div className="lg:col-span-1">
              <TicketTimeline timeline={ticket.timeline} />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
