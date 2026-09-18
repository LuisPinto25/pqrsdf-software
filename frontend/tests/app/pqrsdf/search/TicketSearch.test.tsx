import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { NextIntlClientProvider } from 'next-intl';
import { TicketSearchContainer } from '@/app/pqrsdf/search/components/TicketSearchContainer';
import * as apiClient from '@/shared/api/client';
import { ok, err } from '@/shared/types/result';
import type { PublicTicketStatusDto } from '@/app/pqrsdf/types/pqrsdf';
import messages from '../../../../messages/es.json';

// Helper for intl wrapping
const renderWithIntl = (component: React.ReactElement) => {
  return render(
    <NextIntlClientProvider locale="es" messages={messages}>
      {component}
    </NextIntlClientProvider>
  );
};

const mockInTermTicket: PublicTicketStatusDto = {
  radicadoNumber: '2026-00000001',
  requestType: 'Petition',
  destinationAreaName: 'Atención al Ciudadano',
  subject: 'Solicitud de certificado catastral',
  description: 'Por medio de la presente solicito el certificado catastral del predio.',
  status: 'InReview',
  filingDate: '2026-03-01',
  dueDate: '2026-03-22',
  remainingBusinessDays: 10,
  isOverdue: false,
  overdueBusinessDays: null,
  timeline: [
    { status: 'Registered', title: 'Radicado', date: '2026-03-01T10:00:00Z', isCompleted: true, isCurrent: false },
    { status: 'Assigned', title: 'Asignado', date: '2026-03-02T08:30:00Z', isCompleted: true, isCurrent: false },
    { status: 'InReview', title: 'En trámite', date: '2026-03-03T14:15:00Z', isCompleted: true, isCurrent: true },
    { status: 'Answered', title: 'Respuesta emitida', date: null, isCompleted: false, isCurrent: false },
    { status: 'Closed', title: 'Cerrado', date: null, isCompleted: false, isCurrent: false },
  ],
  resolution: null,
};

const mockOverdueTicket: PublicTicketStatusDto = {
  ...mockInTermTicket,
  radicadoNumber: '2026-00000002',
  isOverdue: true,
  remainingBusinessDays: null,
  overdueBusinessDays: 4,
};

const mockClosedTicketWithResolution: PublicTicketStatusDto = {
  ...mockInTermTicket,
  radicadoNumber: '2026-00000003',
  status: 'Closed',
  remainingBusinessDays: null,
  isOverdue: false,
  timeline: [
    { status: 'Registered', title: 'Radicado', date: '2026-02-01T10:00:00Z', isCompleted: true, isCurrent: false },
    { status: 'Assigned', title: 'Asignado', date: '2026-02-02T08:30:00Z', isCompleted: true, isCurrent: false },
    { status: 'InReview', title: 'En trámite', date: '2026-02-03T14:15:00Z', isCompleted: true, isCurrent: false },
    { status: 'Answered', title: 'Respuesta emitida', date: '2026-02-10T16:00:00Z', isCompleted: true, isCurrent: false },
    { status: 'Closed', title: 'Cerrado', date: '2026-02-10T16:00:00Z', isCompleted: true, isCurrent: true },
  ],
  resolution: {
    responseText: 'Se adjunta el certificado catastral correspondiente a la referencia indicada.',
    responseDate: '2026-02-10T16:00:00Z',
  },
};

describe('TicketSearchContainer Component', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('renders initial search form with empty radicado input', () => {
    renderWithIntl(<TicketSearchContainer initialRadicado="" />);

    expect(screen.getByPlaceholderText(/AAAA-NNNNNNNN/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Consultar/i })).toBeInTheDocument();
  });

  it('validates radicado format and shows error message when invalid', async () => {
    renderWithIntl(<TicketSearchContainer initialRadicado="" />);

    const input = screen.getByPlaceholderText(/AAAA-NNNNNNNN/i);
    const submitBtn = screen.getByRole('button', { name: /Consultar/i });

    fireEvent.change(input, { target: { value: 'invalid-radicado' } });
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(screen.getByText(/formato AAAA-NNNNNNNN/i)).toBeInTheDocument();
    });
  });

  it('successfully retrieves and displays in-term ticket status, timeline, and remaining days', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(ok(mockInTermTicket));

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-00000001" />);

    await waitFor(() => {
      expect(screen.getByText('2026-00000001')).toBeInTheDocument();
      expect(screen.getByText('Solicitud de certificado catastral')).toBeInTheDocument();
      expect(screen.getByText('Atención al Ciudadano')).toBeInTheDocument();
      expect(screen.getByText(/10 días hábiles/i)).toBeInTheDocument();
    });

    // Check timeline milestone visibility
    expect(screen.getByText('Radicado')).toBeInTheDocument();
    expect(screen.getAllByText('En trámite').length).toBeGreaterThanOrEqual(1);
  });

  it('displays overdue warning badge and elapsed days in mora when ticket is overdue', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(ok(mockOverdueTicket));

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-00000002" />);

    await waitFor(() => {
      expect(screen.getByText('2026-00000002')).toBeInTheDocument();
      expect(screen.getAllByText(/Vencida/i).length).toBeGreaterThanOrEqual(1);
      expect(screen.getByText(/Vencida hace 4 días hábiles/i)).toBeInTheDocument();
    });
  });

  it('displays official institutional resolution card when ticket is closed', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(ok(mockClosedTicketWithResolution));

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-00000003" />);

    await waitFor(() => {
      expect(screen.getByText('2026-00000003')).toBeInTheDocument();
      expect(screen.getByText(/Respuesta Oficial Institucional/i)).toBeInTheDocument();
      expect(
        screen.getByText(/Se adjunta el certificado catastral correspondiente a la referencia indicada/i)
      ).toBeInTheDocument();
    });
  });

  it('displays error message when ticket is not found (404)', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(
      err({
        status: 404,
        message: 'No se encontró ninguna solicitud con el radicado ingresado.',
      })
    );

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-99999999" />);

    await waitFor(() => {
      expect(
        screen.getByText(/No se encontró ninguna solicitud con el radicado ingresado/i)
      ).toBeInTheDocument();
    });
  });

  it('displays rate limit guidance when server returns 429', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(
      err({
        status: 429,
        message: 'Demasiadas consultas de seguimiento.',
      })
    );

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-00000001" />);

    await waitFor(() => {
      expect(
        screen.getByText(/Ha superado el límite de consultas permitidas por minuto/i)
      ).toBeInTheDocument();
    });
  });

  it('clears results and resets search form when clicking clear button', async () => {
    vi.spyOn(apiClient, 'getTicketByRadicado').mockResolvedValue(ok(mockInTermTicket));

    renderWithIntl(<TicketSearchContainer initialRadicado="2026-00000001" />);

    await waitFor(() => {
      expect(screen.getByText('2026-00000001')).toBeInTheDocument();
    });

    const clearBtn = screen.getByRole('button', { name: /Limpiar/i });
    fireEvent.click(clearBtn);

    await waitFor(() => {
      expect(screen.queryByText('Solicitud de certificado catastral')).not.toBeInTheDocument();
    });
  });
});
