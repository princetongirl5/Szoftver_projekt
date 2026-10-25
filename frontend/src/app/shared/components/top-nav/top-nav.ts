import { Component, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, Moon, Sun } from 'lucide-angular';

import { ThemeService } from '../../../core/services/theme.service';
import { Logo } from '../logo/logo';

export interface NavItem {
  label: string;
  path: string;
}

@Component({
  selector: 'app-top-nav',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule, Logo],
  template: `
    <header class="cx-topnav">
      <app-logo />
      <nav class="cx-topnav__links" aria-label="Fő menü">
        @for (item of items(); track item.path) {
          <a
            class="cx-navlink"
            [routerLink]="item.path"
            routerLinkActive
            ariaCurrentWhenActive="page"
            >{{ item.label }}</a
          >
        }
      </nav>
      <span class="cx-topnav__divider" aria-hidden="true"></span>
      @let dark = theme.theme() === 'dark';
      <button
        type="button"
        class="cx-theme-toggle"
        [attr.aria-label]="dark ? 'Világos téma bekapcsolása' : 'Sötét téma bekapcsolása'"
        (click)="theme.toggle()"
      >
        <lucide-icon [img]="dark ? Sun : Moon" aria-hidden="true" />
      </button>
    </header>
  `,
})
export class TopNav {
  readonly items = input.required<NavItem[]>();

  protected readonly theme = inject(ThemeService);
  protected readonly Moon = Moon;
  protected readonly Sun = Sun;
}
