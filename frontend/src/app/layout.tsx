import type { Metadata } from 'next';
import { NextIntlClientProvider } from 'next-intl';
import { getLocale, getMessages } from 'next-intl/server';
import { Navigation } from '@/shared/components/Navigation';
import './globals.css';

export const metadata: Metadata = {
  title: 'Sistema PQRSDF',
  description:
    'Portal de Gestión de Peticiones, Quejas, Reclamos, Sugerencias, Denuncias y Felicitaciones',
};

export default async function RootLayout({ children }: { children: React.ReactNode }) {
  const locale = await getLocale();
  const messages = await getMessages();

  return (
    <html lang={locale}>
      <body className="min-h-screen bg-slate-50 text-slate-900 antialiased flex flex-col">
        <NextIntlClientProvider messages={messages}>
          <Navigation />
          <div className="flex-1 max-w-7xl w-full mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {children}
          </div>
          <footer className="bg-white border-t border-slate-200 py-6 text-center text-sm text-slate-500">
            <p>
              © {new Date().getFullYear()} Sistema Institucional PQRSDF. Todos los derechos
              reservados.
            </p>
          </footer>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
