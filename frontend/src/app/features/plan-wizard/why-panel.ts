import { Component, computed, input } from '@angular/core';
import { Image, LucideAngularModule } from 'lucide-angular';

import { WizardStep } from './wizard-state.service';

const PANELS = [
  {
    image: 'Kép: célok',
    heading: 'A cél dönti el az ismétlésszámot',
    text: 'Erőnél kevés ismétlés és hosszú pihenő kell, izomépítésnél 8-12 ismétlés több sorozatban. Ugyanaz a gyakorlat más célra máshogy épül fel.',
  },
  {
    image: 'Kép: otthoni eszközök',
    heading: 'Csak olyat kapsz, amit meg tudsz csinálni',
    text: 'Ha nincs rudad, a tervező kézisúlyzós vagy testsúlyos változatot választ ugyanarra az izomcsoportra.',
  },
  {
    image: 'Kép: heti felosztás',
    heading: 'A napokból lesz a felosztás',
    text: '2-3 napnál teljes test, 4-6 napnál felső és alsó test váltja egymást, így két egymást követő napon más izmok dolgoznak.',
  },
  {
    image: 'Kép: edzés közben',
    heading: 'Az idő és a szint adja a mennyiséget',
    text: '60 percbe általában 5 gyakorlat fér bele. Kezdőként kevesebb sorozattal indulsz.',
  },
  {
    image: 'Kép: kész terv',
    heading: 'Bármikor újratervezheted',
    text: 'Ha változik a heted vagy új eszközöd lesz, a Tervezés menüpontban új tervet kérhetsz. Az új terv a mostani hét helyére kerül.',
  },
];

@Component({
  selector: 'app-why-panel',
  imports: [LucideAngularModule],
  styles: `
    :host {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    h2 {
      margin: 0;
      font: 600 18px/26px var(--font-sans);
      letter-spacing: -0.01em;
    }
    p {
      margin: 0;
      font: 400 15px/24px var(--font-sans);
      color: var(--text-muted);
    }
  `,
  template: `
    @let panel = current();
    <!-- Image slot: the pictures are not chosen yet. -->
    <div class="cx-media">
      <span class="cx-media__empty">
        <lucide-icon [img]="Image" aria-hidden="true" />
        {{ panel.image }}
      </span>
    </div>
    <div>
      <div class="cx-eyebrow">Miért kérdezzük?</div>
      <h2>{{ panel.heading }}</h2>
    </div>
    <p>{{ panel.text }}</p>
  `,
})
export class WhyPanel {
  readonly step = input.required<WizardStep>();

  protected readonly current = computed(() => PANELS[this.step() - 1]);
  protected readonly Image = Image;
}
