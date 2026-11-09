import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';

import { GeneratePlanRequest, WeekPlan } from '../../core/models/plan';
import { PlanService } from '../../core/services/plan.service';
import { WizardStateService } from './wizard-state.service';

describe('WizardStateService', () => {
  let state: WizardStateService;
  let generate: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.spyOn>;

  const setup = (result = of<WeekPlan>({ days: [] })) => {
    generate = vi.fn((_: GeneratePlanRequest) => result);
    TestBed.configureTestingModule({
      providers: [
        WizardStateService,
        provideRouter([]),
        { provide: PlanService, useValue: { generate } },
      ],
    });
    navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    state = TestBed.inject(WizardStateService);
  };

  // Answers every step validly and stops on the review.
  const fillAll = () => {
    state.setGoal('Strength');
    state.next();
    state.setLocation('Home');
    state.toggleEquipment(1);
    state.next();
    state.toggleDay('Friday');
    state.toggleDay('Monday');
    state.next();
    state.setSessionMinutes(45);
    state.setDifficulty('Easy');
    state.next();
  };

  it('blocks "Tovább" until the step is answered', () => {
    setup();
    expect(state.canGoNext()).toBe(false);
    state.next();
    expect(state.step()).toBe(1);

    state.setGoal('Hypertrophy');
    state.next();
    expect(state.step()).toBe(2);
  });

  it('needs 2-6 days on step 3', () => {
    setup();
    state.goTo(3);
    state.toggleDay('Monday');
    expect(state.canGoNext()).toBe(false);
    state.toggleDay('Tuesday');
    expect(state.canGoNext()).toBe(true);
    for (const day of ['Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'] as const) {
      state.toggleDay(day);
    }
    expect(state.canGoNext()).toBe(false);
  });

  it('goes back but not before step 1', () => {
    setup();
    state.back();
    expect(state.step()).toBe(1);
    state.goTo(4);
    state.back();
    expect(state.step()).toBe(3);
  });

  it('submits the answers with the days in week order', () => {
    setup();
    fillAll();
    expect(state.step()).toBe(5);

    state.submit();

    expect(generate).toHaveBeenCalledWith({
      goal: 'Strength',
      location: 'Home',
      equipmentIds: [1],
      days: ['Monday', 'Friday'],
      sessionMinutes: 45,
      difficulty: 'Easy',
    });
    expect(navigate).toHaveBeenCalledWith('/week');
  });

  it('sends no equipment for the gym', () => {
    setup();
    fillAll();
    state.setLocation('Gym');

    state.submit();

    expect(generate.mock.calls[0][0].equipmentIds).toEqual([]);
  });

  it('reports a failed request', () => {
    setup(throwError(() => new Error('down')));
    fillAll();

    state.submit();

    expect(state.status()).toBe('error');
    expect(state.errorMessage()).toBe('Nem sikerült elkészíteni a tervet. Próbáld újra.');
    expect(state.canGoNext()).toBe(true);
    expect(navigate).not.toHaveBeenCalled();
  });

  it("shows the backend's message for a 400", () => {
    const message = 'A megadott feltételekkel nem található gyakorlat.';
    setup(
      throwError(
        () => new HttpErrorResponse({ status: 400, error: { errors: { exercises: [message] } } }),
      ),
    );
    fillAll();

    state.submit();

    expect(state.errorMessage()).toBe(message);
  });

  it('reset starts over', () => {
    setup();
    fillAll();
    state.submit();

    state.reset();

    expect(state.step()).toBe(1);
    expect(state.answers().goal).toBeNull();
    expect(state.status()).toBe('idle');
  });
});
