import { firstValueFrom } from 'rxjs';

import { MockPlanService } from './mock-plan.service';

describe('MockPlanService', () => {
  it('returns a full week with 4 workout days', async () => {
    const week = await firstValueFrom(new MockPlanService().getCurrentWeek());

    expect(week.days).toHaveLength(7);
    const workouts = week.days.filter((d) => d.workoutName !== null);
    expect(workouts.map((d) => d.workoutName)).toEqual([
      'Felsőtest A',
      'Alsótest A',
      'Felsőtest B',
      'Alsótest B',
    ]);
    for (const day of week.days) {
      expect(day.exercises.length > 0).toBe(day.workoutName !== null);
    }
    for (const ex of workouts.flatMap((d) => d.exercises)) {
      expect(Object.keys(ex).sort()).toEqual(
        ['isDone', 'muscleGroup', 'name', 'reps', 'sets', 'weightKg'].sort(),
      );
      expect(ex.sets).toBeGreaterThan(0);
      expect(ex.reps).toBeGreaterThan(0);
    }
  });
});
