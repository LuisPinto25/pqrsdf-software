'use client';

import React, { useState, useEffect } from 'react';
import { 
  X, 
  UserCheck, 
  AlertCircle, 
  RefreshCw, 
  CheckCircle2
} from 'lucide-react';
import { 
  UnassignedTicketDto, 
  AssignableOfficialDto 
} from '../types/assignment.types';
import { WorkloadBadge } from '@/shared/components/WorkloadBadge';
import { getAssignableOfficials, assignTicket } from '@/shared/api/client';

interface AssignOfficialModalProps {
  isOpen: boolean;
  ticket: UnassignedTicketDto | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function AssignOfficialModal({
  isOpen,
  ticket,
  onClose,
  onSuccess,
}: AssignOfficialModalProps) {
  const [officials, setOfficials] = useState<AssignableOfficialDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [selectedOfficialId, setSelectedOfficialId] = useState<string>('');
  const [note, setNote] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setSelectedOfficialId('');
      setNote('');
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedOfficialId) {
      setErrorMessage('Debe seleccionar un funcionario de la lista.');
      return;
    }

    setIsSubmitting(true);
    setErrorMessage(null);

    const result = await assignTicket(ticket.radicadoNumber, {
      officialId: selectedOfficialId,
      note: note.trim() || null,
    });

    setIsSubmitting(false);

    if (result.ok) {
      onSuccess();
      onClose();
    } else {
      setErrorMessage(result.error.message || 'Error al asignar la solicitud.');
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

        {/* Center alignment trick */}
        <span className="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>

        {/* Modal Dialog */}
        <div className="inline-block align-bottom bg-white rounded-2xl text-left overflow-hidden shadow-2xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full border border-slate-200">
          
          {/* Header */}
          <div className="px-6 py-5 bg-slate-50 border-b border-slate-200 flex items-center justify-between">
            <div className="flex items-center space-x-2.5">
              <div className="w-9 h-9 rounded-xl bg-institutional-100 text-institutional-700 flex items-center justify-center">
                <UserCheck className="w-5 h-5" />
              </div>
              <div>
                <h3 className="text-base font-bold text-slate-900" id="modal-title">
                  Asignar Solicitud PQRSDF
                </h3>
                <p className="text-xs text-slate-500 font-mono">
                  Radicado: <span className="font-bold text-institutional-700">{ticket.radicadoNumber}</span>
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
            {/* Body */}
            <div className="p-6 space-y-5">
              {errorMessage && (
                <div className="p-3 bg-red-50 border border-red-200 rounded-xl text-xs text-red-700 flex items-start space-x-2">
                  <AlertCircle className="w-4 h-4 text-red-500 shrink-0 mt-0.5" />
                  <span>{errorMessage}</span>
                </div>
              )}

              {/* Roster of Officials */}
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-bold text-slate-700 uppercase tracking-wider">
                    Funcionario Responsable (Capacidad Máx: 5)
                  </label>
                  <span className="text-xs text-slate-400">
                    {officials.filter((o) => o.canAssign).length} disponibles
                  </span>
                </div>

                {isLoading ? (
                  <div className="py-8 text-center space-y-2">
                    <RefreshCw className="w-6 h-6 text-institutional-600 animate-spin mx-auto" />
                    <p className="text-xs text-slate-500">Cargando funcionarios activos...</p>
                  </div>
                ) : officials.length === 0 ? (
                  <div className="p-4 bg-amber-50 border border-amber-200 rounded-xl text-xs text-amber-800">
                    No se encontraron funcionarios activos registrados en el sistema.
                  </div>
                ) : (
                  <div className="max-h-56 overflow-y-auto space-y-2 border border-slate-200 rounded-xl p-2 bg-slate-50/50 divide-y divide-slate-100">
                    {officials.map((official) => {
                      const isDisabled = !official.canAssign;
                      const isSelected = selectedOfficialId === official.id;

                      return (
                        <label
                          key={official.id}
                          className={`flex items-center justify-between p-3 rounded-lg transition-colors cursor-pointer ${
                            isDisabled
                              ? 'opacity-50 cursor-not-allowed bg-slate-100/60'
                              : isSelected
                              ? 'bg-institutional-50 border border-institutional-300'
                              : 'hover:bg-white bg-white/60 border border-transparent'
                          }`}
                        >
                          <div className="flex items-center space-x-3">
                            <input
                              type="radio"
                              name="assignedOfficial"
                              value={official.id}
                              checked={isSelected}
                              disabled={isDisabled}
                              onChange={() => setSelectedOfficialId(official.id)}
                              className="w-4 h-4 text-institutional-600 focus:ring-institutional-500 border-slate-300 disabled:opacity-40"
                            />
                            <div>
                              <div className="text-sm font-semibold text-slate-900 flex items-center gap-1.5">
                                <span>{official.fullName}</span>
                                {isSelected && (
                                  <CheckCircle2 className="w-3.5 h-3.5 text-institutional-600" />
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

              {/* Optional Administrative Note */}
              <div className="space-y-1.5">
                <div className="flex items-center justify-between">
                  <label htmlFor="assign-note" className="text-xs font-semibold text-slate-700">
                    Instrucciones de Trabajo (Opcional)
                  </label>
                  <span className="text-xs text-slate-400">
                    {note.length}/500
                  </span>
                </div>
                <textarea
                  id="assign-note"
                  rows={3}
                  maxLength={500}
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  placeholder="Escriba antecedentes, prioridades o indicaciones particulares para el funcionario..."
                  className="w-full text-xs p-3 rounded-xl border border-slate-300 focus:outline-hidden focus:ring-2 focus:ring-institutional-500 bg-white"
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
                disabled={isSubmitting || !selectedOfficialId || isLoading}
                className="inline-flex items-center space-x-1.5 px-5 py-2 text-xs font-semibold text-white bg-institutional-600 hover:bg-institutional-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-xl shadow-xs transition-colors"
              >
                {isSubmitting ? (
                  <>
                    <RefreshCw className="w-3.5 h-3.5 animate-spin" />
                    <span>Asignando...</span>
                  </>
                ) : (
                  <>
                    <UserCheck className="w-3.5 h-3.5" />
                    <span>Confirmar Asignación</span>
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
