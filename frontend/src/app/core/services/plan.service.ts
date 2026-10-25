import { Observable } from 'rxjs';

import { GeneratePlanRequest, WeekPlan } from '../models/plan';

// Components inject this; app.config.ts decides which implementation runs.
export abstract class PlanService {
  abstract generate(request: GeneratePlanRequest): Observable<WeekPlan>;
  abstract getCurrentWeek(): Observable<WeekPlan>;
  /** PATCH /api/plans/exercises/{id}: marks one exercise done or not done. */
  abstract setExerciseDone(exerciseId: number, isDone: boolean): Observable<void>;
}
