import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import {
  DayOfWeek,
  Difficulty,
  GeneratePlanRequest,
  Goal,
  Location,
  SessionMinutes,
} from '../../core/models/plan';
import { PlanService } from '../../core/services/plan.service';
import { DAYS } from './wizard-options';

export type WizardStep = 1 | 2 | 3 | 4 | 5;

/** What the user has answered so far; null until a step is answered. */
export interface WizardAnswers {
  goal: Goal | null;
  location: Location | null;
  /** Only used with Home; empty means bodyweight only. */
  equipmentIds: number[];
  days: DayOfWeek[];
  sessionMinutes: SessionMinutes | null;
  difficulty: Difficulty | null;
}

export type SubmitStatus = 'idle' | 'submitting' | 'error';

const GENERIC_ERROR = 'Nem sikerült elkészíteni a tervet. Próbáld újra.';

const EMPTY_ANSWERS: WizardAnswers = {
  goal: null,
  location: null,
  equipmentIds: [],
  days: [],
  sessionMinutes: null,
  difficulty: null,
};

// Provided by the PlanWizard page, so every visit starts with fresh answers.
@Injectable()
export class WizardStateService {
  private readonly planService = inject(PlanService);
  private readonly router = inject(Router);

  readonly step = signal<WizardStep>(1);
  readonly answers = signal<WizardAnswers>(EMPTY_ANSWERS);
  readonly status = signal<SubmitStatus>('idle');
  /** Shown on the review while status is 'error'. */
  readonly errorMessage = signal(GENERIC_ERROR);

  /** Whether "Tovább" (or "Terv generálása" on the last step) is enabled. */
  readonly canGoNext = computed(
    () => this.isStepValid(this.step()) && this.status() !== 'submitting',
  );

  setGoal(goal: Goal): void {
    this.update({ goal });
  }

  setLocation(location: Location): void {
    this.update({ location });
  }

  toggleEquipment(equipmentId: number): void {
    const ids = this.answers().equipmentIds;
    this.update({
      equipmentIds: ids.includes(equipmentId)
        ? ids.filter((id) => id !== equipmentId)
        : [...ids, equipmentId],
    });
  }

  toggleDay(day: DayOfWeek): void {
    const days = this.answers().days;
    const next = days.includes(day) ? days.filter((d) => d !== day) : [...days, day];
    // Keep the week order, so the request and the review read Monday first.
    this.update({ days: DAYS.map((d) => d.value).filter((d) => next.includes(d)) });
  }

  setSessionMinutes(minutes: SessionMinutes): void {
    this.update({ sessionMinutes: minutes });
  }

  setDifficulty(difficulty: Difficulty): void {
    this.update({ difficulty });
  }

  next(): void {
    const step = this.step();
    if (step < 5 && this.canGoNext()) this.step.set((step + 1) as WizardStep);
  }

  back(): void {
    const step = this.step();
    if (step > 1) this.step.set((step - 1) as WizardStep);
  }

  /** Jump to a step, used by the review rows. */
  goTo(step: WizardStep): void {
    this.step.set(step);
  }

  /** Start over: "Mégse" and "Kilépés" until there is a page to leave to. */
  reset(): void {
    this.step.set(1);
    this.answers.set(EMPTY_ANSWERS);
    this.status.set('idle');
  }

  /** Sends the answers to PlanService, then opens the new week on the dashboard. */
  submit(): void {
    if (this.step() !== 5 || !this.canGoNext()) return;
    this.status.set('submitting');
    this.planService.generate(this.toRequest()).subscribe({
      next: () => this.router.navigateByUrl('/week'),
      error: (error: unknown) => {
        this.errorMessage.set(validationMessage(error) ?? GENERIC_ERROR);
        this.status.set('error');
      },
    });
  }

  private update(patch: Partial<WizardAnswers>): void {
    this.answers.update((a) => ({ ...a, ...patch }));
    if (this.status() === 'error') this.status.set('idle');
  }

  private isStepValid(step: WizardStep): boolean {
    const a = this.answers();
    switch (step) {
      case 1:
        return a.goal !== null;
      case 2:
        return a.location !== null;
      case 3:
        return a.days.length >= 2 && a.days.length <= 6;
      case 4:
        return a.sessionMinutes !== null && a.difficulty !== null;
      case 5:
        return ([1, 2, 3, 4] as const).every((s) => this.isStepValid(s));
    }
  }

  // Only called after isStepValid(5), so the non-null assertions hold.
  private toRequest(): GeneratePlanRequest {
    const a = this.answers();
    return {
      goal: a.goal!,
      location: a.location!,
      equipmentIds: a.location === 'Home' ? a.equipmentIds : [],
      days: a.days,
      sessionMinutes: a.sessionMinutes!,
      difficulty: a.difficulty!,
    };
  }
}

/** The first message of a 400 ValidationProblem; the backend writes them in Hungarian. */
function validationMessage(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse) || error.status !== 400) return null;
  const errors: Record<string, string[]> | undefined = error.error?.errors;
  return errors ? (Object.values(errors).flat()[0] ?? null) : null;
}
