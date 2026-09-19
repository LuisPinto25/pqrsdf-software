'use client';

import React, { useState } from 'react';
import { RefreshCw, AlertCircle, Save } from 'lucide-react';
import { changeTicketStatus } from '@/shared/api/client';

interface TicketStatusFormProps {
  radicado: string;
  currentStatus: number;
  onSuccess: () => void;
}

export function TicketStatusForm({ radicado, currentStatus, onSuccess }: TicketStatusFormProps) {
  const [justification, setJustification] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const trimmedLength = justification.trim().length;
  const isTooShort = trimmedLength > 0 && trimmedLength < 10;
  const isTooLong = trimmedLength > 500;
  const isValid = trimmedLength >= 10 && trimmedLength <= 500;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isValid) return;

    setIsSubmitting(true);
    setErrorMessage(null);

    const result = await changeTicketStatus(radicado, {
      newStatus: 3, // InReview (3)
      justification: justification.trim(),
    });

    if (result.ok) {
      setJustification('');
      onSuccess();
    } else {
      setErrorMessage(result.error.message || 'Ocurrió un error al actualizar el estado.');
    }
    setIsSubmitting(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="bg-slate-50 border border-slate-200 rounded-xl p-4 text-xs text-slate-700 leading-relaxed space-y-1">
        <p className="font-semibold text-slate-900">Actualización Operativa de Estado</p>
        <p className="text-slate-500">
          Permite registrar hitos de avance o requerimientos de concepto interno manteniendo la solicitud en trámite. La justificación quedará registrada de forma inmutable en el historial de auditoría interna.
        </p>
      </div>

      {errorMessage && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-3.5 flex items-center gap-2 text-xs text-red-700">
          <AlertCircle className="w-4 h-4 text-red-500 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <div className="space-y-1">
        <label className="text-xs font-semibold text-slate-800">
          Nuevo Estado Operativo {currentStatus === 3 ? '(Mantener En trámite)' : '(Avanzar a En trámite)'}
        </label>
        <div className="p-2.5 bg-slate-100 rounded-xl text-xs font-medium text-slate-700 border border-slate-200 flex items-center justify-between">
          <span>En trámite (InReview)</span>
          <span className="text-[11px] text-slate-500">
            {currentStatus === 3 ? 'Continuar trámite' : 'Iniciar trámite formal'}
          </span>
        </div>
      </div>

      <div className="space-y-1.5">
        <div className="flex items-center justify-between">
          <label htmlFor="justification-text" className="text-xs font-semibold text-slate-800">
            Justificación Obligatoria del Avance (10 a 500 caracteres)
          </label>
          <span
            className={`text-xs font-mono font-medium ${
              isTooShort
                ? 'text-amber-600 font-bold'
                : isTooLong
                ? 'text-red-600 font-bold'
                : 'text-slate-500'
            }`}
          >
            {trimmedLength} / 500 caracteres
          </span>
        </div>

        <textarea
          id="justification-text"
          rows={4}
          value={justification}
          onChange={(e) => setJustification(e.target.value)}
          disabled={isSubmitting}
          placeholder="Describa la razón operativa, solicitud de concepto técnico u observación de avance..."
          className={`w-full text-sm rounded-xl border p-3.5 outline-hidden transition-all resize-y ${
            isTooShort
              ? 'border-amber-300 focus:border-amber-500 focus:ring-2 focus:ring-amber-200'
              : isTooLong
              ? 'border-red-300 focus:border-red-500 focus:ring-2 focus:ring-red-200'
              : 'border-slate-300 focus:border-institutional-500 focus:ring-2 focus:ring-institutional-200'
          }`}
        />

        {isTooShort && (
          <p className="text-[11px] text-amber-600 font-medium">
            Debe ingresar una justificación de al menos 10 caracteres.
          </p>
        )}
        {isTooLong && (
          <p className="text-[11px] text-red-600 font-medium">
            La justificación no puede exceder los 500 caracteres.
          </p>
        )}
      </div>

      <div className="flex justify-end pt-2">
        <button
          type="submit"
          disabled={!isValid || isSubmitting}
          className="inline-flex items-center space-x-2 px-5 py-2.5 text-xs font-semibold text-white bg-institutional-600 hover:bg-institutional-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-xl transition-all shadow-xs"
        >
          {isSubmitting ? (
            <>
              <RefreshCw className="w-3.5 h-3.5 animate-spin" />
              <span>Guardando...</span>
            </>
          ) : (
            <>
              <Save className="w-3.5 h-3.5" />
              <span>Guardar Actualización</span>
            </>
          )}
        </button>
      </div>
    </form>
  );
}
