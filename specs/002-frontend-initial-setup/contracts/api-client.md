# Interface Contract: Shared API Client

**Location**: `src/shared/api/client.ts`  
**Purpose**: Typed HTTP client wrapping `openapi-fetch` that executes requests and returns `Result<T, ApiError>`.

---

## 1. External Dependencies & Configuration

- Base Client: `openapi-fetch` (`createClient<paths>({ baseUrl })`).
- Base URL Source: `process.env.NEXT_PUBLIC_API_BASE_URL` (fallback: `http://localhost:5023`).
- Generated Types Source: `@/shared/api/generated/schema`.

---

## 2. Interface Signatures

```typescript
import createClient from 'openapi-fetch';
import type { paths } from '@/shared/api/generated/schema';
import type { Result } from '@/shared/types/result';
import type { ApiError } from '@/shared/types/api';

/**
 * Underlying openapi-fetch client instance.
 */
export const rawClient = createClient<paths>({
  baseUrl: process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5023',
});

/**
 * Safe request wrapper guaranteeing a Result<T, ApiError> return.
 */
export async function safeRequest<T>(
  requestFn: () => Promise<{ data?: T; error?: unknown; response: Response }>
): Promise<Result<T, ApiError>>
```

---

## 3. Error Mapping Rules

| Scenario | Result Status | ApiError Message (Spanish) |
|---|---|---|
| HTTP 400 Bad Request | `400` | Extracted from backend `ProblemDetails.detail` or "Solicitud incorrecta" |
| HTTP 401 Unauthorized | `401` | "No autorizado para acceder a este recurso" |
| HTTP 403 Forbidden | `403` | "Acceso restringido" |
| HTTP 404 Not Found | `404` | "El recurso solicitado no fue encontrado" |
| HTTP 500 Internal Server Error | `500` | "Error interno del servidor. Por favor intente más tarde" |
| Network offline / fetch rejected | `0` | "No fue posible conectar con el servidor. Compruebe su conexión de red" |

---

## 4. Consumer Usage Example

```typescript
import { rawClient, safeRequest } from '@/shared/api/client';
import { isOk } from '@/shared/types/result';

export async function fetchPqrsdfList() {
  const result = await safeRequest(() => rawClient.GET('/api/pqrsdf'));
  if (isOk(result)) {
    return result.value;
  }
  // Handle localized error without throwing
  console.error(`[${result.error.status}] ${result.error.message}`);
  return [];
}
```
