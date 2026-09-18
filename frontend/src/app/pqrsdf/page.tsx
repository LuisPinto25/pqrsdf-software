import { getTranslations } from 'next-intl/server';
import Link from 'next/link';
import { PqrsdfContainer } from './components/PqrsdfContainer';

export default async function PqrsdfPage() {
  const t = await getTranslations('Pqrsdf');
  const tNav = await getTranslations('Navigation');

  return (
    <div className="space-y-6">
      <div className="flex items-center space-x-2 text-sm text-slate-500">
        <Link href="/" className="hover:text-institutional-700">
          {tNav('home')}
        </Link>
        <span>/</span>
        <span className="text-slate-800 font-medium">{t('title')}</span>
      </div>

      <div className="bg-white rounded-xl border border-slate-200 p-6 sm:p-8 shadow-sm space-y-2">
        <h1 className="text-2xl sm:text-3xl font-bold text-slate-900">{t('title')}</h1>
        <p className="text-slate-600 text-sm sm:text-base">{t('description')}</p>
      </div>

      <PqrsdfContainer />
    </div>
  );
}
