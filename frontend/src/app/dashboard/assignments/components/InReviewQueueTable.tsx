'use client';

import React, { useState } from 'react';
import { 
  Search, 
  ArrowRightLeft, 
  Eye, 
  RefreshCw, 
  User, 
  Inbox
} from 'lucide-react';
import { AssignedTicketDto } from '../types/assignment.types';
import { UrgencyBadge } from '@/shared/components/UrgencyBadge';

interface OfficialFilterOption {
  id: string;
  fullName: string;
}

interface InReviewQueueTableProps {
  tickets: AssignedTicketDto[];
  isLoading: boolean;
  onRefresh: () => void;
  onSelectTicket: (ticket: AssignedTicketDto) => void;
  onReassignTicket: (ticket: AssignedTicketDto) => void;
  search?: string;
  onSearchChange: (search?: string) => void;
  officialId?: string;
  onOfficialChange: (officialId?: string) => void;
  officials: OfficialFilterOption[];
}

export function InReviewQueueTable({
  tickets,
  isLoading,
  onRefresh,
  onSelectTicket,
  onReassignTicket,
  search,
  onSearchChange,
  officialId,
  onOfficialChange,
  officials,
}: InReviewQueueTableProps) {
  const [searchInput, setSearchInput] = useState(search || '');

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSearchChange(searchInput.trim() || undefined);
  };

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

  return (
    <div className="space-y-4">
      {/* Search and Filters Bar */}
      <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-xs flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <form onSubmit={handleSearchSubmit} className="flex-1 max-w-md relative">
          <input
            type="text"
            placeholder="Buscar por radicado o asunto..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="w-full pl-10 pr-4 py-2 text-sm rounded-lg border border-slate-300 focus:outline-hidden focus:ring-2 focus:ring-purple-500 focus:border-purple-500 bg-slate-50 focus:bg-white transition-colors"
          />
          <Search className="w-4 h-4 text-slate-400 absolute left-3 top-2.5" />
        </form>

        <div className="flex flex-wrap items-center gap-2.5">
          {/* Filter by Official */}
          <div className="flex items-center space-x-1.5">
            <User className="w-4 h-4 text-slate-400" />
            <select
              value={officialId ?? ''}
              onChange={(e) => onOfficialChange(e.target.value || undefined)}
              className="text-xs py-2 px-3 rounded-lg border border-slate-300 bg-white text-slate-700 focus:outline-hidden focus:ring-2 focus:ring-purple-500 max-w-[200px] truncate"
            >
              <option value="">Todos los funcionarios</option>
              {officials.map((o) => (
                <option key={o.id} value={o.id}>
                  {o.fullName}
                </option>
              ))}
            </select>
          </div>

          {/* Refresh Button */}
          <button
            onClick={onRefresh}
            disabled={isLoading}
            className="p-2 text-slate-500 hover:text-purple-600 hover:bg-slate-100 rounded-lg transition-colors border border-slate-200"
            title="Actualizar lista"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
        </div>
      </div>

      {/* Table Container */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-xs overflow-hidden">
        {isLoading ? (
          <div className="py-16 text-center space-y-3">
            <RefreshCw className="w-8 h-8 text-purple-500 animate-spin mx-auto" />
            <p className="text-sm text-slate-500">Cargando solicitudes en trámite...</p>
          </div>
        ) : tickets.length === 0 ? (
          <div className="py-16 text-center space-y-3">
            <div className="w-12 h-12 bg-slate-100 rounded-full flex items-center justify-center mx-auto text-slate-400">
              <Inbox className="w-6 h-6" />
            </div>
            <h3 className="text-base font-semibold text-slate-800">
              No hay solicitudes en trámite
            </h3>
            <p className="text-xs text-slate-500 max-w-sm mx-auto">
              No se encontraron solicitudes con asignación activa que coincidan con los filtros aplicados.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-600 border-collapse">
              <thead>
                <tr className="bg-slate-50 border-b border-slate-200 text-xs font-semibold text-slate-700 uppercase tracking-wider">
                  <th className="py-3 px-4">Radicado</th>
                  <th className="py-3 px-4">Tipo</th>
                  <th className="py-3 px-4">Asunto</th>
                  <th className="py-3 px-4">Funcionario Asignado</th>
                  <th className="py-3 px-4">Asignado el</th>
                  <th className="py-3 px-4">Vencimiento SLA</th>
                  <th className="py-3 px-4 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {tickets.map((ticket) => (
                  <tr
                    key={ticket.id}
                    className="hover:bg-slate-50/80 transition-colors group cursor-pointer"
                    onClick={() => onSelectTicket(ticket)}
                  >
                    <td className="py-3 px-4 font-mono font-bold text-purple-700 whitespace-nowrap">
                      {ticket.radicadoNumber}
                    </td>
                    <td className="py-3 px-4 whitespace-nowrap">
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-slate-100 text-slate-800">
                        {ticket.typeName}
                      </span>
                    </td>
                    <td className="py-3 px-4 max-w-xs">
                      <span className="line-clamp-1 font-medium text-slate-900" title={ticket.subject}>
                        {ticket.subject}
                      </span>
                    </td>
                    <td className="py-3 px-4 whitespace-nowrap">
                      <div className="flex items-center space-x-1.5 text-xs text-slate-800 font-semibold">
                        <User className="w-3.5 h-3.5 text-purple-600 shrink-0" />
                        <span>{ticket.assignedOfficialName}</span>
                      </div>
                    </td>
                    <td className="py-3 px-4 whitespace-nowrap text-xs text-slate-500">
                      {formatDate(ticket.assignedAtUtc)}
                    </td>
                    <td className="py-3 px-4 whitespace-nowrap">
                      <UrgencyBadge
                        remainingBusinessDays={ticket.remainingBusinessDays}
                        urgencyLevel={ticket.urgencyLevel}
                      />
                    </td>
                    <td className="py-3 px-4 text-right whitespace-nowrap" onClick={(e) => e.stopPropagation()}>
                      <div className="flex items-center justify-end space-x-2">
                        <button
                          onClick={() => onSelectTicket(ticket)}
                          className="p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-100 rounded-lg transition-colors"
                          title="Ver detalle"
                        >
                          <Eye className="w-4 h-4" />
                        </button>
                        <button
                          onClick={() => onReassignTicket(ticket)}
                          className="inline-flex items-center space-x-1 px-3 py-1 text-xs font-semibold text-white bg-purple-600 hover:bg-purple-700 rounded-lg shadow-xs transition-colors"
                        >
                          <ArrowRightLeft className="w-3.5 h-3.5" />
                          <span>Reasignar</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
