import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { GeneratePlanRequest, WeekPlan } from '../models/plan';
import { PlanService } from './plan.service';

// The real backend; endpoints are in docs/api/plan-api.md.
@Injectable()
export class HttpPlanService extends PlanService {
  private readonly http = inject(HttpClient);

  generate(request: GeneratePlanRequest): Observable<WeekPlan> {
    return this.http.post<WeekPlan>('/api/plans/generate', request);
  }

  getCurrentWeek(): Observable<WeekPlan> {
    return this.http.get<WeekPlan>('/api/plans/current');
  }

  /** 404 until the user has generated a plan. */
  getPreferences(): Observable<GeneratePlanRequest> {
    return this.http.get<GeneratePlanRequest>('/api/plans/preferences');
  }

  setExerciseDone(exerciseId: number, isDone: boolean): Observable<void> {
    return this.http.patch<void>(`/api/plans/exercises/${exerciseId}`, { isDone });
  }

  logActual(
    exerciseId: number,
    actualReps: number,
    actualWeightKg: number | null,
  ): Observable<void> {
    return this.http.patch<void>(`/api/plans/exercises/${exerciseId}`, {
      isDone: true,
      actualReps,
      actualWeightKg,
    });
  }
}
