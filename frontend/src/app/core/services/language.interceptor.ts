import { HttpInterceptorFn } from '@angular/common/http';

/**
 * The backend translates names and messages by Accept-Language, and a browser may send
 * English. The UI is Hungarian only (the team dropped English on 2026-11-14).
 */
export const languageInterceptor: HttpInterceptorFn = (request, next) =>
  next(request.clone({ setHeaders: { 'Accept-Language': 'hu' } }));
