import { Component, input, output } from '@angular/core';

import { Goal } from '../../../core/models/plan';
import { GOALS } from '../wizard-options';

@Component({
  selector: 'app-goal-step',
  styleUrl: './steps.css',
  template: `
    <div class="grid">
      @for (goal of goals; track goal.value) {
        <button
          type="button"
          class="cx-option"
          [attr.aria-pressed]="selected() === goal.value"
          (click)="selectGoal.emit(goal.value)"
        >
          {{ goal.title }}
          <small>{{ goal.hint }}</small>
        </button>
      }
    </div>
  `,
})
export class GoalStep {
  readonly selected = input<Goal | null>(null);
  readonly selectGoal = output<Goal>();

  protected readonly goals = GOALS;
}
