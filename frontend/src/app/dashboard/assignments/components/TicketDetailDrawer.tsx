'use client';

import React, { useEffect } from 'react';
import { 
  X, 
  FileText, 
  Calendar, 
  Building2, 
  UserCheck, 
  ArrowRightLeft,
  Clock,
  User
} from 'lucide-react';
import { UnassignedTicketDto, AssignedTicketDto, OfficialInboxItemDto } from '../types/assignment.types';
import { UrgencyBadge } from '@/shared/components/UrgencyBadge';

interface TicketDetailDrawerProps {
  ticket: UnassignedTicketDto | AssignedTicketDto | OfficialInboxItemDto | null;
  isOpen: boolean;
  onClose: () => void;
  onAssign?: (ticket: UnassignedTicketDto) => void;
  onReassign?: (ticket: AssignedTicketDto) => void;
}

export function TicketDetailDrawer({
  ticket,
  isOpen,
  onClose,
  onAssign,
  onReassign,
}: TicketDetailDrawerProps) {
  // Prevent body scrolling when open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  if (!isOpen || !ticket) return null;

  const isAssigned = 'assignedToUserId' in ticket && Boolean(ticket.assignedToUserId);
  const assignedTicket = isAssigned ? (ticket as AssignedTicketDto) : null;
  const unassignedTicket = !isAssigned ? (ticket as UnassignedTicketDto) : null;

  const formatDate = (isoString?: string) => {
    if (!isoString) return '-';
    try {
      return new Date(isoString).toLocaleDateString('es-CO', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      });
    } catch {
      return isoString;
    }
  };

  return (
    <div className="fixed inset-0 z-50 overflow-hidden" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs transition-opacity duration-300" 
        onClick={onClose}
        aria-hidden="true" 
      />

      <div className="fixed inset-y-0 right-0 max-w-full flex pl-10">
        <div className="w-screen max-w-xl bg-white shadow-2xl flex flex-col transform transition-transform duration-300 ease-in-out">
          
          {/* Header */}
          <div className="px-6 py-5 bg-slate-50 border-b border-slate-200 flex items-center justify-between">
            <div className="space-y-1">
              <div className="flex items-center space-x-2">
                <span className="font-mono text-sm font-bold text-institutional-700 bg-institutional-50 px-2.5 py-0.5 rounded-md border border-institutional-200">
                  {ticket.radicadoNumber}
                </span>
                <span className="text-xs px-2 py-0.5 rounded-md font-medium bg-slate-200 text-slate-700">
                  {ticket.typeName}
                </span>
              </div>
              <h2 id="slide-over-title" className="text-lg font-bold text-slate-900 line-clamp-1">
                {ticket.subject}
              </h2>
            </div>
            <button
              onClick={onClose}
              className="rounded-lg p-2 text-slate-400 hover:text-slate-600 hover:bg-slate-200/60 transition-colors"
              aria-label="Cerrar panel"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          {/* Body Content */}
          <div className="flex-1 overflow-y-auto p-6 space-y-6">
            {/* SLA / Urgency Alert Card */}
            <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 flex items-center justify-between">
              <div className="space-y-1">
                <div className="text-xs text-slate-500 font-medium">Plazo Legal de Respuesta</div>
                <div className="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
                  <Clock className="w-4 h-4 text-slate-400" />
                  <span>Vence: {formatDate(ticket.dueDateUtc)}</span>
                </div>
              </div>
              <UrgencyBadge
                remainingBusinessDays={ticket.remainingBusinessDays}
                urgencyLevel={ticket.urgencyLevel}
              />
            </div>

            {/* General Metadata */}
            <div className="grid grid-cols-2 gap-4">
              <div className="p-3 bg-slate-50 rounded-lg border border-slate-100 space-y-1">
                <div className="text-xs text-slate-500 flex items-center gap-1">
                  <Building2 className="w-3.5 h-3.5" />
                  <span>Área de Destino</span>
                </div>
                <div className="text-sm font-semibold text-slate-900">
                  {ticket.destinationAreaName || 'Dependencia General'}
                </div>
              </div>

              <div className="p-3 bg-slate-50 rounded-lg border border-slate-100 space-y-1">
                <div className="text-xs text-slate-500 flex items-center gap-1">
                  <Calendar className="w-3.5 h-3.5" />
                  <span>Fecha de Radicación</span>
                </div>
                <div className="text-sm font-semibold text-slate-900">
                  {'filingDateUtc' in ticket && ticket.filingDateUtc ? formatDate(ticket.filingDateUtc) : '-'}
                </div>
              </div>
            </div>

            {/* Assigned Official Info (If Assigned by Admin) */}
            {isAssigned && assignedTicket && (
              <div className="p-4 bg-purple-50/50 rounded-xl border border-purple-200 space-y-3">
                <div className="text-xs font-bold text-purple-900 uppercase tracking-wider flex items-center gap-1.5">
                  <User className="w-4 h-4 text-purple-700" />
                  <span>Funcionario Responsable</span>
                </div>
                <div className="space-y-1">
                  <div className="text-sm font-semibold text-purple-950">
                    {assignedTicket.assignedOfficialName}
                  </div>
                  <div className="text-xs text-purple-700">
                    Asignado el {formatDate(assignedTicket.assignedAtUtc)}
                  </div>
                  {assignedTicket.assignmentNote && (
                    <div className="mt-2 text-xs bg-white p-2.5 rounded-lg border border-purple-100 text-slate-700">
                      <span className="font-semibold text-slate-900 block mb-1">Nota de asignación:</span>
                      {assignedTicket.assignmentNote}
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Assignment Note & Date for Official's personal inbox */}
            {!isAssigned && 'assignedAtUtc' in ticket && (
              <div className="p-4 bg-institutional-50/60 rounded-xl border border-institutional-200 space-y-2">
                <div className="text-xs font-bold text-institutional-900 uppercase tracking-wider flex items-center gap-1.5">
                  <User className="w-4 h-4 text-institutional-700" />
                  <span>Detalles de Asignación</span>
                </div>
                <div className="text-xs text-institutional-700">
                  Asignado el {formatDate(ticket.assignedAtUtc)}
                </div>
                {ticket.assignmentNote && (
                  <div className="mt-2 text-xs bg-white p-2.5 rounded-lg border border-institutional-100 text-slate-700">
                    <span className="font-semibold text-slate-900 block mb-1">Instrucción recibida:</span>
                    {ticket.assignmentNote}
                  </div>
                )}
              </div>
            )}

            {/* Full Citizen Description */}
            <div className="space-y-2">
              <div className="text-sm font-semibold text-slate-900 flex items-center gap-1.5">
                <FileText className="w-4 h-4 text-slate-500" />
                <span>Descripción Completa de la Solicitud</span>
              </div>
              <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 text-sm text-slate-800 leading-relaxed whitespace-pre-wrap font-sans max-h-80 overflow-y-auto">
                {ticket.description || 'Sin descripción adicional.'}
              </div>
            </div>
          </div>

          {/* Footer Action */}
          <div className="p-4 bg-slate-50 border-t border-slate-200 flex items-center justify-end space-x-3">
            <button
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-xl hover:bg-slate-100 transition-colors"
            >
              Cerrar
            </button>

            {!isAssigned && unassignedTicket && onAssign && (
              <button
                onClick={() => onAssign(unassignedTicket)}
                className="inline-flex items-center space-x-2 px-5 py-2 text-sm font-semibold text-white bg-institutional-600 hover:bg-institutional-700 rounded-xl shadow-xs transition-colors"
              >
                <UserCheck className="w-4 h-4" />
                <span>Asignar Solicitud</span>
              </button>
            )}

            {isAssigned && assignedTicket && onReassign && (
              <button
                onClick={() => onReassign(assignedTicket)}
                className="inline-flex items-center space-x-2 px-5 py-2 text-sm font-semibold text-white bg-purple-600 hover:bg-purple-700 rounded-xl shadow-xs transition-colors"
              >
                <ArrowRightLeft className="w-4 h-4" />
                <span>Reasignar Funcionario</span>
              </button>
            )}
          </div>

        </div>
      </div>
    </div>
  );
}
