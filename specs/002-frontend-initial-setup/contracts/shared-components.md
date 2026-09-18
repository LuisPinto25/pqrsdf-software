# Interface Contract: Shared Components

**Location**: `src/shared/components/`  
**Purpose**: Reusable foundational UI elements built with Tailwind CSS and compliant with accessibility and Spanish localization requirements.

---

## 1. `LoadingSpinner` Component

**Path**: `src/shared/components/LoadingSpinner.tsx`

### Props Signature
```typescript
export interface LoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  label?: string;
  className?: string;
}
```

### Rendering Contract
- **Root Element**: `div` with `role="status"` and `aria-live="polite"`.
- **Accessibility**: Includes a visually hidden `span` (`sr-only`) displaying the accessible label (default: `"Cargando contenido..."`).
- **Styling**:
  - `sm`: `h-4 w-4 border-2`
  - `md`: `h-8 w-8 border-3` (default)
  - `lg`: `h-12 w-12 border-4`
  - Color: `border-blue-600 border-t-transparent rounded-full animate-spin`.

---

## 2. `EmptyState` Component

**Path**: `src/shared/components/EmptyState.tsx`

### Props Signature
```typescript
export interface EmptyStateProps {
  title: string;
  description?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
  icon?: React.ReactNode;
  className?: string;
}
```

### Rendering Contract
- **Root Element**: `section` or `div` centered with `flex flex-col items-center justify-center p-8 text-center`.
- **Title**: Rendered in `h3` with `text-lg font-semibold text-gray-800`.
- **Description**: Rendered in `p` with `text-sm text-gray-500 mt-1`.
- **Action**: If provided, renders an accessible button with `px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700`.

---

## 3. Global Error Boundary Screen

**Path**: `src/app/error.tsx`

### Rendering Contract
- Client Component (`'use client'`).
- Displays a prominent Spanish error notice: `"Ha ocurrido un error inesperado"`.
- Provides a `"Reintentar"` button invoking Next.js `reset()`.
- Does NOT leak stack traces, error objects, or internal technical details to the user.
