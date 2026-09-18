'use client';

import { useEffect } from 'react';

export default function GlobalError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    // Log unexpected runtime error safely without exposing technical stack to user
    console.error('Unhandled runtime error intercepted by ErrorBoundary:', error);
  }, [error]);

  return (
    <div className="min-h-[60vh] flex flex-col items-center justify-center p-6 text-center">
      <div className="bg-white rounded-2xl border border-red-200 p-8 max-w-lg w-full shadow-sm space-y-5">
        <div className="mx-auto w-12 h-12 rounded-full bg-red-100 flex items-center justify-center text-red-600">
          <svg
            className="w-6 h-6"
            fill="none"
            viewBox="0 0 24 24"
            stroke="currentColor"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth="2"
              d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
            />
          </svg>
        </div>

        <h1 className="text-2xl font-bold text-slate-900">Ha ocurrido un error inesperado</h1>

        <p className="text-sm text-slate-600">
          Se ha presentado una dificultad técnica temporal al procesar su solicitud. Por favor
          intente nuevamente.
        </p>

        <div className="pt-2">
          <button
            type="button"
            onClick={() => reset()}
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg text-sm font-semibold text-white bg-institutional-600 hover:bg-institutional-700 shadow-sm transition focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-institutional-500"
          >
            Reintentar
          </button>
        </div>
      </div>
    </div>
  );
}
