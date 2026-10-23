import { TestBed } from '@angular/core/testing';

import { WizardAnswers } from '../wizard-state.service';
import { ReviewStep } from './review-step';

const ANSWERS: WizardAnswers = {
  goal: 'Hypertrophy',
  location: 'Home',
  equipmentIds: [1, 3],
  days: ['Monday', 'Tuesday', 'Thursday', 'Friday'],
  sessionMinutes: 60,
  difficulty: 'Medium',
};

describe('ReviewStep', () => {
  it('shows one row per answer, with the equipment names', () => {
    const fixture = TestBed.createComponent(ReviewStep);
    fixture.componentRef.setInput('answers', ANSWERS);
    fixture.componentRef.setInput('equipment', [
      { id: 1, name: 'Kézisúlyzó' },
      { id: 2, name: 'Kettlebell' },
      { id: 3, name: 'Húzódzkodó rúd' },
    ]);
    fixture.detectChanges();

    const rows = [...fixture.nativeElement.querySelectorAll('button.row')].map(
      (row: Element) =>
        `${row.querySelector('.row__label')?.textContent?.trim()} ${row.querySelector('.row__value')?.textContent?.trim()}`,
    );
    expect(rows).toEqual([
      'Cél Izomépítés · 8-12 ismétlés',
      'Helyszín Otthon · Kézisúlyzó, Húzódzkodó rúd',
      'Napok Hétfő, Kedd, Csütörtök, Péntek',
      'Edzés 60 perc',
      'Szint Haladó',
    ]);
  });

  it('emits the step of the clicked row', () => {
    const fixture = TestBed.createComponent(ReviewStep);
    fixture.componentRef.setInput('answers', ANSWERS);
    fixture.detectChanges();
    const edited: number[] = [];
    fixture.componentInstance.edit.subscribe((step) => edited.push(step));

    fixture.nativeElement.querySelectorAll('button.row')[2].click();

    expect(edited).toEqual([3]);
  });
});
