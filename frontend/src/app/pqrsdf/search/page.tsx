import { Suspense } from 'react';
import { getTranslations } from 'next-intl/server';
import Link from 'next/link';
import { TicketSearchContainer } from './components/TicketSearchContainer';

export default async function TicketSearchPage() {
  const t = await getTranslations('PqrsdfSearch');
  const tNav = await getTranslations('Navigation');
  const tCommon = await getTranslations('Common');

  return (
    <div className="space-y-6">
      {/* Breadcrumbs */}
      <div className="flex items-center space-x-2 text-sm text-slate-500">
        <Link href="/" className="hover:text-institutional-700">
          {tNav('home')}
        </Link>
        <span>/</span>
        <Link href="/pqrsdf" className="hover:text-institutional-700">
          {tNav('pqrsdf')}
        </Link>
        <span>/</span>
        <span className="text-slate-800 font-medium">{t('title')}</span>
      </div>

      {/* Page Header */}
      <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-2">
        <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">{t('title')}</h1>
        <p className="text-slate-600 text-sm sm:text-base">{t('description')}</p>
      </div>

      {/* Main Container wrapped in Suspense for useSearchParams */}
      <Suspense
        fallback={
          <div className="bg-white rounded-xl border border-slate-200 p-8 text-center text-slate-500">
            {tCommon('loading')}
          </div>
        }
      >
        <TicketSearchContainer />
      </Suspense>
    </div>
  );
}
