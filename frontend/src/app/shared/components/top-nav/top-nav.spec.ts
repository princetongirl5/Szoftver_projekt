import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';

import { NavItem, TopNav } from './top-nav';

@Component({ template: '' })
class Blank {}

@Component({
  imports: [TopNav],
  template: '<app-top-nav [items]="items" />',
})
class Host {
  items: NavItem[] = [
    { label: 'Tervezés', path: '/plan' },
    { label: 'Heti terv', path: '/week' },
  ];
}

describe('TopNav', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([
          { path: 'plan', component: Host },
          { path: 'week', component: Blank },
        ]),
      ],
    });
  });

  it('renders the given items and marks the active one', async () => {
    const harness = await RouterTestingHarness.create('/plan');
    const links = [...harness.routeNativeElement!.querySelectorAll('a.cx-navlink')];

    expect(links.map((a) => a.textContent?.trim())).toEqual(['Tervezés', 'Heti terv']);
    expect(links[0].getAttribute('aria-current')).toBe('page');
    expect(links[1].hasAttribute('aria-current')).toBe(false);
  });
});
