/**
 * Zero-dependency Result monad implementation for operational error governance.
 * Strictly adheres to Constitution v1.3.0 Principle V (prohibiting exceptions for normal flow).
 */

export type Ok<T> = {
  readonly ok: true;
  readonly value: T;
};

export type Err<E> = {
  readonly ok: false;
  readonly error: E;
};

export type Result<T, E = Error> = Ok<T> | Err<E>;

/**
 * Creates a successful Result wrapping a value.
 */
export const ok = <T>(value: T): Ok<T> => ({
  ok: true,
  value,
});

/**
 * Creates a failed Result wrapping an error.
 */
export const err = <E>(error: E): Err<E> => ({
  ok: false,
  error,
});

/**
 * Type guard returning true if the Result is Ok.
 */
export const isOk = <T, E>(result: Result<T, E>): result is Ok<T> => result.ok;

/**
 * Type guard returning true if the Result is Err.
 */
export const isErr = <T, E>(result: Result<T, E>): result is Err<E> => !result.ok;
