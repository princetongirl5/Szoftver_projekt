import { ExerciseHistoryEntry } from '../../core/models/stats';

/** "2026-11-09" or "2026-11-09T00:00:00" as a local date (no time zone shift). */
export const parseDate = (value: string): Date => {
  const [y, m, d] = value.slice(0, 10).split('-').map(Number);
  return new Date(y, m - 1, d);
};

const MONTH_DAY = new Intl.DateTimeFormat('hu-HU', { month: 'short', day: 'numeric' });

/** "nov. 9" (Intl writes "nov. 9.", the dashboard has no closing dot). */
export const monthDay = (date: Date): string => MONTH_DAY.format(date).replace(/\.$/, '');

/** "7,5" */
export const formatNumber = (value: number): string =>
  value.toLocaleString('hu-HU', { maximumFractionDigits: 1 });

export interface Series {
  unit: 'kg' | 'ism.';
  /** One value per done session, oldest first. */
  values: number[];
}

/**
 * What to draw for one exercise: the weight if it ever had one, otherwise the reps.
 * Only done sessions count, and the actual value wins over the planned one.
 */
export const seriesOf = (history: ExerciseHistoryEntry[]): Series => {
  const weighted = history.some((h) => h.weightKg !== null || h.actualWeightKg !== null);
  const done = history.filter((h) => h.isDone);
  return weighted
    ? { unit: 'kg', values: done.map((h) => h.actualWeightKg ?? h.weightKg ?? 0) }
    : { unit: 'ism.', values: done.map((h) => h.actualReps ?? h.reps) };
};

/** SVG polyline points for a sparkline in a width x height box, plus the last point. */
export const sparkline = (
  values: number[],
  width: number,
  height: number,
): { points: string; last: { x: number; y: number } } | null => {
  if (!values.length) return null;
  const pad = 4;
  const min = Math.min(...values);
  const span = Math.max(...values) - min || 1;
  const step = values.length > 1 ? (width - 2 * pad) / (values.length - 1) : 0;
  const xy = values.map((v, i) => ({
    x: values.length > 1 ? pad + i * step : width / 2,
    // A flat line sits in the middle.
    y:
      Math.max(...values) === min
        ? height / 2
        : height - pad - ((v - min) / span) * (height - 2 * pad),
  }));
  return {
    points: xy.map((p) => `${p.x.toFixed(1)},${p.y.toFixed(1)}`).join(' '),
    last: xy[xy.length - 1],
  };
};
