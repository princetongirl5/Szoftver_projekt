import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { Observable, firstValueFrom } from 'rxjs';

import { AuthService, authGuard, authInterceptor } from './auth.service';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  const runGuard = () =>
    TestBed.runInInjectionContext(() =>
      authGuard({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as Observable<boolean | UrlTree> | boolean | UrlTree;

  it('logs in with a cookie, then loads the user', async () => {
    const result = firstValueFrom(auth.login('a@b.hu', 'jelszo123'));

    const login = http.expectOne('/api/auth/login?useCookies=true');
    expect(login.request.body).toEqual({ email: 'a@b.hu', password: 'jelszo123' });
    login.flush(null);
    http.expectOne('/api/auth/manage/info').flush({ email: 'a@b.hu', isEmailConfirmed: false });

    expect(await result).toEqual({ email: 'a@b.hu' });
    expect(auth.user()).toEqual({ email: 'a@b.hu' });
  });

  it('sends a signed-out visitor to /login, asking the backend only once', async () => {
    const first = firstValueFrom(runGuard() as Observable<boolean | UrlTree>);
    http.expectOne('/api/auth/manage/info').flush(null, { status: 401, statusText: '' });

    const toLogin = TestBed.inject(Router).createUrlTree(['/login']);
    expect(await first).toEqual(toLogin);
    expect(auth.user()).toBeNull();
    expect(runGuard()).toEqual(toLogin);
  });

  it('lets a signed-in user in', () => {
    auth.user.set({ email: 'a@b.hu' });
    expect(runGuard()).toBe(true);
  });

  it('goes to /login when an API call answers 401', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    auth.user.set({ email: 'a@b.hu' });

    TestBed.inject(HttpClient)
      .get('/api/plans/current')
      .subscribe({ error: () => {} });
    http.expectOne('/api/plans/current').flush(null, { status: 401, statusText: '' });

    expect(navigate).toHaveBeenCalledWith('/login');
    expect(auth.user()).toBeNull();
  });
});
