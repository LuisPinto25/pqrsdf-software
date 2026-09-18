import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

export default async function HomePage() {
  const t = await getTranslations('Common');
  const tNav = await getTranslations('Navigation');

  return (
    <main className="space-y-8">
      <section className="bg-white rounded-xl shadow-sm border border-slate-200 p-8 text-center space-y-4">
        <h1 className="text-3xl font-extrabold text-slate-900 tracking-tight sm:text-4xl">
          {t('title')}
        </h1>
        <p className="max-w-2xl mx-auto text-base text-slate-600 sm:text-lg">{t('subtitle')}</p>
        <p className="text-sm text-institutional-700 font-medium">{t('welcome')}</p>
      </section>

      <section className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm hover:shadow-md transition flex flex-col justify-between">
          <div>
            <h2 className="text-xl font-bold text-slate-900 mb-2">{tNav('pqrsdf')}</h2>
            <p className="text-sm text-slate-600 mb-4">
              Radique formalmente peticiones, quejas, reclamos, sugerencias, denuncias o felicitaciones ante la entidad.
            </p>
          </div>
          <Link
            href="/pqrsdf"
            className="inline-flex items-center text-sm font-semibold text-institutional-600 hover:text-institutional-800"
          >
            Radicar solicitud →
          </Link>
        </div>

        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm hover:shadow-md transition flex flex-col justify-between">
          <div>
            <h2 className="text-xl font-bold text-slate-900 mb-2">{tNav('search')}</h2>
            <p className="text-sm text-slate-600 mb-4">
              Consulte en tiempo real el estado, línea de tiempo de avance y respuesta oficial con su número de radicado.
            </p>
          </div>
          <Link
            href="/pqrsdf/search"
            className="inline-flex items-center text-sm font-semibold text-institutional-600 hover:text-institutional-800"
          >
            Consultar radicado →
          </Link>
        </div>

        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm hover:shadow-md transition flex flex-col justify-between">
          <div>
            <h2 className="text-xl font-bold text-slate-900 mb-2">{tNav('auth')}</h2>
            <p className="text-sm text-slate-600 mb-4">
              Inicio de sesión seguro para ciudadanos registrados y funcionarios encargados de la gestión institucional.
            </p>
          </div>
          <Link
            href="/auth"
            className="inline-flex items-center text-sm font-semibold text-institutional-600 hover:text-institutional-800"
          >
            Iniciar sesión →
          </Link>
        </div>

        <div className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm hover:shadow-md transition flex flex-col justify-between">
          <div>
            <h2 className="text-xl font-bold text-slate-900 mb-2">{tNav('dashboard')}</h2>
            <p className="text-sm text-slate-600 mb-4">
              Monitoreo y métricas de tiempos de respuesta, solicitudes radicadas y niveles de satisfacción.
            </p>
          </div>
          <Link
            href="/dashboard"
            className="inline-flex items-center text-sm font-semibold text-institutional-600 hover:text-institutional-800"
          >
            Ver métricas →
          </Link>
        </div>
      </section>
    </main>
  );
}
