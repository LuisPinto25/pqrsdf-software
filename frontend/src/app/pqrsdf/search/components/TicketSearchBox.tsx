'use client';

import React, { useState, useEffect } from 'react';
import { useTranslations } from 'next-intl';
import { Search, X, Loader2 } from 'lucide-react';

interface TicketSearchBoxProps {
  initialRadicado?: string;
  isLoading: boolean;
  onSearch: (radicado: string) => void;
  onClear: () => void;
}

export function TicketSearchBox({
  initialRadicado = '',
  isLoading,
  onSearch,
  onClear,
}: TicketSearchBoxProps) {
  const t = useTranslations('PqrsdfSearch.searchBox');
  const tErr = useTranslations('PqrsdfSearch.errors');
  const [radicado, setRadicado] = useState(initialRadicado);
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (initialRadicado) {
      setRadicado(initialRadicado);
    }
  }, [initialRadicado]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const cleanRadicado = radicado.trim();

    if (!cleanRadicado) {
      setValidationError(tErr('radicadoRequired'));
      return;
    }

    const radicadoRegex = /^\d{4}-\d{8}$/;
    if (!radicadoRegex.test(cleanRadicado)) {
      setValidationError(tErr('radicadoInvalid'));
      return;
    }

    setValidationError(null);
    onSearch(cleanRadicado);
  };

  const handleClear = () => {
    setRadicado('');
    setValidationError(null);
    onClear();
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setRadicado(e.target.value);
    if (validationError) {
      setValidationError(null);
    }
  };

  return (
    <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-4">
      <form onSubmit={handleSubmit} className="space-y-3">
        <label htmlFor="radicado-input" className="block text-sm font-semibold text-slate-800">
          {t('label')}
        </label>
        <div className="flex flex-col sm:flex-row gap-3">
          <div className="relative flex-1">
            <input
              id="radicado-input"
              type="text"
              value={radicado}
              onChange={handleChange}
              placeholder={t('placeholder')}
              disabled={isLoading}
              maxLength={15}
              className={`w-full px-4 py-3 rounded-lg border text-base font-mono uppercase transition focus:outline-none focus:ring-2 ${
                validationError
                  ? 'border-red-500 focus:ring-red-200 text-red-900 bg-red-50/20'
                  : 'border-slate-300 focus:ring-institutional-500/20 focus:border-institutional-600 text-slate-900'
              }`}
            />
            {radicado && !isLoading && (
              <button
                type="button"
                onClick={handleClear}
                aria-label={t('clear')}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-600 p-1 rounded-full transition"
              >
                <X className="w-4 h-4" />
              </button>
            )}
          </div>
          <button
            type="submit"
            disabled={isLoading || !radicado.trim()}
            className="inline-flex items-center justify-center px-6 py-3 rounded-lg font-medium text-white bg-institutional-600 hover:bg-institutional-700 disabled:opacity-50 disabled:cursor-not-allowed transition shadow-sm"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                {t('searching')}
              </>
            ) : (
              <>
                <Search className="w-4 h-4 mr-2" />
                {t('button')}
              </>
            )}
          </button>
        </div>
        {validationError ? (
          <p className="text-sm text-red-600 font-medium">{validationError}</p>
        ) : (
          <p className="text-xs text-slate-500">{t('hint')}</p>
        )}
      </form>
    </div>
  );
}
