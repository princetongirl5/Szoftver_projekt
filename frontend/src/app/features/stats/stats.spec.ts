import { ExerciseHistoryEntry } from '../../core/models/stats';
import { formatNumber, monthDay, parseDate, seriesOf, sparkline } from './stats';

const entry = (patch: Partial<ExerciseHistoryEntry>): ExerciseHistoryEntry => ({
  date: '2026-11-09T00:00:00',
  sets: 3,
  reps: 10,
  weightKg: null,
  actualReps: null,
  actualWeightKg: null,
  isDone: true,
  ...patch,
});

describe('stats helpers', () => {
  it('reads API dates as local days', () => {
    expect(monthDay(parseDate('2026-11-09T00:00:00'))).toBe('nov. 9');
    expect(monthDay(parseDate('2026-10-19'))).toBe('okt. 19');
  });

  it('formats numbers the Hungarian way', () => {
    expect(formatNumber(7.5)).toBe('7,5');
    expect(formatNumber(12)).toBe('12');
  });

  it('draws the weight of done sessions, the actual value first', () => {
    const series = seriesOf([
      entry({ weightKg: 5 }),
      entry({ weightKg: 5, actualWeightKg: 7.5 }),
      entry({ weightKg: 5, isDone: false }),
    ]);
    expect(series).toEqual({ unit: 'kg', values: [5, 7.5] });
  });

  it('draws the reps of a bodyweight exercise', () => {
    expect(seriesOf([entry({}), entry({ actualReps: 12 })])).toEqual({
      unit: 'ism.',
      values: [10, 12],
    });
  });

  it('puts the highest value on top and keeps a flat line in the middle', () => {
    expect(sparkline([], 100, 48)).toBeNull();
    expect(sparkline([5, 10], 100, 48)?.points).toBe('4.0,44.0 96.0,4.0');
    expect(sparkline([5, 5], 100, 48)?.points).toBe('4.0,24.0 96.0,24.0');
    expect(sparkline([7], 100, 48)?.last).toEqual({ x: 50, y: 24 });
  });
});
