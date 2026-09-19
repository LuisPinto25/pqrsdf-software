'use client';

import React from 'react';
import { History, User, Clock, ArrowRight } from 'lucide-react';
import { TicketStatusHistoryItemDto } from '../types/ticket-management.types';

interface TicketStatusHistoryListProps {
  history: TicketStatusHistoryItemDto[];
}

export function TicketStatusHistoryList({ history }: TicketStatusHistoryListProps) {
  const formatDate = (isoString: string) => {
    try {
      return new Date(isoString).toLocaleString('es-CO', {
        dateStyle: 'medium',
        timeStyle: 'short',
      });
    } catch {
      return isoString;
    }
  };

  if (!history || history.length === 0) {
    return (
      <div className="py-10 text-center space-y-2.5 bg-slate-50 rounded-2xl border border-slate-200">
        <History className="w-8 h-8 text-slate-300 mx-auto" />
        <p className="text-xs font-medium text-slate-600">Sin historial registrado</p>
        <p className="text-[11px] text-slate-400 max-w-xs mx-auto">
          No se han registrado cambios de estado o justificaciones previas para este radicado.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center space-x-2 text-xs font-bold text-slate-900 uppercase tracking-wider">
        <History className="w-4 h-4 text-institutional-600" />
        <span>Historial de Actualizaciones ({history.length})</span>
      </div>

      <div className="divide-y divide-slate-100 border border-slate-200 rounded-xl overflow-hidden bg-white">
        {history.map((item) => (
          <div key={item.id} className="p-4 space-y-2 text-xs hover:bg-slate-50/70 transition-colors">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div className="flex items-center space-x-1.5 font-medium text-slate-700">
                <span className="px-2 py-0.5 rounded-md bg-slate-100 text-slate-700 text-[11px] font-semibold">
                  {item.previousStatusName}
                </span>
                <ArrowRight className="w-3 h-3 text-slate-400" />
                <span className="px-2 py-0.5 rounded-md bg-institutional-50 text-institutional-700 text-[11px] font-semibold">
                  {item.newStatusName}
                </span>
              </div>
              <div className="flex items-center space-x-1 text-[11px] text-slate-400">
                <Clock className="w-3.5 h-3.5" />
                <span>{formatDate(item.changedAtUtc)}</span>
              </div>
            </div>

            <div className="bg-slate-50 rounded-lg p-2.5 border border-slate-100 text-slate-800 leading-relaxed">
              <span className="font-semibold text-slate-900 block mb-0.5 text-[11px]">Justificación:</span>
              <p className="whitespace-pre-wrap">{item.justification}</p>
            </div>

            <div className="flex items-center space-x-1.5 text-[11px] text-slate-500">
              <User className="w-3.5 h-3.5 text-slate-400" />
              <span>Registrado por: <strong className="text-slate-700">{item.changedByOfficialName}</strong></span>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
