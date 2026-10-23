import { TestBed } from '@angular/core/testing';

import { LocationStep } from './location-step';

describe('LocationStep', () => {
  const render = (inputs: Record<string, unknown>) => {
    const fixture = TestBed.createComponent(LocationStep);
    for (const [name, value] of Object.entries(inputs)) fixture.componentRef.setInput(name, value);
    fixture.detectChanges();
    return fixture.nativeElement as HTMLElement;
  };

  it('shows the equipment chips at home', () => {
    const el = render({ selected: 'Home', equipment: [{ id: 1, name: 'Kézisúlyzó' }] });
    expect(el.querySelector('.cx-chip')?.textContent?.trim()).toBe('Kézisúlyzó');
  });

  it('shows an error message instead of the chips when loading failed', () => {
    const el = render({ selected: 'Home', equipmentError: true });
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('nem érhető el');
    expect(el.querySelector('.cx-chip')).toBeNull();
  });

  it('asks nothing more in the gym', () => {
    const el = render({ selected: 'Gym', equipment: [{ id: 1, name: 'Kézisúlyzó' }] });
    expect(el.querySelector('.cx-chip')).toBeNull();
  });
});
