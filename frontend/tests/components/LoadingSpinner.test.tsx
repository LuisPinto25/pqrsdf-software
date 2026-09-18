import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { LoadingSpinner } from '@/shared/components/LoadingSpinner';

describe('LoadingSpinner Component', () => {
  it('should render with status role and polite aria-live by default', () => {
    render(<LoadingSpinner />);

    const spinner = screen.getByRole('status');
    expect(spinner).toBeInTheDocument();
    expect(spinner).toHaveAttribute('aria-live', 'polite');
    expect(screen.getByText('Cargando contenido...')).toBeInTheDocument();
  });

  it('should render with custom accessible label', () => {
    render(<LoadingSpinner label="Consultando radicado..." />);

    expect(screen.getByText('Consultando radicado...')).toBeInTheDocument();
  });

  it('should apply appropriate size classes', () => {
    const { container: smallContainer } = render(<LoadingSpinner size="sm" />);
    expect(smallContainer.querySelector('.h-4.w-4')).toBeInTheDocument();

    const { container: largeContainer } = render(<LoadingSpinner size="lg" />);
    expect(largeContainer.querySelector('.h-12.w-12')).toBeInTheDocument();
  });
});
