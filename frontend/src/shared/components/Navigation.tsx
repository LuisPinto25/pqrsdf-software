'use client';

import React from 'react';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useTranslations } from 'next-intl';
import { useAuth } from '@/shared/auth/useAuth';
import { 
  FileText, 
  Search, 
  LayoutDashboard, 
  LogIn, 
  LogOut, 
  User, 
  ShieldCheck 
} from 'lucide-react';

export function Navigation() {
  const t = useTranslations('Navigation');
  const tDash = useTranslations('Dashboard');
  const pathname = usePathname();
  const { user, isAuthenticated, isLoading, logout } = useAuth();

  const isActive = (path: string) => {
    if (path === '/') return pathname === '/';
    return pathname.startsWith(path);
  };

  const isRoleAdmin = user?.role === 'Administrador';

  return (
    <header className="bg-institutional-800 text-white shadow-md">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-16 flex items-center justify-between">
        {/* Brand */}
        <div className="flex items-center space-x-3">
          <Link href="/" className="flex items-center space-x-2 font-bold text-lg tracking-tight hover:text-institutional-100 transition-colors">
            <span className="w-8 h-8 rounded-lg bg-institutional-600 flex items-center justify-center font-black text-white text-sm shadow-inner">
              PQ
            </span>
            <span>Portal PQRSDF</span>
          </Link>
        </div>

        {/* Navigation Links */}
        <nav className="flex items-center space-x-1 sm:space-x-4">
          <Link
            href="/"
            className={`text-sm font-medium px-3 py-1.5 rounded-lg transition-colors ${
              isActive('/')
                ? 'bg-institutional-900 text-white'
                : 'text-institutional-100 hover:bg-institutional-700 hover:text-white'
            }`}
          >
            {t('home')}
          </Link>

          <Link
            href="/pqrsdf"
            className={`text-sm font-medium px-3 py-1.5 rounded-lg transition-colors inline-flex items-center space-x-1.5 ${
              isActive('/pqrsdf') && !isActive('/pqrsdf/search')
                ? 'bg-institutional-900 text-white'
                : 'text-institutional-100 hover:bg-institutional-700 hover:text-white'
            }`}
          >
            <FileText className="w-4 h-4 hidden sm:inline" />
            <span>{t('pqrsdf')}</span>
          </Link>

          <Link
            href="/pqrsdf/search"
            className={`text-sm font-medium px-3 py-1.5 rounded-lg transition-colors inline-flex items-center space-x-1.5 ${
              isActive('/pqrsdf/search')
                ? 'bg-institutional-900 text-white'
                : 'text-institutional-100 hover:bg-institutional-700 hover:text-white'
            }`}
          >
            <Search className="w-4 h-4 hidden sm:inline" />
            <span>{t('search')}</span>
          </Link>

          {isAuthenticated && (
            <Link
              href="/dashboard"
              className={`text-sm font-medium px-3 py-1.5 rounded-lg transition-colors inline-flex items-center space-x-1.5 ${
                isActive('/dashboard')
                  ? 'bg-institutional-900 text-white'
                  : 'text-institutional-100 hover:bg-institutional-700 hover:text-white'
              }`}
            >
              <LayoutDashboard className="w-4 h-4 hidden sm:inline" />
              <span>{t('dashboard')}</span>
            </Link>
          )}

          {/* User authentication status & actions */}
          {!isLoading && (
            <div className="flex items-center space-x-3 pl-2 sm:pl-4 border-l border-institutional-700">
              {isAuthenticated && user ? (
                <div className="flex items-center space-x-3">
                  {/* User Profile Badge */}
                  <div className="hidden md:flex flex-col text-right">
                    <span className="text-xs font-semibold text-white truncate max-w-[140px]">
                      {user.fullName || user.email}
                    </span>
                    <span className="inline-flex items-center justify-end space-x-1 text-[10px] text-institutional-200">
                      {isRoleAdmin ? (
                        <ShieldCheck className="w-3 h-3 text-purple-300" />
                      ) : (
                        <User className="w-3 h-3 text-institutional-300" />
                      )}
                      <span>
                        {user.role ? tDash(`roles.${user.role}`) : user.role}
                      </span>
                    </span>
                  </div>

                  {/* Logout Button */}
                  <button
                    onClick={logout}
                    title={t('logout')}
                    className="inline-flex items-center space-x-1.5 px-3 py-1.5 text-xs font-medium text-red-200 hover:text-white bg-red-950/40 hover:bg-red-900/60 rounded-lg border border-red-800/50 transition-colors"
                  >
                    <LogOut className="w-3.5 h-3.5" />
                    <span className="hidden sm:inline">{t('logout')}</span>
                  </button>
                </div>
              ) : (
                <Link
                  href="/auth"
                  className="inline-flex items-center space-x-1.5 px-3 py-1.5 text-xs font-medium text-white bg-institutional-600 hover:bg-institutional-500 rounded-lg transition-colors shadow-sm"
                >
                  <LogIn className="w-3.5 h-3.5" />
                  <span>{t('login')}</span>
                </Link>
              )}
            </div>
          )}
        </nav>
      </div>
    </header>
  );
}
