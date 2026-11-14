import { Component, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LucideAngularModule, LucideIconData, Moon, Sun } from 'lucide-angular';

import { ThemeService } from '../../../core/services/theme.service';
import { Logo } from '../logo/logo';

export interface NavItem {
  label: string;
  path: string;
  /** Shown in the bottom tab bar on phones. */
  icon?: LucideIconData;
}

// Below 768 px (bp-md) the links move from the top bar to the bottom tab bar.
@Component({
  selector: 'app-top-nav',
  imports: [RouterLink, RouterLinkActive, LucideAngularModule, Logo],
  styles: `
    .cx-tabbar {
      display: none;
    }
    @media (max-width: 767.98px) {
      .cx-topnav {
        gap: var(--space-4);
        padding: 0 var(--space-4);
      }
      .cx-topnav__links,
      .cx-topnav__divider {
        display: none;
      }
      .cx-tabbar {
        display: grid;
        position: fixed;
        inset: auto 0 0 0;
      }
    }
  `,
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

    <nav
      class="cx-tabbar"
      aria-label="Fő menü"
      [style.grid-template-columns]="'repeat(' + items().length + ', minmax(0, 1fr))'"
    >
      @for (item of items(); track item.path) {
        <a class="cx-tab" [routerLink]="item.path" routerLinkActive ariaCurrentWhenActive="page">
          @if (item.icon) {
            <lucide-icon [img]="item.icon" aria-hidden="true" />
          }
          {{ item.label }}
        </a>
      }
    </nav>
  `,
})
export class TopNav {
  readonly items = input.required<NavItem[]>();

  protected readonly theme = inject(ThemeService);
  protected readonly Moon = Moon;
  protected readonly Sun = Sun;
}
