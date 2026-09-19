'use client';

import React, { useState } from 'react';
import { Send, AlertCircle, CheckCircle2, ShieldAlert } from 'lucide-react';
import { respondTicket } from '@/shared/api/client';

interface TicketResponseFormProps {
  radicado: string;
  onSuccess: () => void;
}

export function TicketResponseForm({ radicado, onSuccess }: TicketResponseFormProps) {
  const [responseText, setResponseText] = useState<string>('');
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [showConfirmation, setShowConfirmation] = useState<boolean>(false);

  const trimmedLength = responseText.trim().length;
  const isTooShort = trimmedLength > 0 && trimmedLength < 10;
  const isTooLong = trimmedLength > 4000;
  const isValid = trimmedLength >= 10 && trimmedLength <= 4000;

  const handleSubmit = async () => {
    if (!isValid) return;

    setIsSubmitting(true);
    setErrorMessage(null);

    const result = await respondTicket(radicado, {
      responseText: responseText.trim(),
    });

    if (result.ok) {
      onSuccess();
    } else {
      setErrorMessage(result.error.message || 'Ocurrió un error al registrar la respuesta.');
      setIsSubmitting(false);
      setShowConfirmation(false);
    }
  };

  return (
    <div className="space-y-4">
      <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start gap-3">
        <ShieldAlert className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
        <div className="text-xs text-amber-900 leading-relaxed">
          <p className="font-semibold mb-0.5">Cierre Formal de Solicitud</p>
          Al enviar la respuesta definitiva al ciudadano, el caso pasará automáticamente a estado <strong>Cerrado</strong>, se detendrá el cómputo de días hábiles legales de SLA y se liberará un cupo de su capacidad de trabajo asignada.
        </div>
      </div>

      {errorMessage && (
        <div className="bg-red-50 border border-red-200 rounded-xl p-3.5 flex items-center gap-2 text-xs text-red-700">
          <AlertCircle className="w-4 h-4 text-red-500 shrink-0" />
          <span>{errorMessage}</span>
        </div>
      )}

      <div className="space-y-1.5">
        <div className="flex items-center justify-between">
          <label htmlFor="response-text" className="text-xs font-semibold text-slate-800">
            Texto de la Respuesta Institucional (10 a 4.000 caracteres)
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
            {trimmedLength} / 4000 caracteres
          </span>
        </div>

        <textarea
          id="response-text"
          rows={7}
          value={responseText}
          onChange={(e) => setResponseText(e.target.value)}
          disabled={isSubmitting}
          placeholder="Redacte de manera formal, clara y completa la respuesta al requerimiento del ciudadano..."
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
            La respuesta debe tener al menos 10 caracteres para cumplir los estándares de atención.
          </p>
        )}
        {isTooLong && (
          <p className="text-[11px] text-red-600 font-medium">
            La respuesta excede el límite máximo permitido de 4.000 caracteres.
          </p>
        )}
      </div>

      {showConfirmation ? (
        <div className="p-4 bg-slate-50 border border-slate-200 rounded-xl space-y-3">
          <div className="flex items-start gap-2.5">
            <CheckCircle2 className="w-5 h-5 text-institutional-600 shrink-0 mt-0.5" />
            <div>
              <h4 className="text-xs font-bold text-slate-900">¿Confirmar emisión de respuesta?</h4>
              <p className="text-xs text-slate-500 mt-0.5">
                Esta acción notificará formalmente al ciudadano y concluirá el trámite sin opción de reapertura directa.
              </p>
            </div>
          </div>
          <div className="flex items-center justify-end space-x-2 pt-1">
            <button
              type="button"
              disabled={isSubmitting}
              onClick={() => setShowConfirmation(false)}
              className="px-3 py-1.5 text-xs font-medium text-slate-600 hover:bg-slate-200/70 rounded-lg transition-colors"
            >
              Cancelar
            </button>
            <button
              type="button"
              disabled={isSubmitting || !isValid}
              onClick={handleSubmit}
              className="inline-flex items-center space-x-1.5 px-4 py-1.5 text-xs font-semibold text-white bg-institutional-600 hover:bg-institutional-700 rounded-lg transition-colors shadow-xs"
            >
              <Send className="w-3.5 h-3.5" />
              <span>{isSubmitting ? 'Cerrando radicado...' : 'Confirmar y Cerrar'}</span>
            </button>
          </div>
        </div>
      ) : (
        <div className="flex justify-end pt-2">
          <button
            type="button"
            disabled={!isValid || isSubmitting}
            onClick={() => setShowConfirmation(true)}
            className="inline-flex items-center space-x-2 px-5 py-2.5 text-xs font-semibold text-white bg-institutional-600 hover:bg-institutional-700 disabled:opacity-50 disabled:cursor-not-allowed rounded-xl transition-all shadow-xs"
          >
            <Send className="w-4 h-4" />
            <span>Enviar Respuesta y Cerrar Radicado</span>
          </button>
        </div>
      )}
    </div>
  );
}
