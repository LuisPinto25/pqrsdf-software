import Link from 'next/link';

export default function NotFound() {
  return (
    <main className="min-h-[60vh] flex flex-col items-center justify-center p-6 text-center">
      <div className="bg-white rounded-2xl border border-slate-200 p-8 max-w-lg w-full shadow-sm space-y-5">
        <span className="text-5xl font-black text-institutional-600">404</span>

        <h1 className="text-2xl font-bold text-slate-900">Página no encontrada</h1>

        <p className="text-sm text-slate-600">
          El recurso o trámite solicitado no existe, ha cambiado de ubicación o se encuentra
          temporalmente inactivo.
        </p>

        <div className="pt-2">
          <Link
            href="/"
            className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg text-sm font-semibold text-white bg-institutional-600 hover:bg-institutional-700 shadow-sm transition"
          >
            Volver al inicio
          </Link>
        </div>
      </div>
    </main>
  );
}
