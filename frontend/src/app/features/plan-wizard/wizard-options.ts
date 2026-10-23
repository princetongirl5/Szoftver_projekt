// Hungarian UI copy for the wizard, shared by the steps and the review.

import { DayOfWeek, Difficulty, Goal, Location, SessionMinutes } from '../../core/models/plan';

export interface Choice<T> {
  value: T;
  title: string;
  hint: string;
}

export const STEPS = [
  {
    label: 'Cél',
    title: 'Mi a célod?',
    hint: 'Ez határozza meg az ismétlésszámot és a pihenőidőt.',
  },
  {
    label: 'Eszközök',
    title: 'Hol edzel?',
    hint: 'Csak olyan gyakorlat kerül a tervbe, amihez megvan az eszközöd.',
  },
  {
    label: 'Napok',
    title: 'Mely napokon érsz rá?',
    hint: 'Válassz 2-6 napot. A tervező ebből választja ki a felosztást.',
  },
  {
    label: 'Edzés',
    title: 'Milyen hosszú egy edzés?',
    hint: 'A gyakorlatok és sorozatok száma ehhez igazodik.',
  },
  {
    label: 'Összegzés',
    title: 'Minden stimmel?',
    hint: 'Koppints egy sorra, ha valamit módosítanál.',
  },
];

export const GOALS: (Choice<Goal> & { short: string })[] = [
  { value: 'Strength', title: 'Erő', hint: '3-6 ismétlés, hosszú pihenők', short: '3-6 ismétlés' },
  {
    value: 'Hypertrophy',
    title: 'Izomépítés',
    hint: '8-12 ismétlés, több sorozat',
    short: '8-12 ismétlés',
  },
  {
    value: 'Endurance',
    title: 'Állóképesség',
    hint: '15-20 ismétlés, rövid pihenők',
    short: '15-20 ismétlés',
  },
  {
    value: 'General',
    title: 'Általános fittség',
    hint: 'Vegyes, kiegyensúlyozott',
    short: 'Vegyes',
  },
];

export const LOCATIONS: Choice<Location>[] = [
  { value: 'Gym', title: 'Edzőterem', hint: 'Rúd, gépek, csigák, kézisúlyzók' },
  { value: 'Home', title: 'Otthon', hint: 'Saját testsúly és a megadott eszközök' },
];

export const DAYS: { value: DayOfWeek; label: string }[] = [
  { value: 'Monday', label: 'Hétfő' },
  { value: 'Tuesday', label: 'Kedd' },
  { value: 'Wednesday', label: 'Szerda' },
  { value: 'Thursday', label: 'Csütörtök' },
  { value: 'Friday', label: 'Péntek' },
  { value: 'Saturday', label: 'Szombat' },
  { value: 'Sunday', label: 'Vasárnap' },
];

export const SESSION_MINUTES: SessionMinutes[] = [45, 60, 75, 90];

export const LEVELS: Choice<Difficulty>[] = [
  { value: 'Easy', title: 'Kezdő', hint: '6 hónap alatt' },
  { value: 'Medium', title: 'Haladó', hint: '6 hónap - 2 év' },
  { value: 'Hard', title: 'Profi', hint: '2 év felett' },
];

/** The split shown on step 3; the backend decides the real one. */
export function splitLabel(dayCount: number): string | null {
  if (dayCount >= 2 && dayCount <= 3) return 'Teljes test';
  if (dayCount === 4) return 'Felsőtest / Alsótest';
  if (dayCount >= 5 && dayCount <= 6) return 'Izomcsoportonként';
  return null;
}
