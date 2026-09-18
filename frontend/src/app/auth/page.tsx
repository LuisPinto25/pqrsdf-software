import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

export default async function AuthPage() {
  const t = await getTranslations('Auth');
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

      <div className="bg-white rounded-xl border border-slate-200 p-8 shadow-sm space-y-4">
        <h1 className="text-2xl font-bold text-slate-900">{t('title')}</h1>
        <p className="text-slate-600">{t('description')}</p>
        <div className="p-4 bg-institutional-50 border border-institutional-200 rounded-lg text-institutional-900 text-sm">
          {t('placeholder')}
        </div>
      </div>
    </div>
  );
}
