import { Component, computed, input, output } from '@angular/core';

import { DayOfWeek } from '../../../core/models/plan';
import { DAYS, splitLabel } from '../wizard-options';

@Component({
  selector: 'app-days-step',
  styleUrl: './steps.css',
  styles: `
    .days {
      display: grid;
      grid-template-columns: repeat(7, minmax(0, 1fr));
      gap: var(--space-2);
    }
    .days .cx-chip {
      min-height: 56px;
      justify-content: center;
      padding: 0 var(--space-1);
    }
    .summary {
      display: flex;
      justify-content: space-between;
      gap: var(--space-4);
      margin: 0;
      font: 400 13px/20px var(--font-sans);
      color: var(--text-muted);
    }
    .summary strong {
      font: 500 14px/20px var(--font-sans);
      color: var(--accent-text);
    }
  `,
  template: `
    <div class="days">
      @for (day of days; track day.value) {
        <button
          type="button"
          class="cx-chip"
          [attr.aria-pressed]="selected().includes(day.value)"
          (click)="toggleDay.emit(day.value)"
        >
          {{ day.label }}
        </button>
      }
    </div>
    <p class="summary">
      <span>{{ selected().length }} nap kiválasztva</span>
      @if (split(); as split) {
        <strong>Felosztás: {{ split }}</strong>
      }
    </p>
  `,
})
export class DaysStep {
  readonly selected = input<DayOfWeek[]>([]);
  readonly toggleDay = output<DayOfWeek>();

  protected readonly days = DAYS;
  protected readonly split = computed(() => splitLabel(this.selected().length));
}
