import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { NextIntlClientProvider } from 'next-intl';
import { ConfirmationReceipt } from '@/app/pqrsdf/components/ConfirmationReceipt';
import type { MakePqrsdfResponse } from '@/app/pqrsdf/types/pqrsdf';
import messages from '../../messages/es.json';

const renderWithIntl = (component: React.ReactElement) => {
  return render(
    <NextIntlClientProvider locale="es" messages={messages}>
      {component}
    </NextIntlClientProvider>
  );
};

const mockTicketResponse: MakePqrsdfResponse = {
  id: '00000000-0000-0000-0000-000000000001',
  radicadoNumber: '2026-00000042',
  type: 'Petition',
  destinationAreaName: 'Atención al Ciudadano',
  createdAtUtc: '2026-09-18T14:30:00Z',
  dueDate: '2026-10-09',
  businessDaysCount: 15,
  status: 'Received',
  isAnonymous: false,
};

describe('ConfirmationReceipt Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    Object.assign(navigator, {
      clipboard: {
        writeText: vi.fn().mockResolvedValue(undefined),
      },
    });
  });

  it('renders confirmation header and details accurately', () => {
    renderWithIntl(<ConfirmationReceipt data={mockTicketResponse} />);

    // Success title
    expect(screen.getByText('¡Solicitud Radicada con Éxito!')).toBeInTheDocument();

    // Radicado number displayed prominently
    expect(screen.getByText('2026-00000042')).toBeInTheDocument();

    // Destination area
    expect(screen.getByText('Atención al Ciudadano')).toBeInTheDocument();

    // Term days message in statutory box
    expect(screen.getAllByText(/15 días hábiles/i).length).toBeGreaterThanOrEqual(1);

    // Copy radicado button
    expect(screen.getByRole('button', { name: /Copiar radicado/i })).toBeInTheDocument();

    // Track ticket link button
    const trackLink = screen.getByRole('link', { name: /Consultar estado de solicitud/i });
    expect(trackLink).toBeInTheDocument();
    expect(trackLink).toHaveAttribute('href', '/pqrsdf/search?radicado=2026-00000042');
  });

  it('copies the radicado number to clipboard and updates button text to feedback state', async () => {
    renderWithIntl(<ConfirmationReceipt data={mockTicketResponse} />);

    const copyButton = screen.getByRole('button', { name: /Copiar radicado/i });
    fireEvent.click(copyButton);

    expect(navigator.clipboard.writeText).toHaveBeenCalledWith('2026-00000042');

    await waitFor(() => {
      expect(screen.getByText('¡Número copiado!')).toBeInTheDocument();
    });
  });

  it('calls onReset when clicking file another ticket button', () => {
    const onReset = vi.fn();
    renderWithIntl(<ConfirmationReceipt data={mockTicketResponse} onReset={onReset} />);

    const fileAnotherBtn = screen.getByRole('button', { name: /Radicar otra solicitud/i });
    fireEvent.click(fileAnotherBtn);

    expect(onReset).toHaveBeenCalledTimes(1);
  });

  it('renders anonymous indicator when ticket was filed anonymously', () => {
    const anonymousTicket: MakePqrsdfResponse = {
      ...mockTicketResponse,
      type: 'Denunciation',
      isAnonymous: true,
      businessDaysCount: 30,
    };

    renderWithIntl(<ConfirmationReceipt data={anonymousTicket} />);

    expect(screen.getByText(/Anónimo \(Sin datos de contacto\)/i)).toBeInTheDocument();
    expect(screen.getAllByText(/30 días hábiles/i).length).toBeGreaterThanOrEqual(1);
  });
});
