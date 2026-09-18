'use client';

import React from 'react';
import { Users, AlertTriangle } from 'lucide-react';

interface WorkloadBadgeProps {
  activeCount: number;
  maxCapacity?: number;
  className?: string;
  showIcon?: boolean;
}

export function WorkloadBadge({
  activeCount,
  maxCapacity = 5,
  className = '',
  showIcon = true,
}: WorkloadBadgeProps) {
  const isFull = activeCount >= maxCapacity;
  const isHigh = activeCount >= 3 && activeCount < maxCapacity;

  if (isFull) {
    return (
      <span
        className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200 ${className}`}
      >
        {showIcon && <AlertTriangle className="w-3.5 h-3.5 text-rose-500 shrink-0" />}
        <span>Cupo Lleno ({activeCount}/{maxCapacity})</span>
      </span>
    );
  }

  if (isHigh) {
    return (
      <span
        className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-amber-50 text-amber-800 border border-amber-200 ${className}`}
      >
        {showIcon && <Users className="w-3.5 h-3.5 text-amber-600 shrink-0" />}
        <span>{activeCount}/{maxCapacity} activas</span>
      </span>
    );
  }

  return (
    <span
      className={`inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-700 border border-slate-200 ${className}`}
    >
      {showIcon && <Users className="w-3.5 h-3.5 text-slate-500 shrink-0" />}
      <span>{activeCount}/{maxCapacity} activas</span>
    </span>
  );
}
