# Interface Contract: Result Pattern

**Location**: `src/shared/types/result.ts`  
**Purpose**: Standardize explicit return types for operations that can fail without throwing exceptions.

---

## 1. Type Signatures

```typescript
export type Ok<T> = {
  readonly ok: true;
  readonly value: T;
};

export type Err<E> = {
  readonly ok: false;
  readonly error: E;
};

export type Result<T, E = Error> = Ok<T> | Err<E>;
```

---

## 2. Helper Functions Contract

### `ok<T>(value: T): Ok<T>`
- **Description**: Wraps a successful value into an `Ok<T>` result object.
- **Parameters**: `value` of type `T`.
- **Returns**: `{ ok: true, value }`.

### `err<E>(error: E): Err<E>`
- **Description**: Wraps an error value into an `Err<E>` result object.
- **Parameters**: `error` of type `E`.
- **Returns**: `{ ok: false, error }`.

### `isOk<T, E>(result: Result<T, E>): result is Ok<T>`
- **Description**: TypeScript custom type guard checking if result succeeded.
- **Parameters**: `result` of type `Result<T, E>`.
- **Returns**: `true` if `result.ok === true`, narrowing `result` to `Ok<T>`.

### `isErr<T, E>(result: Result<T, E>): result is Err<E>`
- **Description**: TypeScript custom type guard checking if result failed.
- **Parameters**: `result` of type `Result<T, E>`.
- **Returns**: `true` if `result.ok === false`, narrowing `result` to `Err<E>`.

---

## 3. Usage Pattern Example

```typescript
import { ok, err, isOk, Result } from '@/shared/types/result';

async function calculateTotal(items: string[]): Promise<Result<number, string>> {
  if (items.length === 0) {
    return err('La lista de elementos no puede estar vacía');
  }
  return ok(items.length * 100);
}

// Consumer code
const result = await calculateTotal(['ticket-1']);
if (isOk(result)) {
  console.log('Total:', result.value);
} else {
  console.error('Error:', result.error);
}
```
