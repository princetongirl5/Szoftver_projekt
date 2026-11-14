// Mirrors the backend StatsDtos (GET /api/stats, /api/stats/exercises).

/** Rest: no workout; Planned: today or later, not done yet; the rest are past workouts. */
export type DayStatus = 'Done' | 'Partial' | 'Missed' | 'Rest' | 'Planned';

export interface StatsWeek {
  /** "2026-11-09", a Monday. */
  weekStart: string;
  completed: number;
  /** Past workouts only; days still ahead are not counted. */
  planned: number;
  /** Monday first. */
  days: DayStatus[];
}

export interface Stats {
  from: string;
  to: string;
  weeks: number;
  currentStreakWeeks: number;
  longestStreakWeeks: number;
  completedWorkouts: number;
  plannedWorkouts: number;
  completionPercent: number;
  /** Oldest week first. */
  log: StatsWeek[];
}

export interface ExerciseStats {
  exerciseId: number;
  name: string;
  timesPlanned: number;
  timesDone: number;
}

/** One time the exercise was in a plan; the actual values are null when not given. */
export interface ExerciseHistoryEntry {
  date: string;
  sets: number;
  reps: number;
  weightKg: number | null;
  actualReps: number | null;
  actualWeightKg: number | null;
  isDone: boolean;
}
