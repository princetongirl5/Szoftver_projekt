// The API sends enums as text, so these are string unions, not TS enums.
// Proposal for POST /api/plans/generate; aligned to the backend on Sunday.

export type Goal = 'Strength' | 'Hypertrophy' | 'Endurance' | 'General';
export type Location = 'Gym' | 'Home';
export type DayOfWeek =
  'Monday' | 'Tuesday' | 'Wednesday' | 'Thursday' | 'Friday' | 'Saturday' | 'Sunday';
export type SessionMinutes = 45 | 60 | 75 | 90;
export type Difficulty = 'Easy' | 'Medium' | 'Hard';

export interface GeneratePlanRequest {
  goal: Goal;
  location: Location;
  /** Only for Home. */
  equipmentIds: number[];
  /** 2-6 days. */
  days: DayOfWeek[];
  sessionMinutes: SessionMinutes;
  difficulty: Difficulty;
}

export interface PlanExercise {
  /** The WorkoutExercise id, used by PATCH /api/plans/exercises/{id}. */
  id: number;
  /** As it comes from wger, mostly English. */
  name: string;
  muscleGroup: string;
  sets: number;
  reps: number;
  /** null for bodyweight exercises. */
  weightKg: number | null;
  isDone: boolean;
}

export interface PlanDay {
  day: DayOfWeek;
  /** null on a rest day. */
  workoutName: string | null;
  exercises: PlanExercise[];
}

export interface WeekPlan {
  days: PlanDay[];
}
