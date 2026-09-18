import { Suspense } from 'react';
import { getTranslations } from 'next-intl/server';
import Link from 'next/link';
import { LoginForm } from './components/LoginForm';

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

      <div className="py-6">
        <Suspense fallback={<div className="text-center py-12 text-slate-500">Cargando formulario...</div>}>
          <LoginForm />
        </Suspense>
      </div>
    </div>
  );
}
