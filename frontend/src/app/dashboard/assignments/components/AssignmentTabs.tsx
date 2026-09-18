'use client';

import React from 'react';
import { Inbox, Clock } from 'lucide-react';

interface AssignmentTabsProps {
  activeTab: 'unassigned' | 'inReview';
  onTabChange: (tab: 'unassigned' | 'inReview') => void;
  unassignedCount?: number;
  inReviewCount?: number;
}

export function AssignmentTabs({
  activeTab,
  onTabChange,
  unassignedCount,
  inReviewCount,
}: AssignmentTabsProps) {
  return (
    <div className="border-b border-slate-200 bg-white px-2 pt-2 rounded-t-xl">
      <nav className="-mb-px flex space-x-6" aria-label="Pestañas de Asignación">
        <button
          onClick={() => onTabChange('unassigned')}
          className={`group inline-flex items-center py-3.5 px-2 border-b-2 font-semibold text-sm transition-all ${
            activeTab === 'unassigned'
              ? 'border-institutional-600 text-institutional-600'
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
          }`}
        >
          <Inbox
            className={`w-4 h-4 mr-2 transition-colors ${
              activeTab === 'unassigned'
                ? 'text-institutional-600'
                : 'text-slate-400 group-hover:text-slate-500'
            }`}
          />
          <span>Cola sin Asignar</span>
          {unassignedCount !== undefined && (
            <span
              className={`ml-2.5 py-0.5 px-2 rounded-full text-xs font-bold transition-colors ${
                activeTab === 'unassigned'
                  ? 'bg-institutional-100 text-institutional-700'
                  : 'bg-slate-100 text-slate-600 group-hover:bg-slate-200'
              }`}
            >
              {unassignedCount}
            </span>
          )}
        </button>

        <button
          onClick={() => onTabChange('inReview')}
          className={`group inline-flex items-center py-3.5 px-2 border-b-2 font-semibold text-sm transition-all ${
            activeTab === 'inReview'
              ? 'border-purple-600 text-purple-600'
              : 'border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300'
          }`}
        >
          <Clock
            className={`w-4 h-4 mr-2 transition-colors ${
              activeTab === 'inReview'
                ? 'text-purple-600'
                : 'text-slate-400 group-hover:text-slate-500'
            }`}
          />
          <span>Solicitudes en Trámite</span>
          {inReviewCount !== undefined && (
            <span
              className={`ml-2.5 py-0.5 px-2 rounded-full text-xs font-bold transition-colors ${
                activeTab === 'inReview'
                  ? 'bg-purple-100 text-purple-700'
                  : 'bg-slate-100 text-slate-600 group-hover:bg-slate-200'
              }`}
            >
              {inReviewCount}
            </span>
          )}
        </button>
      </nav>
    </div>
  );
}
