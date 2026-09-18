'use client';

import React, { useState } from 'react';
import { useTranslations } from 'next-intl';
import { Copy, Check, Calendar, AlertTriangle, Clock, Building2, Tag } from 'lucide-react';
import type { PublicTicketStatusDto } from '@/app/pqrsdf/types/pqrsdf';

interface TicketStatusHeaderProps {
  ticket: PublicTicketStatusDto;
}

export function TicketStatusHeader({ ticket }: TicketStatusHeaderProps) {
  const t = useTranslations('PqrsdfSearch.status');
  const tStatuses = useTranslations('PqrsdfSearch.statuses');
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    try {
      await navigator.clipboard.writeText(ticket.radicadoNumber);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch {
      // Fallback if clipboard API is unavailable
    }
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Registered':
        return 'bg-blue-50 text-blue-700 border-blue-200';
      case 'Assigned':
        return 'bg-indigo-50 text-indigo-700 border-indigo-200';
      case 'InReview':
        return 'bg-amber-50 text-amber-700 border-amber-200';
      case 'Answered':
        return 'bg-emerald-50 text-emerald-700 border-emerald-200';
      case 'Closed':
        return 'bg-slate-100 text-slate-800 border-slate-300';
      default:
        return 'bg-slate-50 text-slate-700 border-slate-200';
    }
  };

  type StatusKey = 'Registered' | 'Assigned' | 'InReview' | 'Answered' | 'Closed';
  const isKnownStatus = (s: string): s is StatusKey =>
    ['Registered', 'Assigned', 'InReview', 'Answered', 'Closed'].includes(s);

  const statusLabel = isKnownStatus(ticket.status) ? tStatuses(ticket.status) : ticket.status;

  return (
    <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-6">
      {/* Top Bar: Radicado and Status */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-100 pb-5">
        <div className="space-y-1">
          <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">
            {t('radicadoLabel')}
          </span>
          <div className="flex items-center space-x-3">
            <span className="text-2xl sm:text-3xl font-mono font-bold text-slate-900">
              {ticket.radicadoNumber}
            </span>
            <button
              onClick={handleCopy}
              className="inline-flex items-center px-2.5 py-1 text-xs font-medium text-slate-600 bg-slate-100 hover:bg-slate-200 rounded-md transition"
              title={t('copyRadicado')}
            >
              {copied ? (
                <>
                  <Check className="w-3.5 h-3.5 mr-1 text-emerald-600" />
                  <span className="text-emerald-700 font-semibold">{t('copied')}</span>
                </>
              ) : (
                <>
                  <Copy className="w-3.5 h-3.5 mr-1 text-slate-500" />
                  <span>{t('copyRadicado')}</span>
                </>
              )}
            </button>
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {ticket.isOverdue && ticket.status !== 'Closed' && ticket.status !== 'Answered' && (
            <span className="inline-flex items-center px-3 py-1.5 rounded-full text-xs font-bold bg-red-100 text-red-800 border border-red-200 animate-pulse">
              <AlertTriangle className="w-3.5 h-3.5 mr-1 text-red-600" />
              {t('overdueBadge')}
            </span>
          )}
          <span
            className={`inline-flex items-center px-3 py-1.5 rounded-full text-xs font-semibold border ${getStatusBadge(
              ticket.status
            )}`}
          >
            {statusLabel}
          </span>
        </div>
      </div>

      {/* Details Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
        <div className="p-3.5 rounded-lg bg-slate-50/70 border border-slate-100 space-y-1">
          <div className="flex items-center text-slate-500 text-xs">
            <Tag className="w-3.5 h-3.5 mr-1.5 text-slate-400" />
            {t('typeLabel')}
          </div>
          <p className="font-semibold text-slate-800">{ticket.requestType}</p>
        </div>

        <div className="p-3.5 rounded-lg bg-slate-50/70 border border-slate-100 space-y-1">
          <div className="flex items-center text-slate-500 text-xs">
            <Building2 className="w-3.5 h-3.5 mr-1.5 text-slate-400" />
            {t('areaLabel')}
          </div>
          <p className="font-semibold text-slate-800">{ticket.destinationAreaName}</p>
        </div>

        <div className="p-3.5 rounded-lg bg-slate-50/70 border border-slate-100 space-y-1">
          <div className="flex items-center text-slate-500 text-xs">
            <Calendar className="w-3.5 h-3.5 mr-1.5 text-slate-400" />
            {t('filingDateLabel')}
          </div>
          <p className="font-semibold text-slate-800">{ticket.filingDate}</p>
        </div>

        <div className="p-3.5 rounded-lg bg-slate-50/70 border border-slate-100 space-y-1">
          <div className="flex items-center text-slate-500 text-xs">
            <Clock className="w-3.5 h-3.5 mr-1.5 text-slate-400" />
            {t('dueDateLabel')}
          </div>
          <p className="font-semibold text-slate-800">{ticket.dueDate}</p>
        </div>
      </div>

      {/* SLA Status Banner: Remaining days or Overdue warning */}
      {ticket.status !== 'Closed' && ticket.status !== 'Answered' && (
        <div>
          {ticket.isOverdue ? (
            <div className="flex items-center p-4 rounded-lg bg-red-50 border border-red-200 text-red-800 text-sm">
              <AlertTriangle className="w-5 h-5 mr-3 text-red-600 flex-shrink-0" />
              <div>
                <p className="font-bold">
                  {t('overdueLabel', { days: ticket.overdueBusinessDays ?? 1 })}
                </p>
                <p className="text-xs text-red-700 mt-0.5">
                  El plazo legal reglamentario para responder esta solicitud ha expirado.
                </p>
              </div>
            </div>
          ) : (
            <div className="flex items-center p-4 rounded-lg bg-emerald-50 border border-emerald-200 text-emerald-800 text-sm">
              <Clock className="w-5 h-5 mr-3 text-emerald-600 flex-shrink-0" />
              <div>
                <span className="font-bold">
                  {ticket.remainingBusinessDays === 0
                    ? t('dueToday')
                    : t('remainingDaysCount', { count: ticket.remainingBusinessDays ?? 0 })}
                </span>
                <span className="text-xs text-emerald-700 ml-2">
                  (cómputo exclusivo en días hábiles oficiales colombianos)
                </span>
              </div>
            </div>
          )}
        </div>
      )}

      {(ticket.status === 'Closed' || ticket.status === 'Answered') && (
        <div className="flex items-center p-3.5 rounded-lg bg-slate-100 text-slate-700 text-sm">
          <Check className="w-4 h-4 mr-2 text-slate-500 flex-shrink-0" />
          <span className="font-medium">{t('closedNotice')}</span>
        </div>
      )}
    </div>
  );
}
