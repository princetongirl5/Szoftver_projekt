import { Component, computed, input, output } from '@angular/core';
import { ChevronRight, LucideAngularModule } from 'lucide-angular';

import { Equipment } from '../../../core/models/equipment';
import { WizardAnswers, WizardStep } from '../wizard-state.service';
import { DAYS, GOALS, LEVELS, LOCATIONS } from '../wizard-options';

interface ReviewRow {
  label: string;
  value: string;
  step: WizardStep;
}

@Component({
  selector: 'app-review-step',
  imports: [LucideAngularModule],
  styleUrl: './steps.css',
  styles: `
    .rows {
      display: flex;
      flex-direction: column;
    }
    .row {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-3) 0;
      border: 0;
      border-top: 1px solid var(--border);
      background: none;
      color: var(--text);
      font: 500 14px/20px var(--font-sans);
      text-align: left;
      cursor: pointer;
    }
    .row__label {
      flex: none;
      width: 88px;
      font: 400 13px/20px var(--font-sans);
      color: var(--text-muted);
    }
    .row__value {
      flex: 1;
      min-width: 0;
    }
    .row lucide-icon {
      display: flex;
      flex: none;
      color: var(--text-muted);
    }
  `,
  template: `
    <div class="rows">
      @for (row of rows(); track row.label) {
        <button type="button" class="row" (click)="edit.emit(row.step)">
          <span class="row__label">{{ row.label }}</span>
          <span class="row__value">{{ row.value }}</span>
          <lucide-icon [img]="ChevronRight" [size]="16" aria-hidden="true" />
        </button>
      }
    </div>
  `,
})
export class ReviewStep {
  readonly answers = input.required<WizardAnswers>();
  /** For the equipment names on the Helyszín row. */
  readonly equipment = input<Equipment[]>([]);
  readonly edit = output<WizardStep>();

  protected readonly ChevronRight = ChevronRight;

  protected readonly rows = computed<ReviewRow[]>(() => {
    const a = this.answers();
    const goal = GOALS.find((g) => g.value === a.goal);
    return [
      { label: 'Cél', value: goal ? `${goal.title} · ${goal.short}` : '', step: 1 },
      { label: 'Helyszín', value: this.locationText(a), step: 2 },
      {
        label: 'Napok',
        value: DAYS.filter((d) => a.days.includes(d.value))
          .map((d) => d.label)
          .join(', '),
        step: 3,
      },
      { label: 'Edzés', value: a.sessionMinutes ? `${a.sessionMinutes} perc` : '', step: 4 },
      {
        label: 'Szint',
        value: LEVELS.find((l) => l.value === a.difficulty)?.title ?? '',
        step: 4,
      },
    ];
  });

  private locationText(a: WizardAnswers): string {
    const title = LOCATIONS.find((l) => l.value === a.location)?.title ?? '';
    if (a.location !== 'Home') return title;
    const names = this.equipment()
      .filter((e) => a.equipmentIds.includes(e.id))
      .map((e) => e.name);
    if (!a.equipmentIds.length) return `${title} · Saját testsúly`;
    // Without the equipment list (backend down) only the count is known.
    return `${title} · ${names.length ? names.join(', ') : `${a.equipmentIds.length} eszköz`}`;
  }
}
