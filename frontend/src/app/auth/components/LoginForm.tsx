'use client';

import React, { useState } from 'react';
import { useTranslations } from 'next-intl';
import { useSearchParams } from 'next/navigation';
import { useAuth } from '../hooks/useAuth';
import { LogIn, AlertCircle, CheckCircle2, Lock, Mail, Loader2 } from 'lucide-react';

export function LoginForm() {
  const t = useTranslations('Auth');
  const searchParams = useSearchParams();
  const isExpired = searchParams.get('expired') === 'true';
  const isLoggedOut = searchParams.get('loggedOut') === 'true';

  const { login, isLoading } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<{ email?: string; password?: string }>({});

  const validate = (): boolean => {
    const errors: { email?: string; password?: string } = {};

    if (!email.trim()) {
      errors.email = t('errors.emailRequired');
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      errors.email = t('errors.emailInvalid');
    }

    if (!password.trim()) {
      errors.password = t('errors.passwordRequired');
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMessage(null);

    if (!validate()) {
      return;
    }

    const returnUrl = searchParams.get('returnUrl') || '/dashboard';
    const result = await login(
      {
        email: email.trim(),
        password,
      },
      returnUrl
    );

    if (!result.isSuccess) {
      setErrorMessage(result.error || t('errors.generalError'));
    }
  };

  return (
    <div className="w-full max-w-md mx-auto">
      <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-8 space-y-6">
        <div className="text-center space-y-2">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-institutional-50 text-institutional-700 mb-2">
            <Lock className="w-6 h-6" />
          </div>
          <h1 className="text-2xl font-bold text-slate-900">{t('title')}</h1>
          <p className="text-sm text-slate-600">{t('subtitle')}</p>
        </div>

        {/* Notices */}
        {isExpired && (
          <div className="flex items-start space-x-3 p-4 bg-amber-50 border border-amber-200 rounded-xl text-amber-800 text-sm">
            <AlertCircle className="w-5 h-5 flex-shrink-0 mt-0.5 text-amber-600" />
            <div>
              <p className="font-medium">{t('sessionExpired')}</p>
            </div>
          </div>
        )}

        {isLoggedOut && (
          <div className="flex items-start space-x-3 p-4 bg-emerald-50 border border-emerald-200 rounded-xl text-emerald-800 text-sm">
            <CheckCircle2 className="w-5 h-5 flex-shrink-0 mt-0.5 text-emerald-600" />
            <div>
              <p className="font-medium">{t('logoutSuccess')}</p>
            </div>
          </div>
        )}

        {errorMessage && (
          <div className="flex items-start space-x-3 p-4 bg-red-50 border border-red-200 rounded-xl text-red-800 text-sm">
            <AlertCircle className="w-5 h-5 flex-shrink-0 mt-0.5 text-red-600" />
            <div>
              <p className="font-medium">{errorMessage}</p>
            </div>
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="space-y-4" noValidate>
          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700" htmlFor="email">
              {t('emailLabel')}
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                <Mail className="w-4 h-4" />
              </div>
              <input
                id="email"
                type="email"
                autoComplete="email"
                value={email}
                onChange={(e) => {
                  setEmail(e.target.value);
                  if (validationErrors.email) {
                    setValidationErrors((prev) => ({ ...prev, email: undefined }));
                  }
                }}
                placeholder={t('emailPlaceholder')}
                disabled={isLoading}
                className={`w-full pl-10 pr-4 py-2.5 rounded-xl border text-sm transition-all focus:outline-none focus:ring-2 ${
                  validationErrors.email
                    ? 'border-red-300 focus:border-red-500 focus:ring-red-200 bg-red-50/20'
                    : 'border-slate-300 focus:border-institutional-500 focus:ring-institutional-100 bg-white'
                }`}
              />
            </div>
            {validationErrors.email && (
              <p className="text-xs text-red-600 mt-1">{validationErrors.email}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <label className="block text-sm font-semibold text-slate-700" htmlFor="password">
              {t('passwordLabel')}
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none text-slate-400">
                <Lock className="w-4 h-4" />
              </div>
              <input
                id="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(e) => {
                  setPassword(e.target.value);
                  if (validationErrors.password) {
                    setValidationErrors((prev) => ({ ...prev, password: undefined }));
                  }
                }}
                placeholder={t('passwordPlaceholder')}
                disabled={isLoading}
                className={`w-full pl-10 pr-4 py-2.5 rounded-xl border text-sm transition-all focus:outline-none focus:ring-2 ${
                  validationErrors.password
                    ? 'border-red-300 focus:border-red-500 focus:ring-red-200 bg-red-50/20'
                    : 'border-slate-300 focus:border-institutional-500 focus:ring-institutional-100 bg-white'
                }`}
              />
            </div>
            {validationErrors.password && (
              <p className="text-xs text-red-600 mt-1">{validationErrors.password}</p>
            )}
          </div>

          <button
            type="submit"
            disabled={isLoading}
            className="w-full flex items-center justify-center space-x-2 py-3 px-4 bg-institutional-700 hover:bg-institutional-800 disabled:bg-slate-300 text-white font-semibold rounded-xl shadow-sm transition-all focus:outline-none focus:ring-2 focus:ring-institutional-500/20 disabled:cursor-not-allowed text-sm mt-2"
          >
            {isLoading ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>{t('submitting')}</span>
              </>
            ) : (
              <>
                <LogIn className="w-4 h-4" />
                <span>{t('submitButton')}</span>
              </>
            )}
          </button>
        </form>

        <div className="pt-2 text-center text-xs text-slate-500">
          <p>Cuentas preconfiguradas para pruebas:</p>
          <p className="font-mono text-slate-600 mt-1">funcionario@pqrsdf.gov.co | admin@pqrsdf.gov.co</p>
        </div>
      </div>
    </div>
  );
}
