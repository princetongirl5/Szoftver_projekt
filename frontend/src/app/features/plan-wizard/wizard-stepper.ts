import { Component, input } from '@angular/core';
import { Check, LucideAngularModule } from 'lucide-angular';

import { STEPS } from './wizard-options';
import { WizardStep } from './wizard-state.service';

@Component({
  selector: 'app-wizard-stepper',
  imports: [LucideAngularModule],
  template: `
    <ol class="cx-stepper" aria-label="Lépések">
      @for (step of steps; track step.label; let i = $index, last = $last) {
        <li
          class="cx-step"
          [class.cx-step--done]="i + 1 < current()"
          [attr.aria-current]="i + 1 === current() ? 'step' : null"
        >
          <span class="cx-step__dot">
            @if (i + 1 < current()) {
              <lucide-icon [img]="Check" [size]="12" aria-hidden="true" />
            } @else {
              {{ i + 1 }}
            }
          </span>
          {{ step.label }}
        </li>
        @if (!last) {
          <li class="cx-step__line" aria-hidden="true"></li>
        }
      }
    </ol>
  `,
})
export class WizardStepper {
  readonly current = input.required<WizardStep>();

  protected readonly steps = STEPS;
  protected readonly Check = Check;
}
