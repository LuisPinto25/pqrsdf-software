export type SpinnerSize = 'sm' | 'md' | 'lg';

export interface LoadingSpinnerProps {
  /**
   * Visual size of the spinner.
   * Default: 'md'
   */
  readonly size?: SpinnerSize;

  /**
   * Accessible label for screen readers.
   * Default: 'Cargando contenido...'
   */
  readonly label?: string;

  /**
   * Optional additional CSS classes.
   */
  readonly className?: string;
}

const sizeClasses: Record<SpinnerSize, string> = {
  sm: 'h-4 w-4 border-2',
  md: 'h-8 w-8 border-3',
  lg: 'h-12 w-12 border-4',
};

export function LoadingSpinner({
  size = 'md',
  label = 'Cargando contenido...',
  className = '',
}: LoadingSpinnerProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className={`inline-flex items-center justify-center ${className}`}
    >
      <div
        className={`animate-spin rounded-full border-institutional-600 border-t-transparent ${sizeClasses[size]}`}
      />
      <span className="sr-only">{label}</span>
    </div>
  );
}
