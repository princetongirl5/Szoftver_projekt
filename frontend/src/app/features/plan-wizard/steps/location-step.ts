import { Component, input, output } from '@angular/core';
import { CircleAlert, LucideAngularModule } from 'lucide-angular';

import { Equipment } from '../../../core/models/equipment';
import { Location } from '../../../core/models/plan';
import { LOCATIONS } from '../wizard-options';

@Component({
  selector: 'app-location-step',
  imports: [LucideAngularModule],
  styleUrl: './steps.css',
  template: `
    <div class="grid">
      @for (location of locations; track location.value) {
        <button
          type="button"
          class="cx-option"
          [attr.aria-pressed]="selected() === location.value"
          (click)="selectLocation.emit(location.value)"
        >
          {{ location.title }}
          <small>{{ location.hint }}</small>
        </button>
      }
    </div>

    @if (selected() === 'Home') {
      <div class="group">
        <p class="label">Milyen eszközeid vannak otthon?</p>
        @if (equipmentError()) {
          <div class="cx-alert cx-alert--danger" role="alert">
            <lucide-icon [img]="CircleAlert" aria-hidden="true" />
            <span>Az eszközlista most nem érhető el. Próbáld újra később.</span>
          </div>
        } @else if (equipmentLoading()) {
          <p class="cx-muted">Eszközök betöltése…</p>
        } @else {
          <div class="chips">
            @for (item of equipment(); track item.id) {
              <button
                type="button"
                class="cx-chip"
                [attr.aria-pressed]="equipmentIds().includes(item.id)"
                (click)="toggleEquipment.emit(item.id)"
              >
                {{ item.name }}
              </button>
            }
          </div>
        }
      </div>
    }
  `,
})
export class LocationStep {
  readonly selected = input<Location | null>(null);
  readonly equipment = input<Equipment[]>([]);
  readonly equipmentIds = input<number[]>([]);
  readonly equipmentLoading = input(false);
  readonly equipmentError = input(false);

  readonly selectLocation = output<Location>();
  readonly toggleEquipment = output<number>();

  protected readonly locations = LOCATIONS;
  protected readonly CircleAlert = CircleAlert;
}
