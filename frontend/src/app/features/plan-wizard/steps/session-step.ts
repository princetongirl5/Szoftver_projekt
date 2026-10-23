import { Component, input, output } from '@angular/core';

import { Difficulty, SessionMinutes } from '../../../core/models/plan';
import { LEVELS, SESSION_MINUTES } from '../wizard-options';

@Component({
  selector: 'app-session-step',
  styleUrl: './steps.css',
  template: `
    <div class="group">
      <p class="label">Egy edzés hossza</p>
      <div class="chips">
        @for (minutes of sessionMinutes; track minutes) {
          <button
            type="button"
            class="cx-chip"
            [attr.aria-pressed]="minutes === selectedMinutes()"
            (click)="selectMinutes.emit(minutes)"
          >
            {{ minutes }} perc
          </button>
        }
      </div>
    </div>

    <div class="group">
      <p class="label">Tapasztalat</p>
      <div class="grid grid--3">
        @for (level of levels; track level.value) {
          <button
            type="button"
            class="cx-option"
            [attr.aria-pressed]="level.value === selectedDifficulty()"
            (click)="selectDifficulty.emit(level.value)"
          >
            {{ level.title }}
            <small>{{ level.hint }}</small>
          </button>
        }
      </div>
    </div>
  `,
})
export class SessionStep {
  readonly selectedMinutes = input<SessionMinutes | null>(null);
  readonly selectedDifficulty = input<Difficulty | null>(null);

  readonly selectMinutes = output<SessionMinutes>();
  readonly selectDifficulty = output<Difficulty>();

  protected readonly sessionMinutes = SESSION_MINUTES;
  protected readonly levels = LEVELS;
}
