import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'plan' },
  {
    path: 'plan',
    loadComponent: () => import('./features/plan-wizard/plan-wizard').then((m) => m.PlanWizard),
  },
];
