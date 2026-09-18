'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import { FileText } from 'lucide-react';

interface TicketDetailCardProps {
  subject: string;
  description: string;
}

export function TicketDetailCard({ subject, description }: TicketDetailCardProps) {
  const t = useTranslations('PqrsdfSearch.detail');

  return (
    <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-5">
      <div className="flex items-center space-x-2 border-b border-slate-100 pb-3">
        <FileText className="w-5 h-5 text-institutional-600" />
        <h2 className="text-lg font-bold text-slate-900">{t('title')}</h2>
      </div>

      <div className="space-y-4">
        <div>
          <span className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1">
            {t('subject')}
          </span>
          <p className="text-base font-medium text-slate-900 bg-slate-50/50 p-3 rounded-lg border border-slate-100">
            {subject}
          </p>
        </div>

        <div>
          <span className="block text-xs font-semibold uppercase tracking-wider text-slate-500 mb-1">
            {t('description')}
          </span>
          <div className="text-sm sm:text-base text-slate-700 bg-slate-50/50 p-4 rounded-lg border border-slate-100 whitespace-pre-wrap leading-relaxed font-sans">
            {description}
          </div>
        </div>
      </div>
    </div>
  );
}
