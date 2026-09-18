'use client';

import React, { useState, useEffect, useCallback } from 'react';
import Link from 'next/link';
import { ShieldAlert, ArrowLeft, RefreshCw, Inbox, AlertCircle, Clock } from 'lucide-react';
import { useAuth } from '@/shared/auth/useAuth';
import { 
  getUnassignedTickets, 
  getAssignedTickets, 
  getAssignableOfficials,
  getActiveDestinationAreas,
  SharedDestinationAreaDto
} from '@/shared/api/client';
import { 
  UnassignedTicketDto, 
  AssignedTicketDto, 
  AssignableOfficialDto 
} from './types/assignment.types';
import { AssignmentTabs } from './components/AssignmentTabs';
import { UnassignedQueueTable } from './components/UnassignedQueueTable';
import { InReviewQueueTable } from './components/InReviewQueueTable';
import { TicketDetailDrawer } from './components/TicketDetailDrawer';
import { AssignOfficialModal } from './components/AssignOfficialModal';
import { ReassignOfficialModal } from './components/ReassignOfficialModal';

export default function AssignmentsPage() {
  const { user, isLoading: authLoading, isAuthenticated } = useAuth();
  const [areas, setAreas] = useState<SharedDestinationAreaDto[]>([]);

  // Active Tab
  const [activeTab, setActiveTab] = useState<'unassigned' | 'inReview'>('unassigned');

  // Unassigned tickets state
  const [unassignedTickets, setUnassignedTickets] = useState<UnassignedTicketDto[]>([]);
  const [isUnassignedLoading, setIsUnassignedLoading] = useState<boolean>(true);
  const [filterType, setFilterType] = useState<number | undefined>(undefined);
  const [filterAreaId, setFilterAreaId] = useState<string | undefined>(undefined);
  const [filterSearch, setFilterSearch] = useState<string | undefined>(undefined);

  // In Review tickets state
  const [inReviewTickets, setInReviewTickets] = useState<AssignedTicketDto[]>([]);
  const [isInReviewLoading, setIsInReviewLoading] = useState<boolean>(true);
  const [inReviewSearch, setInReviewSearch] = useState<string | undefined>(undefined);
  const [inReviewOfficialId, setInReviewOfficialId] = useState<string | undefined>(undefined);

  // Officials roster
  const [officials, setOfficials] = useState<AssignableOfficialDto[]>([]);

  // Drawers and Modals state
  const [selectedTicket, setSelectedTicket] = useState<UnassignedTicketDto | AssignedTicketDto | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState<boolean>(false);
  const [assignModalTicket, setAssignModalTicket] = useState<UnassignedTicketDto | null>(null);
  const [reassignModalTicket, setReassignModalTicket] = useState<AssignedTicketDto | null>(null);

  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Fetch unassigned tickets
  const fetchUnassigned = useCallback(async () => {
    setIsUnassignedLoading(true);
    setErrorMessage(null);

    const result = await getUnassignedTickets({
      type: filterType,
      destinationAreaId: filterAreaId,
      search: filterSearch,
    });

    if (result.ok) {
      setUnassignedTickets(result.value);
    } else {
      setErrorMessage(result.error.message);
    }

    setIsUnassignedLoading(false);
  }, [filterType, filterAreaId, filterSearch]);

  // Fetch in-review tickets
  const fetchInReview = useCallback(async () => {
    setIsInReviewLoading(true);
    setErrorMessage(null);

    const result = await getAssignedTickets({
      search: inReviewSearch,
      officialId: inReviewOfficialId,
    });

    if (result.ok) {
      setInReviewTickets(result.value);
    } else {
      setErrorMessage(result.error.message);
    }

    setIsInReviewLoading(false);
  }, [inReviewSearch, inReviewOfficialId]);

  // Fetch officials
  const fetchOfficials = useCallback(async () => {
    const result = await getAssignableOfficials();
    if (result.ok) {
      setOfficials(result.value);
    }
  }, []);

  // Fetch destination areas
  const fetchAreas = useCallback(async () => {
    const result = await getActiveDestinationAreas();
    if (result.ok) {
      setAreas(result.value);
    }
  }, []);

  useEffect(() => {
    if (isAuthenticated && user?.role === 'Administrador') {
      fetchUnassigned();
      fetchInReview();
      fetchOfficials();
      fetchAreas();
    }
  }, [isAuthenticated, user?.role, fetchUnassigned, fetchInReview, fetchOfficials, fetchAreas]);

  const handleSelectTicket = (ticket: UnassignedTicketDto | AssignedTicketDto) => {
    setSelectedTicket(ticket);
    setIsDrawerOpen(true);
  };

  const handleCloseDrawer = () => {
    setIsDrawerOpen(false);
    setSelectedTicket(null);
  };

  const handleAssignTicket = (ticket: UnassignedTicketDto) => {
    setAssignModalTicket(ticket);
  };

  const handleReassignTicket = (ticket: AssignedTicketDto) => {
    setReassignModalTicket(ticket);
  };

  // Auth Loading State
  if (authLoading) {
    return (
      <div className="min-h-[50vh] flex items-center justify-center">
        <RefreshCw className="w-8 h-8 text-institutional-600 animate-spin" />
      </div>
    );
  }

  // RBAC 403 Forbidden State for Non-Administrators
  if (!isAuthenticated || user?.role !== 'Administrador') {
    return (
      <div className="max-w-xl mx-auto py-16 px-4 text-center space-y-5">
        <div className="w-16 h-16 bg-red-50 text-red-600 rounded-full flex items-center justify-center mx-auto border border-red-200 shadow-xs">
          <ShieldAlert className="w-8 h-8" />
        </div>
        <div className="space-y-2">
          <h1 className="text-2xl font-bold text-slate-900">Acceso Restringido (403)</h1>
          <p className="text-sm text-slate-600 leading-relaxed">
            El módulo de asignación y balanceo de solicitudes está reservado exclusivamente para usuarios con el rol de <strong className="text-slate-800">Administrador</strong>.
          </p>
        </div>
        <div className="pt-2">
          <Link
            href="/dashboard"
            className="inline-flex items-center space-x-2 px-5 py-2.5 bg-institutional-600 text-white text-sm font-semibold rounded-xl hover:bg-institutional-700 transition-colors shadow-xs"
          >
            <ArrowLeft className="w-4 h-4" />
            <span>Volver al Tablero Principal</span>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Top Header */}
      <div className="bg-white rounded-2xl border border-slate-200 p-6 shadow-xs flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div className="space-y-1">
          <div className="flex items-center space-x-2 text-xs text-institutional-600 font-medium">
            <Link href="/dashboard" className="hover:underline flex items-center gap-1">
              <ArrowLeft className="w-3.5 h-3.5" />
              <span>Tablero</span>
            </Link>
            <span>/</span>
            <span className="text-slate-500">Asignación de Solicitudes</span>
          </div>
          <h1 className="text-2xl font-bold text-slate-900">
            Asignación y Balanceo de PQRSDF
          </h1>
          <p className="text-xs text-slate-500">
            Supervise la cola de solicitudes ciudadanas sin asignar y distribuya las cargas equitativamente a los funcionarios activos.
          </p>
        </div>

        <div className="flex items-center space-x-3">
          <div className="px-3.5 py-1.5 bg-institutional-50 text-institutional-700 rounded-xl border border-institutional-200 text-xs font-semibold flex items-center gap-1.5">
            <Inbox className="w-4 h-4" />
            <span>{unassignedTickets.length} sin asignar</span>
          </div>
          <div className="px-3.5 py-1.5 bg-purple-50 text-purple-700 rounded-xl border border-purple-200 text-xs font-semibold flex items-center gap-1.5">
            <Clock className="w-4 h-4" />
            <span>{inReviewTickets.length} en trámite</span>
          </div>
        </div>
      </div>

      {/* Error Alert if any */}
      {errorMessage && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-xl flex items-center justify-between text-sm text-red-700">
          <div className="flex items-center space-x-2">
            <AlertCircle className="w-5 h-5 text-red-500 shrink-0" />
            <span>{errorMessage}</span>
          </div>
          <button
            onClick={() => {
              fetchUnassigned();
              fetchInReview();
            }}
            className="text-xs font-semibold underline hover:text-red-900"
          >
            Reintentar
          </button>
        </div>
      )}

      {/* Assignment Navigation Tabs */}
      <AssignmentTabs
        activeTab={activeTab}
        onTabChange={setActiveTab}
        unassignedCount={unassignedTickets.length}
        inReviewCount={inReviewTickets.length}
      />

      {/* Active Tab View */}
      {activeTab === 'unassigned' ? (
        <UnassignedQueueTable
          tickets={unassignedTickets}
          areas={areas}
          isLoading={isUnassignedLoading}
          onRefresh={fetchUnassigned}
          onSelectTicket={handleSelectTicket}
          onAssignTicket={handleAssignTicket}
          filterType={filterType}
          filterAreaId={filterAreaId}
          filterSearch={filterSearch}
          onFilterChange={(filters) => {
            setFilterType(filters.type);
            setFilterAreaId(filters.destinationAreaId);
            setFilterSearch(filters.search);
          }}
        />
      ) : (
        <InReviewQueueTable
          tickets={inReviewTickets}
          isLoading={isInReviewLoading}
          onRefresh={fetchInReview}
          onSelectTicket={handleSelectTicket}
          onReassignTicket={handleReassignTicket}
          search={inReviewSearch}
          onSearchChange={setInReviewSearch}
          officialId={inReviewOfficialId}
          onOfficialChange={setInReviewOfficialId}
          officials={officials}
        />
      )}

      {/* Ticket Preview Slide-Over Drawer */}
      <TicketDetailDrawer
        ticket={selectedTicket}
        isOpen={isDrawerOpen}
        onClose={handleCloseDrawer}
        onAssign={handleAssignTicket}
        onReassign={handleReassignTicket}
      />

      {/* Assign Official Modal */}
      <AssignOfficialModal
        isOpen={Boolean(assignModalTicket)}
        ticket={assignModalTicket}
        onClose={() => setAssignModalTicket(null)}
        onSuccess={() => {
          fetchUnassigned();
          fetchInReview();
          fetchOfficials();
          handleCloseDrawer();
        }}
      />

      {/* Reassign Official Modal */}
      <ReassignOfficialModal
        isOpen={Boolean(reassignModalTicket)}
        ticket={reassignModalTicket}
        onClose={() => setReassignModalTicket(null)}
        onSuccess={() => {
          fetchUnassigned();
          fetchInReview();
          fetchOfficials();
          handleCloseDrawer();
        }}
      />
    </div>
  );
}
