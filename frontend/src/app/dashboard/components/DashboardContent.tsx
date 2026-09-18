'use client';

import React from 'react';
import { useTranslations } from 'next-intl';
import Link from 'next/link';
import { useAuth } from '@/shared/auth/useAuth';
import { 
  ShieldCheck, 
  UserCheck, 
  FileText, 
  BarChart3, 
  Users, 
  Search, 
  LogOut 
} from 'lucide-react';

export function DashboardContent() {
  const t = useTranslations('Dashboard');
  const tNav = useTranslations('Navigation');
  const { user, logout } = useAuth();

  const isRoleAdmin = user?.role === 'Administrador';

  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div className="bg-white rounded-2xl border border-slate-200 p-8 shadow-sm">
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div className="space-y-2">
            <div className="flex items-center space-x-3">
              <span
                className={`inline-flex items-center space-x-1.5 px-3 py-1 rounded-full text-xs font-semibold ${
                  isRoleAdmin
                    ? 'bg-purple-50 text-purple-700 border border-purple-200'
                    : 'bg-institutional-50 text-institutional-700 border border-institutional-200'
                }`}
              >
                {isRoleAdmin ? (
                  <ShieldCheck className="w-3.5 h-3.5" />
                ) : (
                  <UserCheck className="w-3.5 h-3.5" />
                )}
                <span>
                  {user?.role ? t(`roles.${user.role}`) : 'Funcionario'}
                </span>
              </span>
              <span className="text-xs text-slate-400 font-mono">
                {user?.email || 'funcionario@pqrsdf.gov.co'}
              </span>
            </div>
            <h1 className="text-2xl font-bold text-slate-900">
              {user?.fullName ? `¡Hola, ${user.fullName}!` : t('welcomeTitle')}
            </h1>
            <p className="text-sm text-slate-600">{t('welcomeSubtitle')}</p>
          </div>

          <div className="flex items-center space-x-3">
            <button
              onClick={logout}
              className="inline-flex items-center space-x-2 px-4 py-2 text-sm font-medium text-red-700 bg-red-50 hover:bg-red-100 rounded-xl transition-colors border border-red-200"
            >
              <LogOut className="w-4 h-4" />
              <span>{tNav('logout')}</span>
            </button>
          </div>
        </div>
      </div>

      {/* Action Cards Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <Link
          href="/pqrsdf/search"
          className="group p-6 bg-white rounded-xl border border-slate-200 shadow-sm hover:shadow-md hover:border-institutional-300 transition-all flex flex-col justify-between"
        >
          <div className="space-y-3">
            <div className="w-10 h-10 rounded-xl bg-institutional-50 text-institutional-700 flex items-center justify-center group-hover:bg-institutional-600 group-hover:text-white transition-colors">
              <Search className="w-5 h-5" />
            </div>
            <h2 className="text-base font-bold text-slate-900">
              Consulta de Solicitudes
            </h2>
            <p className="text-xs text-slate-600">
              Búsqueda de radicados y revisión de líneas de tiempo de atención ciudadana.
            </p>
          </div>
          <span className="text-xs text-institutional-600 font-semibold mt-4 block">
            Acceder &rarr;
          </span>
        </Link>

        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-sm flex flex-col justify-between opacity-85">
          <div className="space-y-3">
            <div className="w-10 h-10 rounded-xl bg-slate-100 text-slate-600 flex items-center justify-center">
              <FileText className="w-5 h-5" />
            </div>
            <h2 className="text-base font-bold text-slate-900">
              {t('actions.manageTickets')}
            </h2>
            <p className="text-xs text-slate-600">
              Bandeja de gestión de solicitudes asignadas a su dependencia (próximamente).
            </p>
          </div>
          <span className="text-xs text-slate-400 font-medium mt-4 block">
            En desarrollo
          </span>
        </div>

        {isRoleAdmin && (
          <div className="p-6 bg-white rounded-xl border border-purple-200 bg-purple-50/20 shadow-sm flex flex-col justify-between">
            <div className="space-y-3">
              <div className="w-10 h-10 rounded-xl bg-purple-100 text-purple-700 flex items-center justify-center">
                <Users className="w-5 h-5" />
              </div>
              <h2 className="text-base font-bold text-purple-950">
                {t('actions.users')}
              </h2>
              <p className="text-xs text-purple-800">
                Administración de cuentas de funcionarios y permisos de acceso institucional.
              </p>
            </div>
            <span className="text-xs text-purple-600 font-medium mt-4 block">
              Módulo exclusivo Admin
            </span>
          </div>
        )}

        <div className="p-6 bg-white rounded-xl border border-slate-200 shadow-sm flex flex-col justify-between opacity-85">
          <div className="space-y-3">
            <div className="w-10 h-10 rounded-xl bg-slate-100 text-slate-600 flex items-center justify-center">
              <BarChart3 className="w-5 h-5" />
            </div>
            <h2 className="text-base font-bold text-slate-900">
              {t('actions.reports')}
            </h2>
            <p className="text-xs text-slate-600">
              Indicadores de cumplimiento de tiempos legales y estadísticas de PQRSDF.
            </p>
          </div>
          <span className="text-xs text-slate-400 font-medium mt-4 block">
            En desarrollo
          </span>
        </div>
      </div>
    </div>
  );
}
