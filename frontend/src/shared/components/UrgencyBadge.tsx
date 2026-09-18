'use client';

import React from 'react';
import { AlertCircle, AlertTriangle, CheckCircle } from 'lucide-react';

export type UrgencyLevel = 'Critical' | 'Attention' | 'OnTime';

interface UrgencyBadgeProps {
  remainingBusinessDays: number;
  urgencyLevel?: UrgencyLevel;
  className?: string;
}

export function UrgencyBadge({
  remainingBusinessDays,
  urgencyLevel,
  className = '',
}: UrgencyBadgeProps) {
  // If not explicitly passed, compute based on standard thresholds
  const level: UrgencyLevel =
    urgencyLevel ??
    (remainingBusinessDays <= 3
      ? 'Critical'
      : remainingBusinessDays <= 7
      ? 'Attention'
      : 'OnTime');

  if (remainingBusinessDays <= 0) {
    return (
      <span
        className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-rose-100 text-rose-800 border border-rose-300 ${className}`}
      >
        <AlertCircle className="w-3.5 h-3.5 text-rose-600 shrink-0" />
        <span>Vencida ({remainingBusinessDays}d)</span>
      </span>
    );
  }

  if (level === 'Critical') {
    return (
      <span
        className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-red-50 text-red-700 border border-red-200 ${className}`}
      >
        <AlertCircle className="w-3.5 h-3.5 text-red-500 shrink-0" />
        <span>{remainingBusinessDays} {remainingBusinessDays === 1 ? 'día hábil' : 'días hábiles'}</span>
      </span>
    );
  }

  if (level === 'Attention') {
    return (
      <span
        className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-50 text-amber-800 border border-amber-200 ${className}`}
      >
        <AlertTriangle className="w-3.5 h-3.5 text-amber-600 shrink-0" />
        <span>{remainingBusinessDays} días hábiles</span>
      </span>
    );
  }

  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200 ${className}`}
    >
      <CheckCircle className="w-3.5 h-3.5 text-emerald-500 shrink-0" />
      <span>{remainingBusinessDays} días hábiles</span>
    </span>
  );
}
