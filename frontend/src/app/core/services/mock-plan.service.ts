import { Injectable } from '@angular/core';
import { Observable, delay, of } from 'rxjs';

import { GeneratePlanRequest, PlanExercise, WeekPlan } from '../models/plan';
import { PlanService } from './plan.service';

const ex = (
  name: string,
  muscleGroup: string,
  sets: number,
  reps: number,
  weightKg: number | null,
): PlanExercise => ({ name, muscleGroup, sets, reps, weightKg, isDone: false });

const MOCK_WEEK: WeekPlan = {
  days: [
    {
      day: 'Monday',
      workoutName: 'Felsőtest A',
      exercises: [
        ex('Bench Press', 'Chest', 4, 8, 60),
        ex('Bent Over Rowing', 'Back', 4, 8, 50),
        ex('Shoulder Press, Dumbbells', 'Shoulders', 3, 10, 16),
        ex('Biceps Curls With Dumbbell', 'Biceps', 3, 12, 12),
        ex('Triceps Pushdown', 'Triceps', 3, 12, 25),
      ],
    },
    {
      day: 'Tuesday',
      workoutName: 'Alsótest A',
      exercises: [
        ex('Squats', 'Legs', 4, 8, 80),
        ex('Romanian Deadlift', 'Hamstrings', 3, 10, 60),
        ex('Leg Press', 'Legs', 3, 12, 120),
        ex('Standing Calf Raises', 'Calves', 4, 15, 40),
        ex('Plank', 'Abs', 3, 45, null),
      ],
    },
    { day: 'Wednesday', workoutName: null, exercises: [] },
    {
      day: 'Thursday',
      workoutName: 'Felsőtest B',
      exercises: [
        ex('Incline Dumbbell Press', 'Chest', 4, 10, 22),
        ex('Pull-ups', 'Back', 4, 8, null),
        ex('Lateral Raises', 'Shoulders', 3, 15, 8),
        ex('Hammer Curls', 'Biceps', 3, 12, 14),
        ex('Dips', 'Triceps', 3, 10, null),
      ],
    },
    {
      day: 'Friday',
      workoutName: 'Alsótest B',
      exercises: [
        ex('Deadlifts', 'Back', 4, 6, 100),
        ex('Bulgarian Split Squat', 'Legs', 3, 10, 16),
        ex('Leg Curl', 'Hamstrings', 3, 12, 35),
        ex('Hip Thrust', 'Glutes', 3, 10, 70),
        ex('Crunches', 'Abs', 3, 20, null),
      ],
    },
    { day: 'Saturday', workoutName: null, exercises: [] },
    { day: 'Sunday', workoutName: null, exercises: [] },
  ],
};

@Injectable()
export class MockPlanService extends PlanService {
  // Ignores the request and always returns the same week; the real plan comes from the backend.
  generate(_request: GeneratePlanRequest): Observable<WeekPlan> {
    return this.getCurrentWeek();
  }

  getCurrentWeek(): Observable<WeekPlan> {
    return of(structuredClone(MOCK_WEEK)).pipe(delay(400));
  }
}
