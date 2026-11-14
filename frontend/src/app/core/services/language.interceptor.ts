import { HttpInterceptorFn } from '@angular/common/http';

/**
 * The backend translates names and messages by Accept-Language, and a browser may send
 * English. The UI is Hungarian only for now; the I18nService will set this later.
 */
export const languageInterceptor: HttpInterceptorFn = (request, next) =>
  next(request.clone({ setHeaders: { 'Accept-Language': 'hu' } }));
