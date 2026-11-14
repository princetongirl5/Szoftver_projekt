import { Routes } from '@angular/router';

import { authGuard } from './core/services/auth.service';

const authPage = () => import('./features/auth/auth-page').then((m) => m.AuthPage);

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'plan' },
  { path: 'login', loadComponent: authPage, data: { mode: 'login' } },
  { path: 'register', loadComponent: authPage, data: { mode: 'register' } },
  {
    path: 'plan',
    canActivate: [authGuard],
    loadComponent: () => import('./features/plan-wizard/plan-wizard').then((m) => m.PlanWizard),
  },
  {
    path: 'week',
    canActivate: [authGuard],
    loadComponent: () => import('./features/workout-plan/dashboard').then((m) => m.Dashboard),
  },
];
