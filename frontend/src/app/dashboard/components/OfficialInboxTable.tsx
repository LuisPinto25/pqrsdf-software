'use client';

import React, { useState, useEffect, useCallback } from 'react';
import { 
  Inbox, 
  RefreshCw, 
  AlertCircle, 
  Eye,
  CheckCircle2
} from 'lucide-react';
import { OfficialInboxItemDto, OfficialInboxResponse } from '../assignments/types/assignment.types';
import { UrgencyBadge } from '@/shared/components/UrgencyBadge';
import { WorkloadBadge } from '@/shared/components/WorkloadBadge';
import { getOfficialInbox } from '@/shared/api/client';
import { ManageTicketDrawer } from './ManageTicketDrawer';

export function OfficialInboxTable() {
  const [inboxData, setInboxData] = useState<OfficialInboxResponse | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const [selectedTicket, setSelectedTicket] = useState<OfficialInboxItemDto | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState<boolean>(false);

  const fetchInbox = useCallback(async () => {
    setIsLoading(true);
    setErrorMessage(null);

    const result = await getOfficialInbox();
    if (result.ok) {
      setInboxData(result.value);
    } else {
      setErrorMessage(result.error.message || 'Error al consultar su bandeja de trabajo.');
    }
    setIsLoading(false);
  }, []);

  useEffect(() => {
    fetchInbox();
  }, [fetchInbox]);

  const formatDate = (isoString?: string) => {
    if (!isoString) return '-';
    try {
      return new Date(isoString).toLocaleDateString('es-CO', {
        month: 'short',
        day: 'numeric',
        year: 'numeric',
      });
    } catch {
      return isoString;
    }
  };

  const handleSelectTicket = (ticket: OfficialInboxItemDto) => {
    setSelectedTicket(ticket);
    setIsDrawerOpen(true);
  };

  const handleCloseDrawer = () => {
    setIsDrawerOpen(false);
    setSelectedTicket(null);
  };

  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm overflow-hidden space-y-4 p-6">
      {/* Inbox Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 border-b border-slate-100 pb-5">
        <div className="space-y-1">
          <div className="flex items-center space-x-2.5">
            <div className="w-9 h-9 rounded-xl bg-institutional-50 text-institutional-700 flex items-center justify-center">
              <Inbox className="w-5 h-5" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-slate-900">
                Mis Solicitudes Asignadas
              </h2>
              <p className="text-xs text-slate-500">
                Bandeja de trabajo personal con solicitudes en trámite bajo su responsabilidad.
              </p>
            </div>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <WorkloadBadge
            activeCount={inboxData?.activeCount ?? 0}
            maxCapacity={inboxData?.maxCapacity ?? 5}
          />

          <button
            onClick={fetchInbox}
            disabled={isLoading}
            className="p-2 text-slate-500 hover:text-institutional-600 hover:bg-slate-100 rounded-lg transition-colors border border-slate-200"
            title="Actualizar bandeja"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Error State */}
      {errorMessage && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 flex items-center justify-between">
          <div className="flex items-center space-x-2">
            <AlertCircle className="w-4 h-4 text-red-500 shrink-0" />
            <span>{errorMessage}</span>
          </div>
          <button
            onClick={fetchInbox}
            className="font-semibold underline hover:text-red-900"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Table Content */}
      {isLoading ? (
        <div className="py-12 text-center space-y-2">
          <RefreshCw className="w-6 h-6 text-institutional-600 animate-spin mx-auto" />
          <p className="text-xs text-slate-500">Cargando solicitudes asignadas...</p>
        </div>
      ) : !inboxData || inboxData.tickets.length === 0 ? (
        <div className="py-12 text-center space-y-3">
          <div className="w-12 h-12 bg-slate-100 rounded-full flex items-center justify-center mx-auto text-slate-400">
            <CheckCircle2 className="w-6 h-6 text-emerald-500" />
          </div>
          <h3 className="text-base font-semibold text-slate-800">
            Bandeja al día
          </h3>
          <p className="text-xs text-slate-500 max-w-sm mx-auto">
            No tiene solicitudes asignadas pendientes de respuesta en este momento.
          </p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm text-slate-600 border-collapse">
            <thead>
              <tr className="bg-slate-50/80 border-b border-slate-200 text-xs font-semibold text-slate-700 uppercase tracking-wider">
                <th className="py-3 px-4">Radicado</th>
                <th className="py-3 px-4">Tipo</th>
                <th className="py-3 px-4">Asunto</th>
                <th className="py-3 px-4">Área Destino</th>
                <th className="py-3 px-4">Asignado el</th>
                <th className="py-3 px-4">Vencimiento SLA</th>
                <th className="py-3 px-4 text-right">Acción</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {inboxData.tickets.map((ticket) => (
                <tr
                  key={ticket.id}
                  className="hover:bg-slate-50/80 transition-colors group cursor-pointer"
                  onClick={() => handleSelectTicket(ticket)}
                >
                  <td className="py-3.5 px-4 font-mono font-bold text-institutional-700 whitespace-nowrap">
                    {ticket.radicadoNumber}
                  </td>
                  <td className="py-3.5 px-4 whitespace-nowrap">
                    <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-800">
                      {ticket.typeName}
                    </span>
                  </td>
                  <td className="py-3.5 px-4 max-w-xs">
                    <span className="line-clamp-1 font-medium text-slate-900" title={ticket.subject}>
                      {ticket.subject}
                    </span>
                    {ticket.assignmentNote && (
                      <span className="line-clamp-1 text-[11px] text-slate-400 italic">
                        Nota: {ticket.assignmentNote}
                      </span>
                    )}
                  </td>
                  <td className="py-3.5 px-4 whitespace-nowrap text-xs text-slate-700">
                    {ticket.destinationAreaName}
                  </td>
                  <td className="py-3.5 px-4 whitespace-nowrap text-xs text-slate-500">
                    {formatDate(ticket.assignedAtUtc)}
                  </td>
                  <td className="py-3.5 px-4 whitespace-nowrap">
                    <UrgencyBadge
                      remainingBusinessDays={ticket.remainingBusinessDays}
                      urgencyLevel={ticket.urgencyLevel}
                    />
                  </td>
                  <td className="py-3.5 px-4 text-right whitespace-nowrap" onClick={(e) => e.stopPropagation()}>
                    <button
                      onClick={() => handleSelectTicket(ticket)}
                      className="inline-flex items-center space-x-1 px-3 py-1 text-xs font-semibold text-institutional-700 bg-institutional-50 hover:bg-institutional-100 rounded-lg border border-institutional-200 transition-colors"
                    >
                      <Eye className="w-3.5 h-3.5" />
                      <span>Gestionar</span>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Ticket Slide-over Management Drawer */}
      <ManageTicketDrawer
        radicado={selectedTicket?.radicadoNumber ?? null}
        isOpen={isDrawerOpen}
        onClose={handleCloseDrawer}
        onTicketUpdated={fetchInbox}
      />
    </div>
  );
}
