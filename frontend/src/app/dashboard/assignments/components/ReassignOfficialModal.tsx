'use client';

import React, { useState, useEffect } from 'react';
import { 
  X, 
  ArrowRightLeft, 
  AlertCircle, 
  RefreshCw, 
  CheckCircle2
} from 'lucide-react';
import { 
  AssignedTicketDto, 
  AssignableOfficialDto 
} from '../types/assignment.types';
import { WorkloadBadge } from '@/shared/components/WorkloadBadge';
import { getAssignableOfficials, reassignTicket } from '@/shared/api/client';

interface ReassignOfficialModalProps {
  isOpen: boolean;
  ticket: AssignedTicketDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function ReassignOfficialModal({
  isOpen,
  ticket,
  onClose,
  onSuccess,
}: ReassignOfficialModalProps) {
  const [officials, setOfficials] = useState<AssignableOfficialDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [selectedOfficialId, setSelectedOfficialId] = useState<string>('');
  const [justification, setJustification] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSelectedOfficialId('');
      setJustification('');
      setErrorMessage(null);
      setIsLoading(true);

      getAssignableOfficials().then((result) => {
        if (result.ok) {
          setOfficials(result.value);
        } else {
          setErrorMessage(result.error.message || 'Error al consultar los funcionarios disponibles.');
        }
        setIsLoading(false);
      });
    }
  }, [isOpen]);

  if (!isOpen || !ticket) return null;

  const isJustificationValid = justification.trim().length >= 10 && justification.trim().length <= 500;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedOfficialId) {
      setErrorMessage('Debe seleccionar el nuevo funcionario receptor.');
      return;
    }

    if (!isJustificationValid) {
      setErrorMessage('La justificación es obligatoria y debe contener entre 10 y 500 caracteres.');
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    const result = await reassignTicket(ticket.radicadoNumber, {
      newOfficialId: selectedOfficialId,
      justification: justification.trim(),
    });

    setIsSubmitting(false);

    if (result.ok) {
      onSuccess();
      onClose();
    } else {
      setErrorMessage(result.error.message || 'Error al reasignar la solicitud.');
    }
  };

  return (
    <div className="fixed inset-0 z-60 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
        {/* Backdrop */}
        <div 
          className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs transition-opacity" 
          onClick={onClose}
          aria-hidden="true" 
        />

        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        {/* Modal Dialog */}
        <div className="inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full border border-slate-200">
          
          {/* Header */}
          <div className="px-6 py-5 bg-purple-50/50 border-b border-purple-100 flex items-center justify-between">
            <div className="flex items-center space-x-2.5">
              <div className="w-9 h-9 rounded-xl bg-purple-100 text-purple-700 flex items-center justify-center">
                <ArrowRightLeft className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-900" id="modal-title">
                  Reasignar Funcionario Responsable
                </h3>
                <p className="text-xs text-slate-500 font-mono">
                  Radicado: <span className="font-bold text-purple-700">{ticket.radicadoNumber}</span>
                </p>
              </div>
            </div>
            <button
              onClick={onClose}
              disabled={isSubmitting}
              className="rounded-lg p-1.5 text-slate-400 hover:text-slate-600 hover:bg-slate-200/60 transition-colors"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <form onSubmit={handleSubmit}>
            <div className="p-6 space-y-5">
              {/* Current Assignee banner */}
              <div className="p-3 bg-slate-100 rounded-xl border border-slate-200 flex items-center justify-between text-xs">
                <span className="text-slate-500 font-medium">Asignado actualmente a:</span>
                <span className="font-bold text-slate-900">{ticket.assignedOfficialName}</span>
              </div>

              {errorMessage && (
                <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 flex items-start space-x-2">
                  <AlertCircle className="w-4 h-4 text-red-500 shrink-0 mt-0.5" />
                  <span>{errorMessage}</span>
                </div>
              )}

              {/* Roster of Eligible Target Officials */}
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-bold text-slate-700 uppercase tracking-wider">
                    Nuevo Funcionario Receptor
                  </label>
                  <span className="text-xs text-slate-400">
                    {officials.filter((o) => o.canAssign && o.id !== ticket.assignedToUserId).length} disponibles
                  </span>
                </div>

                {isLoading ? (
                  <div className="py-8 text-center space-y-2">
                    <RefreshCw className="w-6 h-6 text-purple-600 animate-spin mx-auto" />
                    <p className="text-xs text-slate-500">Cargando funcionarios activos...</p>
                  </div>
                ) : (
                  <div className="max-h-52 overflow-y-auto space-y-2 border border-slate-200 rounded-xl p-2 bg-slate-50/50 divide-y divide-slate-100">
                    {officials.map((official) => {
                      const isCurrent = official.id === ticket.assignedToUserId;
                      const isAtCapacity = !official.canAssign;
                      const isDisabled = isCurrent || isAtCapacity;
                      const isSelected = selectedOfficialId === official.id;

                      return (
                        <label
                          key={official.id}
                          className={`flex items-center justify-between p-3 rounded-lg transition-colors cursor-pointer ${
                            isDisabled
                              ? 'opacity-40 cursor-not-allowed bg-slate-100/60'
                              : isSelected
                              ? 'bg-purple-50 border border-purple-300'
                              : 'hover:bg-white bg-white/60 border border-transparent'
                          }`}
                        >
                          <div className="flex items-center space-x-3">
                            <input
                              type="radio"
                              name="reassignedOfficial"
                              value={official.id}
                              checked={isSelected}
                              disabled={isDisabled}
                              onChange={() => setSelectedOfficialId(official.id)}
                              className="w-4 h-4 text-purple-600 focus:ring-purple-500 border-slate-300 disabled:opacity-40"
                            />
                            <div>
                              <div className="text-sm font-semibold text-slate-900 flex items-center gap-1.5">
                                <span>{official.fullName}</span>
                                {isCurrent && (
                                  <span className="text-[10px] px-1.5 py-0.5 rounded-sm bg-slate-200 text-slate-700 font-normal">
                                    Actual
                                  </span>
                                )}
                                {isSelected && (
                                  <CheckCircle2 className="w-3.5 h-3.5 text-purple-600" />
                                )}
                              </div>
                              <div className="text-xs text-slate-500 font-mono">
                                {official.email}
                              </div>
                            </div>
                          </div>

                          <WorkloadBadge
                            activeCount={official.activeTicketsCount}
                            maxCapacity={official.maxCapacity}
                          />
                        </label>
                      );
                    })}
                  </div>
                )}
              </div>

              {/* Mandatory Justification */}
              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <label htmlFor="reassign-justification" className="text-xs font-semibold text-slate-700">
                    Motivo Obligatorio del Traslado (mínimo 10 caracteres) <span className="text-red-500">*</span>
                  </label>
                  <span className={`text-xs ${justification.length < 10 ? 'text-amber-600 font-medium' : 'text-slate-400'}`}>
                    {justification.length}/500 {justification.length < 10 ? '(faltan ' + (10 - justification.length) + ')' : ''}
                  </span>
                </div>
                <textarea
                  id="reassign-justification"
                  rows={3}
                  required
                  minLength={10}
                  maxLength={500}
                  value={justification}
                  onChange={(e) => setJustification(e.target.value)}
                  placeholder="Exponga el motivo legal u operativo del traslado (quedará registrado inmutablemente en la auditoría)..."
                  className="w-full text-xs p-3 rounded-xl border border-slate-300 focus:outline-hidden focus:ring-2 focus:ring-purple-500 bg-white"
                />
              </div>
            </div>

            {/* Footer */}
            <div className="px-6 py-4 bg-slate-50 border-t border-slate-200 flex items-center justify-end space-x-3">
              <button
                type="button"
                onClick={onClose}
                disabled={isSubmitting}
                className="px-4 py-2 text-xs font-medium text-slate-700 bg-white border border-slate-300 rounded-xl hover:bg-slate-100 transition-colors"
              >
                Cancelar
              </button>
              <button
                type="submit"
                disabled={isSubmitting || !selectedOfficialId || !isJustificationValid || isLoading}
                className="inline-flex items-center space-x-1.5 px-5 py-2 text-xs font-semibold text-white bg-purple-600 hover:bg-purple-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-xl shadow-xs transition-colors"
              >
                {isSubmitting ? (
                  <>
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    <span>Reasignando...</span>
                  </>
                ) : (
                  <>
                    <ArrowRightLeft className="w-3.5 h-3.5" />
                    <span>Confirmar Reasignación</span>
                  </>
                )}
              </button>
            </div>
          </form>

        </div>
      </div>
    </div>
  );
}
