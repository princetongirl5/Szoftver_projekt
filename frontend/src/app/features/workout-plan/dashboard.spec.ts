import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';

import { PlanExercise, WeekPlan } from '../../core/models/plan';
import { PlanService } from '../../core/services/plan.service';
import { Dashboard } from './dashboard';
import { NOW } from './week';

const ex = (id: number, name: string, muscleGroup: string, isDone = false): PlanExercise => ({
  id,
  name,
  muscleGroup,
  sets: 3,
  reps: 10,
  weightKg: 40,
  isDone,
});

const WEEK: WeekPlan = {
  days: [
    { day: 'Monday', workoutName: 'Felsőtest A', exercises: [ex(1, 'Bench Press', 'Chest', true)] },
    {
      day: 'Tuesday',
      workoutName: 'Alsótest A',
      exercises: [ex(2, 'Squats', 'Legs'), ex(3, 'Standing Calf Raises', 'Calves')],
    },
    { day: 'Wednesday', workoutName: null, exercises: [] },
    { day: 'Thursday', workoutName: 'Felsőtest B', exercises: [ex(4, 'Pull-ups', 'Back')] },
    { day: 'Friday', workoutName: 'Alsótest B', exercises: [ex(5, 'Leg Curl', 'Hamstrings')] },
    { day: 'Saturday', workoutName: null, exercises: [] },
    { day: 'Sunday', workoutName: null, exercises: [] },
  ],
};

describe('Dashboard', () => {
  let setExerciseDone: ReturnType<typeof vi.fn>;

  const setup = async (
    week: Observable<WeekPlan> = of(structuredClone(WEEK)),
    save: Observable<void> = of(undefined),
  ) => {
    setExerciseDone = vi.fn(() => save);
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        // Tuesday, 27 October 2026.
        { provide: NOW, useValue: () => new Date(2026, 9, 27, 10) },
        { provide: PlanService, useValue: { getCurrentWeek: () => week, setExerciseDone } },
      ],
    });
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    return fixture;
  };

  const text = (el: Element | null) => el?.textContent?.replace(/\s+/g, ' ').trim();

  it('shows the week with today selected', async () => {
    const el: HTMLElement = (await setup()).nativeElement;

    expect(text(el.querySelector('.cx-eyebrow'))).toBe('okt. 26 - nov. 1 · Upper / Lower');
    expect(text(el.querySelector('.week-progress__label .cx-num'))).toBe('1 / 4 edzés');

    const days = [...el.querySelectorAll('.strip .cx-day')];
    expect(days.map((d) => text(d.querySelector('.cx-day__name')))).toEqual([
      'H · 10.26',
      'K · 10.27',
      'Sze · 10.28',
      'Cs · 10.29',
      'P · 10.30',
      'Szo · 10.31',
      'V · 11.01',
    ]);
    expect(text(days[0].querySelector('.cx-badge'))).toBe('Kész');
    expect(text(days[1].querySelector('.cx-badge'))).toBe('Ma');
    expect(days[1].getAttribute('aria-pressed')).toBe('true');

    expect(text(el.querySelector('#workout-title'))).toBe('Alsótest A');
    expect(text(el.querySelector('.workout__meta'))).toBe('2 gyakorlat · Comb, vádli');
    expect(text(el.querySelector('.next'))).toContain('Csütörtök · Felsőtest B');
  });

  it('saves a ticked exercise and counts it', async () => {
    const fixture = await setup();
    const el: HTMLElement = fixture.nativeElement;

    el.querySelector<HTMLButtonElement>('.cx-check')!.click();
    fixture.detectChanges();

    expect(setExerciseDone).toHaveBeenCalledWith(2, true);
    expect(el.querySelector('.cx-check')!.getAttribute('aria-checked')).toBe('true');
    expect(text(el.querySelector('.workout__count'))).toBe('1 / 2');
  });

  it('"Edzés befejezése" ticks the rest and completes the day', async () => {
    const fixture = await setup();
    const el: HTMLElement = fixture.nativeElement;

    el.querySelector<HTMLButtonElement>('.workout__footer button')!.click();
    fixture.detectChanges();

    expect(setExerciseDone.mock.calls).toEqual([
      [2, true],
      [3, true],
    ]);
    expect(text(el.querySelector('.week-progress__label .cx-num'))).toBe('2 / 4 edzés');
    expect(text(el.querySelector('.workout__footer button'))).toBe('Edzés kész');
  });

  it('undoes the tick when saving fails', async () => {
    const fixture = await setup(
      undefined,
      throwError(() => new Error('offline')),
    );
    const el: HTMLElement = fixture.nativeElement;

    el.querySelector<HTMLButtonElement>('.cx-check')!.click();
    fixture.detectChanges();

    expect(el.querySelector('.cx-check')!.getAttribute('aria-checked')).toBe('false');
    expect(text(el.querySelector('[role="alert"]'))).toContain('Nem sikerült menteni');
  });

  it('shows a rest day when one is selected', async () => {
    const fixture = await setup();
    const el: HTMLElement = fixture.nativeElement;

    el.querySelectorAll<HTMLButtonElement>('.strip .cx-day')[2].click();
    fixture.detectChanges();

    expect(text(el.querySelector('#workout-title'))).toBe('Pihenőnap');
    expect(el.querySelector('.exercises')).toBeNull();
  });

  it('sends the user to the wizard when there is no plan yet', async () => {
    const el: HTMLElement = (await setup(throwError(() => new HttpErrorResponse({ status: 404 }))))
      .nativeElement;

    expect(text(el.querySelector('.cx-empty__title'))).toBe('Erre a hétre még nincs terved');
    expect(el.querySelector('.cx-empty a')!.getAttribute('href')).toBe('/plan');
  });
});
