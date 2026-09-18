export interface EmptyStateProps {
  /**
   * Title text displayed in bold.
   */
  readonly title: string;

  /**
   * Optional informative description explaining the empty state.
   */
  readonly description?: string;

  /**
   * Optional call-to-action button.
   */
  readonly action?: {
    readonly label: string;
    readonly onClick: () => void;
  };

  /**
   * Optional custom icon or graphic slot.
   */
  readonly icon?: React.ReactNode;

  /**
   * Optional container styling classes.
   */
  readonly className?: string;
}

export function EmptyState({ title, description, action, icon, className = '' }: EmptyStateProps) {
  return (
    <section
      className={`flex flex-col items-center justify-center p-8 text-center bg-white rounded-xl border border-slate-200 ${className}`}
    >
      {icon && <div className="mb-4 text-slate-400">{icon}</div>}
      <h3 className="text-lg font-semibold text-slate-800">{title}</h3>
      {description && <p className="text-sm text-slate-500 mt-1 max-w-sm">{description}</p>}
      {action && (
        <button
          type="button"
          onClick={action.onClick}
          className="mt-5 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-institutional-600 hover:bg-institutional-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-institutional-500 transition"
        >
          {action.label}
        </button>
      )}
    </section>
  );
}
