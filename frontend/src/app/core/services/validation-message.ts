import { HttpErrorResponse } from '@angular/common/http';

/** The first message of a 400 ValidationProblem; the backend writes them in the request's language. */
export function validationMessage(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) return null;
  const errors: Record<string, string[]> | undefined = error.error?.errors;
  return errors ? (Object.values(errors).flat()[0] ?? null) : null;
}
