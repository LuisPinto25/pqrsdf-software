'use client';

import React, { useState, useEffect, useCallback } from 'react';
import {
  X,
  FileText,
  Calendar,
  Building2,
  Clock,
  User,
  Mail,
  Phone,
  ShieldCheck,
  Send,
  RefreshCw,
  History,
  AlertCircle,
  CheckCircle2,
} from 'lucide-react';
import { TicketManagementDetailDto } from '../types/ticket-management.types';
import { UrgencyBadge } from '@/shared/components/UrgencyBadge';
import { getTicketManagementDetail } from '@/shared/api/client';
import { TicketResponseForm } from './TicketResponseForm';
import { TicketStatusForm } from './TicketStatusForm';
import { TicketStatusHistoryList } from './TicketStatusHistoryList';

interface ManageTicketDrawerProps {
  radicado: string | null;
  isOpen: boolean;
  onClose: () => void;
  onTicketUpdated: () => void;
}

type TabType = 'overview' | 'response' | 'status' | 'history';

export function ManageTicketDrawer({
  radicado,
  isOpen,
  onClose,
  onTicketUpdated,
}: ManageTicketDrawerProps) {
  const [ticketDetail, setTicketDetail] = useState<TicketManagementDetailDto | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<TabType>('overview');
  const [actionNotification, setActionNotification] = useState<string | null>(null);

  // Prevent background scroll when drawer is open
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

  const loadTicketDetail = useCallback(async () => {
    if (!radicado) return;
    setIsLoading(true);
    setErrorMessage(null);

    const result = await getTicketManagementDetail(radicado);
    if (result.ok) {
      setTicketDetail(result.value);
    } else {
      setErrorMessage(result.error.message || 'No fue posible cargar el detalle del radicado.');
    }
    setIsLoading(false);
  }, [radicado]);

  useEffect(() => {
    if (isOpen && radicado) {
      setActiveTab('overview');
      setActionNotification(null);
      loadTicketDetail();
    }
  }, [isOpen, radicado, loadTicketDetail]);

  const handleResponseSuccess = () => {
    setActionNotification('La respuesta institucional fue emitida exitosamente y el radicado ha sido cerrado.');
    onTicketUpdated();
    loadTicketDetail();
  };

  const handleStatusSuccess = () => {
    setActionNotification('El estado y la justificación operativa fueron registrados correctamente.');
    onTicketUpdated();
    loadTicketDetail();
  };

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

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden" aria-labelledby="slide-over-title" role="dialog" aria-modal="true">
      {/* Backdrop */}
      <div
        className="fixed inset-0 bg-slate-900/50 backdrop-blur-xs transition-opacity duration-300"
        onClick={onClose}
        aria-hidden="true"
      />

      <div className="fixed inset-y-0 right-0 max-w-full flex pl-10">
        <div className="w-screen max-w-2xl bg-white shadow-2xl flex flex-col transform transition-transform duration-300 ease-in-out">

          {/* Header */}
          <div className="px-6 py-5 bg-slate-50 border-b border-slate-200 flex items-center justify-between">
            <div className="space-y-1">
              <div className="flex items-center space-x-2">
                <span className="font-mono text-sm font-bold text-institutional-700 bg-institutional-50 px-2.5 py-0.5 rounded-md border border-institutional-200">
                  {radicado}
                </span>
                {ticketDetail && (
                  <span className="text-xs px-2 py-0.5 rounded-md font-medium bg-slate-200 text-slate-700">
                    {ticketDetail.typeName}
                  </span>
                )}
                {ticketDetail && (
                  <span
                    className={`text-xs px-2.5 py-0.5 rounded-md font-semibold ${
                      ticketDetail.status === 5
                        ? 'bg-emerald-100 text-emerald-800'
                        : 'bg-blue-100 text-blue-800'
                    }`}
                  >
                    {ticketDetail.statusName}
                  </span>
                )}
              </div>
              <h2 id="slide-over-title" className="text-base font-bold text-slate-900 line-clamp-1">
                {ticketDetail ? ticketDetail.subject : 'Gestión de Solicitud'}
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

          {/* Tabs Navigation */}
          {ticketDetail && (
            <div className="flex border-b border-slate-200 bg-slate-50/50 px-6 pt-2 space-x-2">
              <button
                onClick={() => setActiveTab('overview')}
                className={`pb-2.5 px-3 text-xs font-semibold border-b-2 transition-all flex items-center gap-1.5 ${
                  activeTab === 'overview'
                    ? 'border-institutional-600 text-institutional-700'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                <FileText className="w-3.5 h-3.5" />
                <span>Detalle</span>
              </button>

              {ticketDetail.canManage && ticketDetail.status !== 5 && (
                <>
                  <button
                    onClick={() => setActiveTab('response')}
                    className={`pb-2.5 px-3 text-xs font-semibold border-b-2 transition-all flex items-center gap-1.5 ${
                      activeTab === 'response'
                        ? 'border-institutional-600 text-institutional-700'
                        : 'border-transparent text-slate-500 hover:text-slate-800'
                    }`}
                  >
                    <Send className="w-3.5 h-3.5" />
                    <span>Emitir Respuesta</span>
                  </button>

                  <button
                    onClick={() => setActiveTab('status')}
                    className={`pb-2.5 px-3 text-xs font-semibold border-b-2 transition-all flex items-center gap-1.5 ${
                      activeTab === 'status'
                        ? 'border-institutional-600 text-institutional-700'
                        : 'border-transparent text-slate-500 hover:text-slate-800'
                    }`}
                  >
                    <RefreshCw className="w-3.5 h-3.5" />
                    <span>Actualizar Estado</span>
                  </button>
                </>
              )}

              <button
                onClick={() => setActiveTab('history')}
                className={`pb-2.5 px-3 text-xs font-semibold border-b-2 transition-all flex items-center gap-1.5 ${
                  activeTab === 'history'
                    ? 'border-institutional-600 text-institutional-700'
                    : 'border-transparent text-slate-500 hover:text-slate-800'
                }`}
              >
                <History className="w-3.5 h-3.5" />
                <span>Historial ({ticketDetail.statusHistory.length})</span>
              </button>
            </div>
          )}

          {/* Success Banner */}
          {actionNotification && (
            <div className="mx-6 mt-4 p-3.5 bg-emerald-50 border border-emerald-200 rounded-xl text-xs text-emerald-800 flex items-center justify-between">
              <div className="flex items-center space-x-2">
                <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0" />
                <span>{actionNotification}</span>
              </div>
              <button
                onClick={() => setActionNotification(null)}
                className="text-emerald-700 hover:text-emerald-900 font-bold"
              >
                ×
              </button>
            </div>
          )}

          {/* Body Content */}
          <div className="flex-1 overflow-y-auto p-6 space-y-6">
            {isLoading ? (
              <div className="py-20 text-center space-y-2">
                <RefreshCw className="w-8 h-8 text-institutional-600 animate-spin mx-auto" />
                <p className="text-xs text-slate-500 font-medium">Cargando detalles de la solicitud...</p>
              </div>
            ) : errorMessage ? (
              <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 flex items-center justify-between">
                <div className="flex items-center space-x-2">
                  <AlertCircle className="w-4 h-4 text-red-500 shrink-0" />
                  <span>{errorMessage}</span>
                </div>
                <button
                  onClick={loadTicketDetail}
                  className="font-semibold underline hover:text-red-900"
                >
                  Reintentar
                </button>
              </div>
            ) : ticketDetail ? (
              <>
                {/* TAB: OVERVIEW */}
                {activeTab === 'overview' && (
                  <div className="space-y-6">
                    {/* SLA / Urgency Alert Card */}
                    <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 flex items-center justify-between">
                      <div className="space-y-1">
                        <div className="text-xs text-slate-500 font-medium">Plazo Legal de Respuesta</div>
                        <div className="text-sm font-semibold text-slate-800 flex items-center gap-1.5">
                          <Clock className="w-4 h-4 text-slate-400" />
                          <span>Vence: {formatDate(ticketDetail.dueDateUtc)}</span>
                        </div>
                      </div>
                      {ticketDetail.remainingBusinessDays !== null ? (
                        <UrgencyBadge
                          remainingBusinessDays={ticketDetail.remainingBusinessDays}
                          urgencyLevel={ticketDetail.urgencyLevel}
                        />
                      ) : (
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800 border border-emerald-300">
                          <CheckCircle2 className="w-3.5 h-3.5 text-emerald-600" />
                          <span>Concluido</span>
                        </span>
                      )}
                    </div>

                    {/* Metadata Grid */}
                    <div className="grid grid-cols-2 gap-4">
                      <div className="p-3 bg-slate-50 rounded-lg border border-slate-100 space-y-1">
                        <div className="text-xs text-slate-500 flex items-center gap-1">
                          <Building2 className="w-3.5 h-3.5" />
                          <span>Área de Destino</span>
                        </div>
                        <div className="text-sm font-semibold text-slate-900">
                          {ticketDetail.destinationAreaName}
                        </div>
                      </div>

                      <div className="p-3 bg-slate-50 rounded-lg border border-slate-100 space-y-1">
                        <div className="text-xs text-slate-500 flex items-center gap-1">
                          <Calendar className="w-3.5 h-3.5" />
                          <span>Fecha de Radicación</span>
                        </div>
                        <div className="text-sm font-semibold text-slate-900">
                          {formatDate(ticketDetail.filingDateUtc)}
                        </div>
                      </div>
                    </div>

                    {/* Applicant Information */}
                    <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 space-y-3">
                      <div className="text-xs font-bold text-slate-900 uppercase tracking-wider flex items-center gap-1.5">
                        <User className="w-4 h-4 text-institutional-600" />
                        <span>Datos del Ciudadano Solicitante</span>
                      </div>

                      {ticketDetail.isAnonymous || !ticketDetail.applicant ? (
                        <div className="flex items-center space-x-2 text-xs text-slate-600 py-1">
                          <ShieldCheck className="w-4 h-4 text-emerald-600" />
                          <span>Solicitud radicada bajo modalidad <strong>Anónima</strong>. No se registran datos personales de contacto.</span>
                        </div>
                      ) : (
                        <div className="grid grid-cols-2 gap-3 text-xs">
                          <div>
                            <span className="text-slate-500 block">Nombre Completo:</span>
                            <span className="font-semibold text-slate-900">{ticketDetail.applicant.fullName}</span>
                          </div>
                          <div>
                            <span className="text-slate-500 block">Identificación:</span>
                            <span className="font-semibold text-slate-900">
                              {ticketDetail.applicant.documentType} {ticketDetail.applicant.documentNumber}
                            </span>
                          </div>
                          <div className="flex items-center space-x-1 text-slate-700">
                            <Mail className="w-3.5 h-3.5 text-slate-400" />
                            <span>{ticketDetail.applicant.email}</span>
                          </div>
                          {ticketDetail.applicant.phone && (
                            <div className="flex items-center space-x-1 text-slate-700">
                              <Phone className="w-3.5 h-3.5 text-slate-400" />
                              <span>{ticketDetail.applicant.phone}</span>
                            </div>
                          )}
                        </div>
                      )}
                    </div>

                    {/* Assignment Instructions Note */}
                    {ticketDetail.assignmentNote && (
                      <div className="p-4 bg-institutional-50/60 rounded-xl border border-institutional-200 space-y-1 text-xs">
                        <span className="font-bold text-institutional-900 block">Instrucción recibida del Administrador:</span>
                        <p className="text-slate-700 italic">{ticketDetail.assignmentNote}</p>
                      </div>
                    )}

                    {/* Citizen Statement */}
                    <div className="space-y-2">
                      <div className="text-sm font-semibold text-slate-900 flex items-center gap-1.5">
                        <FileText className="w-4 h-4 text-slate-500" />
                        <span>Descripción Completa de la Solicitud</span>
                      </div>
                      <div className="p-4 bg-slate-50 rounded-xl border border-slate-200 text-sm text-slate-800 leading-relaxed whitespace-pre-wrap font-sans max-h-60 overflow-y-auto">
                        {ticketDetail.description}
                      </div>
                    </div>

                    {/* Institutional Response (if already closed) */}
                    {ticketDetail.responseText && (
                      <div className="p-4 bg-emerald-50 rounded-xl border border-emerald-200 space-y-2 text-xs">
                        <div className="flex items-center justify-between">
                          <span className="font-bold text-emerald-900 flex items-center gap-1.5">
                            <CheckCircle2 className="w-4 h-4 text-emerald-600" />
                            <span>Respuesta Institucional Entregada</span>
                          </span>
                          <span className="text-[11px] text-emerald-700 font-medium">
                            {formatDate(ticketDetail.responseDateUtc || undefined)}
                          </span>
                        </div>
                        <p className="text-slate-800 text-sm whitespace-pre-wrap leading-relaxed">
                          {ticketDetail.responseText}
                        </p>
                      </div>
                    )}
                  </div>
                )}

                {/* TAB: EMIT RESPONSE */}
                {activeTab === 'response' && radicado && (
                  <TicketResponseForm
                    radicado={radicado}
                    onSuccess={handleResponseSuccess}
                  />
                )}

                {/* TAB: STATUS UPDATE */}
                {activeTab === 'status' && radicado && (
                  <TicketStatusForm
                    radicado={radicado}
                    currentStatus={ticketDetail.status}
                    onSuccess={handleStatusSuccess}
                  />
                )}

                {/* TAB: HISTORY */}
                {activeTab === 'history' && (
                  <TicketStatusHistoryList history={ticketDetail.statusHistory} />
                )}
              </>
            ) : null}
          </div>

          {/* Footer Action */}
          <div className="p-4 bg-slate-50 border-t border-slate-200 flex items-center justify-between">
            <div className="text-[11px] text-slate-500">
              {ticketDetail?.canManage && ticketDetail.status !== 5 ? (
                <span className="text-institutional-700 font-medium">Usted está autorizado para gestionar este radicado.</span>
              ) : ticketDetail?.status === 5 ? (
                <span className="text-emerald-700 font-medium">Radicado en estado cerrado.</span>
              ) : (
                <span className="text-slate-400">Modo de solo lectura.</span>
              )}
            </div>
            <button
              onClick={onClose}
              className="px-4 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-xl hover:bg-slate-100 transition-colors"
            >
              Cerrar
            </button>
          </div>

        </div>
      </div>
    </div>
  );
}
