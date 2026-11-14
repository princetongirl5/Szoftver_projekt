import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Observable, catchError, finalize, map, of, switchMap, tap, throwError } from 'rxjs';

export interface AuthUser {
  email: string;
}

// ASP.NET Core Identity with a cookie (docs: MapIdentityApi). The browser sends the
// cookie by itself, since the dev server proxies /api to the same origin.
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  /** undefined until the first check, null when nobody is signed in. */
  readonly user = signal<AuthUser | null | undefined>(undefined);

  /** Asks the backend who owns the cookie; any error counts as signed out. */
  check(): Observable<AuthUser | null> {
    return this.http.get<AuthUser>('/api/auth/manage/info').pipe(
      map((info): AuthUser | null => ({ email: info.email })),
      catchError(() => of(null)),
      tap((user) => this.user.set(user)),
    );
  }

  /** A wrong email or password fails with 401. */
  login(email: string, password: string): Observable<AuthUser | null> {
    return this.http
      .post<void>('/api/auth/login?useCookies=true', { email, password })
      .pipe(switchMap(() => this.check()));
  }

  /** Register does not sign in, so a login follows. Bad input fails with a 400 ValidationProblem. */
  register(email: string, password: string): Observable<AuthUser | null> {
    return this.http
      .post<void>('/api/auth/register', { email, password })
      .pipe(switchMap(() => this.login(email, password)));
  }

  /** Signed out locally even if the request fails. */
  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', null).pipe(finalize(() => this.user.set(null)));
  }
}

/** Lets only a signed-in user in; the first visit asks the backend once. */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const toLogin = inject(Router).createUrlTree(['/login']);
  const user = auth.user();
  if (user !== undefined) return user ? true : toLogin;
  return auth.check().pipe(map((checked) => (checked ? true : toLogin)));
};

/** A 401 from the API outside /api/auth means the session ended: back to the login page. */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return next(request).pipe(
    catchError((error: unknown) => {
      if (
        error instanceof HttpErrorResponse &&
        error.status === 401 &&
        !request.url.startsWith('/api/auth/')
      ) {
        auth.user.set(null);
        router.navigateByUrl('/login');
      }
      return throwError(() => error);
    }),
  );
};
