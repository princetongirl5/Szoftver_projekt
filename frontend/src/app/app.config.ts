import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
} from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { MockPlanService } from './core/services/mock-plan.service';
import { PlanService } from './core/services/plan.service';
import { ThemeService } from './core/services/theme.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(),
    provideAppInitializer(() => {
      inject(ThemeService);
    }),
    // Mock or real backend: swap MockPlanService for HttpPlanService here.
    { provide: PlanService, useClass: MockPlanService },
  ],
};
