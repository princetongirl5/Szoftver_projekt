import { InjectionToken } from '@angular/core';

import { DayOfWeek, PlanDay } from '../../core/models/plan';

/** The current time; tests replace it to get a fixed "today". */
export const NOW = new InjectionToken<() => Date>('NOW', {
  providedIn: 'root',
  factory: () => () => new Date(),
});

/** Monday first, like the API. */
export const WEEK_DAYS: { value: DayOfWeek; short: string; label: string }[] = [
  { value: 'Monday', short: 'H', label: 'Hétfő' },
  { value: 'Tuesday', short: 'K', label: 'Kedd' },
  { value: 'Wednesday', short: 'Sze', label: 'Szerda' },
  { value: 'Thursday', short: 'Cs', label: 'Csütörtök' },
  { value: 'Friday', short: 'P', label: 'Péntek' },
  { value: 'Saturday', short: 'Szo', label: 'Szombat' },
  { value: 'Sunday', short: 'V', label: 'Vasárnap' },
];

const MONTHS = [
  'jan.',
  'febr.',
  'márc.',
  'ápr.',
  'máj.',
  'jún.',
  'júl.',
  'aug.',
  'szept.',
  'okt.',
  'nov.',
  'dec.',
];

// muscleGroup is still open (plan-api.md, question 3): the wger categories and the
// MuscleGroup enum are both covered; anything else is shown as it comes.
const MUSCLES: Record<string, string> = {
  Chest: 'Mell',
  Back: 'Hát',
  Legs: 'Comb',
  Shoulders: 'Váll',
  Arms: 'Kar',
  Biceps: 'Bicepsz',
  Triceps: 'Tricepsz',
  Hamstrings: 'Hátsó comb',
  Glutes: 'Farizom',
  Calves: 'Vádli',
  Abs: 'Has',
  UpperBody: 'Felsőtest',
  LowerBody: 'Alsótest',
  Core: 'Törzs',
  Mobility: 'Mobilitás',
  FullBody: 'Teljes test',
};

export const muscleLabel = (group: string): string => MUSCLES[group] ?? group;

/** "Comb, farizom, vádli": the distinct muscle groups of a day, in exercise order. */
export const muscleList = (day: PlanDay): string[] => [
  ...new Set(day.exercises.map((e) => muscleLabel(e.muscleGroup))),
];

export const joinMuscles = (labels: string[]): string =>
  labels.map((l, i) => (i === 0 ? l : l.toLowerCase())).join(', ');

/** 0 = Monday ... 6 = Sunday. */
export const weekdayIndex = (date: Date): number => (date.getDay() + 6) % 7;

/** The 7 dates of the week that contains `date`, Monday first. */
export const weekDates = (date: Date): Date[] => {
  const monday = new Date(date.getFullYear(), date.getMonth(), date.getDate() - weekdayIndex(date));
  return WEEK_DAYS.map(
    (_, i) => new Date(monday.getFullYear(), monday.getMonth(), monday.getDate() + i),
  );
};

/** "10.27" */
export const shortDate = (date: Date): string =>
  `${String(date.getMonth() + 1).padStart(2, '0')}.${String(date.getDate()).padStart(2, '0')}`;

/** "okt. 26 - nov. 1" */
export const weekRange = (dates: Date[]): string => {
  const first = dates[0];
  const last = dates[dates.length - 1];
  return `${MONTHS[first.getMonth()]} ${first.getDate()} - ${MONTHS[last.getMonth()]} ${last.getDate()}`;
};

/** "Upper / Lower" for the 4-day split, "Full body" for 2-3 days, otherwise nothing. */
export const splitName = (days: PlanDay[]): string | null => {
  const names = days.map((d) => d.workoutName).filter((n): n is string => n !== null);
  if (!names.length) return null;
  if (names.every((n) => n.startsWith('Felsőtest') || n.startsWith('Alsótest'))) {
    return 'Upper / Lower';
  }
  if (names.every((n) => n.startsWith('Teljes test'))) return 'Full body';
  return null;
};

export const isComplete = (day: PlanDay): boolean =>
  day.exercises.length > 0 && day.exercises.every((e) => e.isDone);
