'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import { CheckCircle2, Circle, Clock, GitCommit } from 'lucide-react';
import type { TicketTimelineMilestoneDto } from '@/app/pqrsdf/types/pqrsdf';

interface TicketTimelineProps {
  timeline: TicketTimelineMilestoneDto[];
}

export function TicketTimeline({ timeline }: TicketTimelineProps) {
  const t = useTranslations('PqrsdfSearch.timeline');

  const formatDate = (dateStr: string | null) => {
    if (!dateStr) return null;
    try {
      const date = new Date(dateStr);
      return new Intl.DateTimeFormat('es-CO', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      }).format(date);
    } catch {
      return dateStr;
    }
  };

  return (
    <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-6">
      <div className="flex items-center space-x-2 border-b border-slate-100 pb-3">
        <GitCommit className="w-5 h-5 text-institutional-600" />
        <h2 className="text-lg font-bold text-slate-900">{t('title')}</h2>
      </div>

      <div className="relative pl-6 sm:pl-8 space-y-8 before:absolute before:left-3 sm:before:left-4 before:top-3 before:bottom-3 before:w-0.5 before:bg-slate-200">
        {timeline.map((milestone, index) => {
          const isCompleted = milestone.isCompleted;
          const isCurrent = milestone.isCurrent;
          const formattedDate = formatDate(milestone.date);

          return (
            <div key={milestone.status || index} className="relative group">
              {/* Milestone Icon Indicator */}
              <div
                className={`absolute -left-6 sm:-left-8 top-0.5 flex items-center justify-center w-6 h-6 rounded-full ring-4 ring-white transition ${
                  isCurrent
                    ? 'bg-institutional-600 text-white shadow-md'
                    : isCompleted
                    ? 'bg-emerald-600 text-white'
                    : 'bg-slate-100 text-slate-400 border border-slate-200'
                }`}
              >
                {isCurrent ? (
                  <Clock className="w-3.5 h-3.5 animate-spin" />
                ) : isCompleted ? (
                  <CheckCircle2 className="w-3.5 h-3.5" />
                ) : (
                  <Circle className="w-2.5 h-2.5 fill-current" />
                )}
              </div>

              {/* Milestone Content */}
              <div className="space-y-1">
                <div className="flex flex-wrap items-center gap-2">
                  <span
                    className={`text-base font-semibold ${
                      isCurrent
                        ? 'text-institutional-700 font-bold'
                        : isCompleted
                        ? 'text-slate-900'
                        : 'text-slate-400'
                    }`}
                  >
                    {milestone.title}
                  </span>

                  {isCurrent && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-semibold bg-institutional-100 text-institutional-800">
                      {t('current')}
                    </span>
                  )}
                  {isCompleted && !isCurrent && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-emerald-50 text-emerald-700">
                      {t('completed')}
                    </span>
                  )}
                  {!isCompleted && (
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-[11px] font-medium bg-slate-100 text-slate-500">
                      {t('pending')}
                    </span>
                  )}
                </div>

                {formattedDate && (
                  <p className="text-xs text-slate-500 font-mono">{formattedDate}</p>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
