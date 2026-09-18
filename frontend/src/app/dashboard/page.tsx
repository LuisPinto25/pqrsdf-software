import { getTranslations } from 'next-intl/server';
import Link from 'next/link';
import { DashboardContent } from './components/DashboardContent';

export default async function DashboardPage() {
  const t = await getTranslations('Dashboard');
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

      <DashboardContent />
    </div>
  );
}

