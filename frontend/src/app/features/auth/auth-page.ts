import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CircleAlert, LucideAngularModule } from 'lucide-angular';

import { AuthService } from '../../core/services/auth.service';
import { validationMessage } from '../../core/services/validation-message';
import { Logo } from '../../shared/components/logo/logo';

export type AuthMode = 'login' | 'register';

const COPY = {
  login: {
    title: 'Belépés',
    hint: 'Lépj be, és folytasd a heti tervedet.',
    submit: 'Belépés',
    switchText: 'Még nincs fiókod?',
    switchLink: 'Regisztráció',
    switchPath: '/register',
  },
  register: {
    title: 'Regisztráció',
    hint: 'Egy e-mail-cím és egy jelszó elég. A tervedet és a pipáidat a fiókodhoz mentjük.',
    submit: 'Fiók létrehozása',
    switchText: 'Van már fiókod?',
    switchLink: 'Belépés',
    switchPath: '/login',
  },
};

const GENERIC_ERROR = 'Most nem sikerült. Próbáld újra.';

// One page for both routes; the mode comes from the route data.
@Component({
  selector: 'app-auth-page',
  imports: [RouterLink, LucideAngularModule, Logo],
  styles: `
    .body {
      width: min(400px, calc(100% - 2 * var(--space-4)));
      margin: 0 auto;
      padding: var(--space-12) 0;
    }
    form {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
      padding: var(--space-8);
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius-lg);
    }
    .head {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    h1 {
      margin: 0;
      font: 600 28px/34px var(--font-sans);
      letter-spacing: -0.02em;
    }
    .hint,
    .switch {
      margin: 0;
      font: 400 15px/24px var(--font-sans);
      color: var(--text-muted);
    }
    .fields {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    .switch {
      font-size: 13px;
      line-height: 20px;
      text-align: center;
    }
    .switch a {
      color: var(--accent-text);
      font-weight: 500;
    }
    @media (max-width: 767.98px) {
      .cx-topnav {
        padding: 0 var(--space-4);
      }
      .body {
        padding: var(--space-6) 0;
      }
      form {
        padding: var(--space-4);
      }
      h1 {
        font-size: 24px;
        line-height: 30px;
      }
    }
  `,
  template: `
    <header class="cx-topnav">
      <app-logo />
    </header>

    <main class="body">
      <form (submit)="submit($event, email.value, password.value)">
        <div class="head">
          <h1>{{ copy.title }}</h1>
          <p class="hint">{{ copy.hint }}</p>
        </div>

        <div class="fields">
          <div class="cx-field">
            <label for="email">E-mail-cím</label>
            <input #email id="email" class="cx-input" type="email" autocomplete="email" required />
          </div>
          <div class="cx-field">
            <label for="password">Jelszó</label>
            <input
              #password
              id="password"
              class="cx-input"
              type="password"
              required
              [attr.minlength]="mode === 'register' ? 8 : null"
              [attr.autocomplete]="mode === 'register' ? 'new-password' : 'current-password'"
              [attr.aria-describedby]="mode === 'register' ? 'password-hint' : null"
            />
            @if (mode === 'register') {
              <span id="password-hint" class="cx-field__hint">
                Legalább 8 karakter, benne egy számjegy.
              </span>
            }
          </div>
        </div>

        @if (error(); as message) {
          <div class="cx-alert cx-alert--danger" role="alert">
            <lucide-icon [img]="CircleAlert" aria-hidden="true" />
            <span>{{ message }}</span>
          </div>
        }

        <button type="submit" class="cx-btn cx-btn--primary cx-btn--lg" [disabled]="submitting()">
          {{ copy.submit }}
        </button>

        <p class="switch">
          {{ copy.switchText }} <a [routerLink]="copy.switchPath">{{ copy.switchLink }}</a>
        </p>
      </form>
    </main>
  `,
})
export class AuthPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly mode: AuthMode = inject(ActivatedRoute).snapshot.data['mode'];
  protected readonly copy = COPY[this.mode];

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly CircleAlert = CircleAlert;

  submit(event: Event, email: string, password: string): void {
    event.preventDefault();
    if (this.submitting()) return;
    this.submitting.set(true);
    this.error.set(null);

    const request =
      this.mode === 'login'
        ? this.auth.login(email, password)
        : this.auth.register(email, password);
    request.subscribe({
      // A new account has no plan yet, so it starts in the wizard.
      next: () => this.router.navigateByUrl(this.mode === 'login' ? '/week' : '/plan'),
      error: (error: unknown) => {
        this.error.set(this.messageFor(error));
        this.submitting.set(false);
      },
    });
  }

  private messageFor(error: unknown): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Hibás e-mail-cím vagy jelszó.';
    }
    return validationMessage(error) ?? GENERIC_ERROR;
  }
}
