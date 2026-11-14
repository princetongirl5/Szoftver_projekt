import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/services/auth.service';
import { languageInterceptor } from './core/services/language.interceptor';
import { HttpPlanService } from './core/services/http-plan.service';
import { PlanService } from './core/services/plan.service';
import { ThemeService } from './core/services/theme.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([languageInterceptor, authInterceptor])),
    provideAppInitializer(() => {
      inject(ThemeService);
    }),
    // Real backend; swap in MockPlanService to run the frontend without it.
    { provide: PlanService, useClass: HttpPlanService },
  ],
};
