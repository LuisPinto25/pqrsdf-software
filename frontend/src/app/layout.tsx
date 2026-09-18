import type { Metadata } from 'next';
import { NextIntlClientProvider } from 'next-intl';
import { getLocale, getMessages } from 'next-intl/server';
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
          <header className="bg-institutional-800 text-white shadow-md">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <span className="text-xl font-bold tracking-tight">Portal PQRSDF</span>
              </div>
              <nav className="flex space-x-6">
                <a
                  href="/"
                  className="text-sm font-medium text-slate-200 hover:text-white transition"
                >
                  Inicio
                </a>
                <a
                  href="/pqrsdf"
                  className="text-sm font-medium text-slate-200 hover:text-white transition"
                >
                  PQRSDF
                </a>
                <a
                  href="/auth"
                  className="text-sm font-medium text-slate-200 hover:text-white transition"
                >
                  Autenticación
                </a>
                <a
                  href="/dashboard"
                  className="text-sm font-medium text-slate-200 hover:text-white transition"
                >
                  Panel de Control
                </a>
              </nav>
            </div>
          </header>
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
