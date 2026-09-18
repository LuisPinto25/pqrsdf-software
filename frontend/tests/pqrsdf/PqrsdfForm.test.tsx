import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor, fireEvent } from '@testing-library/react';
import { NextIntlClientProvider } from 'next-intl';
import { PqrsdfForm } from '@/app/pqrsdf/components/PqrsdfForm';
import { rawClient } from '@/shared/api/client';
import messages from '../../messages/es.json';

const renderWithIntl = (component: React.ReactElement) => {
  return render(
    <NextIntlClientProvider locale="es" messages={messages}>
      {component}
    </NextIntlClientProvider>
  );
};

const mockAreas = [
  { id: '11111111-1111-1111-1111-111111111111', name: 'Atención al Ciudadano', code: 'ATC' },
  {
    id: '22222222-2222-2222-2222-222222222222',
    name: 'Control Interno Disciplinario',
    code: 'CID',
  },
];

describe('PqrsdfForm Component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(rawClient, 'GET').mockResolvedValue({
      data: { value: mockAreas },
      response: new Response(JSON.stringify({ value: mockAreas }), { status: 200 }),
    } as never);
  });

  it('renders all default form fields including applicant information', async () => {
    const onSuccess = vi.fn();
    renderWithIntl(<PqrsdfForm onSuccess={onSuccess} />);

    // Classification section
    expect(screen.getByLabelText(/Tipo de Solicitud/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Área o Dependencia Destino/i)).toBeInTheDocument();

    // Destination areas should be loaded
    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Atención al Ciudadano/i })).toBeInTheDocument();
    });

    // Applicant section should be visible for default 'Petition'
    expect(screen.getByLabelText(/Nombre Completo/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Tipo de Documento/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Número de Documento/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Correo Electrónico/i)).toBeInTheDocument();

    // Ticket content
    expect(screen.getByLabelText(/Asunto/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Descripción Detallada/i)).toBeInTheDocument();

    // Anonymous checkbox should NOT be present for Petition
    expect(
      screen.queryByRole('checkbox', { name: /Radicar de forma anónima/i })
    ).not.toBeInTheDocument();

    // Submit button
    expect(screen.getByRole('button', { name: /Radicar Solicitud/i })).toBeInTheDocument();
  });

  it('allows anonymous registration for Denunciation and hides applicant fields when checked', async () => {
    const onSuccess = vi.fn();
    renderWithIntl(<PqrsdfForm onSuccess={onSuccess} />);

    // Select 'Denunciation'
    const typeSelect = screen.getByLabelText(/Tipo de Solicitud/i);
    fireEvent.change(typeSelect, { target: { value: 'Denunciation' } });

    // Anonymous checkbox should now be visible
    const anonymousCheckbox = await screen.findByRole('checkbox', {
      name: /Radicar de forma anónima/i,
    });
    expect(anonymousCheckbox).toBeInTheDocument();
    expect(anonymousCheckbox).not.toBeChecked();

    // Applicant fields should still be present before checking
    expect(screen.getByLabelText(/Nombre Completo/i)).toBeInTheDocument();

    // Check anonymous
    fireEvent.click(anonymousCheckbox);
    expect(anonymousCheckbox).toBeChecked();

    // Applicant fields should be removed from the DOM
    expect(screen.queryByLabelText(/Nombre Completo/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Correo Electrónico/i)).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/Número de Documento/i)).not.toBeInTheDocument();
  });

  it('validates required fields on submit and displays error messages', async () => {
    const onSuccess = vi.fn();
    renderWithIntl(<PqrsdfForm onSuccess={onSuccess} />);

    // Click submit without filling required fields
    const submitButton = screen.getByRole('button', { name: /Radicar Solicitud/i });
    fireEvent.click(submitButton);

    // Validation errors should appear
    await waitFor(() => {
      expect(screen.getByText('Debe seleccionar el área de destino.')).toBeInTheDocument();
      expect(
        screen.getByText('El nombre completo debe tener al menos 3 caracteres.')
      ).toBeInTheDocument();
      expect(
        screen.getByText('El número de documento debe tener al menos 4 caracteres.')
      ).toBeInTheDocument();
      expect(screen.getByText('Ingrese un correo electrónico válido.')).toBeInTheDocument();
      expect(
        screen.getByText('El asunto debe contener al menos 5 caracteres.')
      ).toBeInTheDocument();
      expect(
        screen.getByText('La descripción debe contener al menos 10 caracteres.')
      ).toBeInTheDocument();
    });

    expect(onSuccess).not.toHaveBeenCalled();
  });

  it('submits successfully and calls onSuccess with response data', async () => {
    const onSuccess = vi.fn();

    const mockResponse = {
      id: '00000000-0000-0000-0000-000000000001',
      radicadoNumber: '2026-00000001',
      type: 'Petition',
      destinationAreaName: 'Atención al Ciudadano',
      createdAtUtc: '2026-09-18T10:00:00Z',
      dueDate: '2026-10-09',
      businessDaysCount: 15,
      status: 'Received',
      isAnonymous: false,
    };

    vi.spyOn(rawClient, 'POST').mockResolvedValue({
      data: { value: mockResponse },
      response: new Response(JSON.stringify({ value: mockResponse }), { status: 201 }),
    } as never);

    renderWithIntl(<PqrsdfForm onSuccess={onSuccess} />);

    // Wait for areas to load and select one
    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Atención al Ciudadano/i })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText(/Área o Dependencia Destino/i), {
      target: { value: mockAreas[0].id },
    });

    // Fill applicant info
    fireEvent.change(screen.getByLabelText(/Nombre Completo/i), {
      target: { value: 'Carlos Gomez' },
    });
    fireEvent.change(screen.getByLabelText(/Número de Documento/i), {
      target: { value: '12345678' },
    });
    fireEvent.change(screen.getByLabelText(/Correo Electrónico/i), {
      target: { value: 'carlos@ejemplo.com' },
    });

    // Fill ticket content
    fireEvent.change(screen.getByLabelText(/Asunto/i), {
      target: { value: 'Solicitud de información general' },
    });
    fireEvent.change(screen.getByLabelText(/Descripción Detallada/i), {
      target: { value: 'Esta es una descripción detallada con más de diez caracteres.' },
    });

    // Submit
    const submitButton = screen.getByRole('button', { name: /Radicar Solicitud/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(onSuccess).toHaveBeenCalledWith(mockResponse);
    });
  });

  it('displays server error message when submission fails', async () => {
    const onSuccess = vi.fn();

    vi.spyOn(rawClient, 'POST').mockResolvedValue({
      error: { detail: 'Error de prueba del servidor al radicar.' },
      response: new Response(
        JSON.stringify({ detail: 'Error de prueba del servidor al radicar.' }),
        { status: 400 }
      ),
    } as never);

    renderWithIntl(<PqrsdfForm onSuccess={onSuccess} />);

    await waitFor(() => {
      expect(screen.getByRole('option', { name: /Atención al Ciudadano/i })).toBeInTheDocument();
    });
    fireEvent.change(screen.getByLabelText(/Área o Dependencia Destino/i), {
      target: { value: mockAreas[0].id },
    });

    fireEvent.change(screen.getByLabelText(/Nombre Completo/i), {
      target: { value: 'Carlos Gomez' },
    });
    fireEvent.change(screen.getByLabelText(/Número de Documento/i), {
      target: { value: '12345678' },
    });
    fireEvent.change(screen.getByLabelText(/Correo Electrónico/i), {
      target: { value: 'carlos@ejemplo.com' },
    });
    fireEvent.change(screen.getByLabelText(/Asunto/i), {
      target: { value: 'Solicitud de información general' },
    });
    fireEvent.change(screen.getByLabelText(/Descripción Detallada/i), {
      target: { value: 'Esta es una descripción detallada con más de diez caracteres.' },
    });

    const submitButton = screen.getByRole('button', { name: /Radicar Solicitud/i });
    fireEvent.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText('Error de prueba del servidor al radicar.')).toBeInTheDocument();
    });

    expect(onSuccess).not.toHaveBeenCalled();
  });
});
