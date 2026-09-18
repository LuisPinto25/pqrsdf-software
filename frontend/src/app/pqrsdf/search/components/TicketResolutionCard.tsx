'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import { CheckCircle, Calendar, MessageSquare } from 'lucide-react';
import type { TicketResolutionDto } from '@/app/pqrsdf/types/pqrsdf';

interface TicketResolutionCardProps {
  resolution?: TicketResolutionDto | null;
}

export function TicketResolutionCard({ resolution }: TicketResolutionCardProps) {
  const t = useTranslations('PqrsdfSearch.resolution');

  if (!resolution) {
    return (
      <div className="bg-slate-50 rounded-xl border border-dashed border-slate-300 p-6 text-center space-y-2">
        <p className="text-sm font-medium text-slate-600">{t('pendingResponse')}</p>
      </div>
    );
  }

  const formattedDate = (() => {
    try {
      const date = new Date(resolution.responseDate);
      return new Intl.DateTimeFormat('es-CO', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      }).format(date);
    } catch {
      return resolution.responseDate;
    }
  })();

  return (
    <div className="bg-white rounded-xl border-2 border-emerald-200 p-6 sm:p-8 shadow-sm space-y-4">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 border-b border-emerald-100 pb-3">
        <div className="flex items-center space-x-2">
          <MessageSquare className="w-5 h-5 text-emerald-600" />
          <h2 className="text-lg font-bold text-slate-900">{t('title')}</h2>
        </div>
        <span className="inline-flex items-center px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800">
          <CheckCircle className="w-3.5 h-3.5 mr-1 text-emerald-600" />
          {t('completedBadge')}
        </span>
      </div>

      <div className="flex items-center text-xs text-slate-500 space-x-1.5">
        <Calendar className="w-3.5 h-3.5 text-slate-400" />
        <span>{t('dateLabel')}</span>
        <span className="font-semibold text-slate-700">{formattedDate}</span>
      </div>

      <div className="p-4 sm:p-5 rounded-lg bg-emerald-50/50 border border-emerald-100 text-slate-800 text-sm sm:text-base whitespace-pre-wrap leading-relaxed">
        {resolution.responseText}
      </div>
    </div>
  );
}
