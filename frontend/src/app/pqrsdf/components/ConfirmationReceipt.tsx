'use client';

import React, { useState, useCallback } from 'react';
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import {
  CheckCircle2,
  Copy,
  Check,
  Calendar,
  Building2,
  Clock,
  UserCheck,
  UserX,
  FileText,
  RotateCcw,
  Home,
  Search,
} from 'lucide-react';
import type { MakePqrsdfResponse } from '../types/pqrsdf';

export interface ConfirmationReceiptProps {
  data: MakePqrsdfResponse;
  onReset?: () => void;
}

export const ConfirmationReceipt: React.FC<ConfirmationReceiptProps> = ({ data, onReset }) => {
  const t = useTranslations('Pqrsdf');
  const [copied, setCopied] = useState(false);

  const handleCopy = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(data.radicadoNumber);
      setCopied(true);
      setTimeout(() => setCopied(false), 2500);
    } catch {
      // Fallback
    }
  }, [data.radicadoNumber]);

  // Format dates in Spanish locale
  const formatDate = (isoString: string) => {
    try {
      const date = new Date(isoString);
      return new Intl.DateTimeFormat('es-CO', {
        dateStyle: 'long',
        timeStyle: 'short',
        timeZone: 'America/Bogota',
      }).format(date);
    } catch {
      return isoString;
    }
  };

  const formatDueDate = (isoString: string) => {
    try {
      const date = new Date(isoString);
      return new Intl.DateTimeFormat('es-CO', {
        dateStyle: 'full',
        timeZone: 'America/Bogota',
      }).format(date);
    } catch {
      return isoString;
    }
  };

  return (
    <div
      role="region"
      aria-label={t('confirmation.title')}
      className="bg-white border border-gray-200 rounded-xl shadow-sm p-6 sm:p-10 space-y-8 animate-fadeIn"
    >
      {/* Encabezado de Confirmación */}
      <div className="text-center space-y-3 pb-6 border-b border-gray-100">
        <div className="inline-flex items-center justify-center w-16 h-16 bg-emerald-100 rounded-full mb-2">
          <CheckCircle2 className="w-10 h-10 text-emerald-600" />
        </div>
        <h2 className="text-2xl sm:text-3xl font-bold text-gray-900">{t('confirmation.title')}</h2>
        <p className="text-gray-600 max-w-xl mx-auto text-sm sm:text-base">
          {t('confirmation.subtitle')}
        </p>
      </div>

      {/* Tarjeta Destacada de Número de Radicado */}
      <div className="bg-slate-50 border-2 border-blue-500 rounded-xl p-6 text-center space-y-3">
        <p className="text-xs sm:text-sm font-semibold text-blue-900 tracking-wider uppercase">
          {t('confirmation.radicadoLabel')}
        </p>
        <div className="flex flex-col sm:flex-row items-center justify-center gap-3">
          <span className="font-mono text-3xl sm:text-4xl font-extrabold text-blue-700 tracking-wider select-all">
            {data.radicadoNumber}
          </span>
          <button
            type="button"
            onClick={handleCopy}
            className={`inline-flex items-center space-x-1.5 px-4 py-2 rounded-lg text-sm font-medium transition cursor-pointer ${
              copied
                ? 'bg-emerald-600 text-white shadow-xs'
                : 'bg-blue-600 hover:bg-blue-700 text-white shadow-xs'
            }`}
            aria-live="polite"
          >
            {copied ? (
              <>
                <Check className="w-4 h-4" />
                <span>{t('confirmation.copied')}</span>
              </>
            ) : (
              <>
                <Copy className="w-4 h-4" />
                <span>{t('confirmation.copyRadicado')}</span>
              </>
            )}
          </button>
        </div>
        <p className="text-xs text-gray-500 max-w-md mx-auto">
          {t('confirmation.importantNotice')}
        </p>
      </div>

      {/* Desglose de Datos del Trámite */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Tipo de Trámite */}
        <div className="flex items-start space-x-3 p-4 bg-gray-50 rounded-lg">
          <FileText className="w-5 h-5 text-gray-500 mt-0.5 flex-shrink-0" />
          <div>
            <span className="block text-xs font-semibold text-gray-500 uppercase">
              {t('form.requestType')}
            </span>
            <span className="text-sm font-medium text-gray-900">{t(`types.${data.type}`)}</span>
          </div>
        </div>

        {/* Dependencia Destino (si está disponible) */}
        {data.destinationAreaName && (
          <div className="flex items-start space-x-3 p-4 bg-gray-50 rounded-lg">
            <Building2 className="w-5 h-5 text-gray-500 mt-0.5 flex-shrink-0" />
            <div>
              <span className="block text-xs font-semibold text-gray-500 uppercase">
                {t('form.destinationArea')}
              </span>
              <span className="text-sm font-medium text-gray-900">{data.destinationAreaName}</span>
            </div>
          </div>
        )}

        {/* Fecha y Hora de Radicación */}
        <div className="flex items-start space-x-3 p-4 bg-gray-50 rounded-lg">
          <Clock className="w-5 h-5 text-gray-500 mt-0.5 flex-shrink-0" />
          <div>
            <span className="block text-xs font-semibold text-gray-500 uppercase">
              {t('confirmation.createdAtLabel')}
            </span>
            <span className="text-sm font-medium text-gray-900">
              {formatDate(
                data.createdAtUtc || (data as unknown as { createdAt?: string }).createdAt || ''
              )}
            </span>
          </div>
        </div>

        {/* Tipo de Presentación (Identificado o Anónimo) */}
        <div className="flex items-start space-x-3 p-4 bg-gray-50 rounded-lg">
          {data.isAnonymous ? (
            <UserX className="w-5 h-5 text-purple-500 mt-0.5 flex-shrink-0" />
          ) : (
            <UserCheck className="w-5 h-5 text-blue-500 mt-0.5 flex-shrink-0" />
          )}
          <div>
            <span className="block text-xs font-semibold text-gray-500 uppercase">
              Modalidad de Radicación
            </span>
            <span className="text-sm font-medium text-gray-900">
              {data.isAnonymous
                ? 'Anónimo (Sin datos de contacto)'
                : 'Identificado con datos de contacto'}
            </span>
          </div>
        </div>

        {/* Plazo Legal de Respuesta */}
        <div className="md:col-span-2 flex items-start space-x-3 p-4 bg-amber-50 border border-amber-200 rounded-lg">
          <Calendar className="w-5 h-5 text-amber-600 mt-0.5 flex-shrink-0" />
          <div className="space-y-1">
            <span className="block text-xs font-semibold text-amber-900 uppercase">
              {t('confirmation.dueDateLabel')}
            </span>
            <span className="block text-base font-bold text-amber-900 capitalize">
              {formatDueDate(data.dueDate)}
            </span>
            <span className="block text-xs text-amber-800">
              {t('confirmation.businessDaysInfo', {
                days:
                  data.businessDaysCount ||
                  (data as unknown as { termDays?: number }).termDays ||
                  15,
              })}
            </span>
          </div>
        </div>
      </div>

      {/* Botones de Acción */}
      <div className="pt-4 border-t border-gray-200 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex flex-col sm:flex-row items-center gap-3 w-full sm:w-auto">
          <Link
            href={`/pqrsdf/search?radicado=${encodeURIComponent(data.radicadoNumber)}`}
            className="w-full sm:w-auto inline-flex items-center justify-center space-x-2 px-6 py-2.5 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-lg transition shadow-xs cursor-pointer text-sm"
          >
            <Search className="w-4 h-4" />
            <span>{t('confirmation.trackTicket')}</span>
          </Link>

          {onReset && (
            <button
              type="button"
              onClick={onReset}
              className="w-full sm:w-auto inline-flex items-center justify-center space-x-2 px-6 py-2.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-medium rounded-lg transition text-sm cursor-pointer"
            >
              <RotateCcw className="w-4 h-4" />
              <span>{t('confirmation.fileAnother')}</span>
            </button>
          )}
        </div>

        <Link
          href="/"
          className="w-full sm:w-auto inline-flex items-center justify-center space-x-2 px-6 py-2.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg transition text-sm"
        >
          <Home className="w-4 h-4" />
          <span>{t('confirmation.returnHome')}</span>
        </Link>
      </div>
    </div>
  );
};
