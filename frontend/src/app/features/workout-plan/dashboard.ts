import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import {
  Check,
  CircleAlert,
  Dumbbell,
  Image,
  LucideAngularModule,
  PersonStanding,
} from 'lucide-angular';
import { forkJoin } from 'rxjs';

import { PlanDay, PlanExercise, WeekPlan } from '../../core/models/plan';
import { PlanService } from '../../core/services/plan.service';
import { NavItem, TopNav } from '../../shared/components/top-nav/top-nav';
import {
  NOW,
  WEEK_DAYS,
  isComplete,
  joinMuscles,
  muscleLabel,
  muscleList,
  shortDate,
  splitName,
  weekDates,
  weekRange,
  weekdayIndex,
} from './week';

/** One cell of the week strip, with everything the template needs. */
interface DayView {
  index: number;
  plan: PlanDay;
  short: string;
  label: string;
  date: string;
  isToday: boolean;
  isRest: boolean;
  isDone: boolean;
}

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, LucideAngularModule, TopNav],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {
  private readonly planService = inject(PlanService);
  private readonly now = inject(NOW)();

  // Statisztika joins the menu when its page exists.
  protected readonly navItems: NavItem[] = [
    { label: 'Tervezés', path: '/plan' },
    { label: 'Heti terv', path: '/week' },
  ];

  protected readonly todayIndex = weekdayIndex(this.now);
  private readonly dates = weekDates(this.now);
  protected readonly range = weekRange(this.dates);

  protected readonly plan = rxResource({ stream: () => this.planService.getCurrentWeek() });
  /** 404 means nothing was generated for this week yet. */
  protected readonly noPlan = computed(() => {
    const error = this.plan.error();
    return error instanceof HttpErrorResponse && error.status === 404;
  });

  /** A local copy, so a tick shows at once and is undone if the PATCH fails. */
  protected readonly week = linkedSignal<WeekPlan | null>(() =>
    this.plan.hasValue() ? this.plan.value() : null,
  );
  protected readonly saveError = signal(false);
  private readonly saving = signal<ReadonlySet<number>>(new Set());

  protected readonly days = computed<DayView[]>(() =>
    (this.week()?.days ?? []).map((plan, index) => ({
      index,
      plan,
      short: WEEK_DAYS[index].short,
      label: WEEK_DAYS[index].label,
      date: shortDate(this.dates[index]),
      isToday: index === this.todayIndex,
      isRest: plan.workoutName === null,
      isDone: isComplete(plan),
    })),
  );

  protected readonly split = computed(() => splitName(this.week()?.days ?? []));
  protected readonly workoutCount = computed(() => this.days().filter((d) => !d.isRest).length);
  protected readonly doneCount = computed(() => this.days().filter((d) => d.isDone).length);

  /** Starts on today; the strip changes it. */
  protected readonly selectedIndex = signal(this.todayIndex);
  protected readonly selected = computed(() => this.days()[this.selectedIndex()] ?? null);
  protected readonly selectedMuscles = computed(() => {
    const day = this.selected();
    return day ? muscleList(day.plan) : [];
  });
  protected readonly selectedDone = computed(
    () => this.selected()?.plan.exercises.filter((e) => e.isDone).length ?? 0,
  );
  /** How the selected day loads its muscles, seen from today. */
  protected readonly loadText = computed(() => {
    const index = this.selectedIndex();
    if (index === this.todayIndex) return 'ma terhelve';
    return index < this.todayIndex ? 'terhelve' : 'tervezett';
  });

  protected readonly nextWorkout = computed(
    () => this.days().find((d) => d.index > this.todayIndex && !d.isRest) ?? null,
  );

  protected readonly Check = Check;
  protected readonly CircleAlert = CircleAlert;
  protected readonly Dumbbell = Dumbbell;
  protected readonly Image = Image;
  protected readonly PersonStanding = PersonStanding;

  protected readonly joinMuscles = joinMuscles;
  protected readonly muscleLabel = muscleLabel;
  protected readonly muscleList = muscleList;

  select(index: number): void {
    this.selectedIndex.set(index);
  }

  isSaving(exercise: PlanExercise): boolean {
    return this.saving().has(exercise.id);
  }

  toggle(exercise: PlanExercise): void {
    if (this.isSaving(exercise)) return;
    this.save([exercise.id], !exercise.isDone);
  }

  /** "Edzés befejezése": ticks every exercise of the selected day that is still open. */
  finish(): void {
    const open = this.selected()
      ?.plan.exercises.filter((e) => !e.isDone && !this.isSaving(e))
      .map((e) => e.id);
    if (open?.length) this.save(open, true);
  }

  private save(ids: number[], isDone: boolean): void {
    this.saveError.set(false);
    this.setDone(ids, isDone);
    this.saving.update((s) => new Set([...s, ...ids]));
    forkJoin(ids.map((id) => this.planService.setExerciseDone(id, isDone))).subscribe({
      error: () => {
        this.setDone(ids, !isDone);
        this.saveError.set(true);
        this.doneSaving(ids);
      },
      complete: () => this.doneSaving(ids),
    });
  }

  private doneSaving(ids: number[]): void {
    this.saving.update((s) => new Set([...s].filter((id) => !ids.includes(id))));
  }

  private setDone(ids: number[], isDone: boolean): void {
    this.week.update((week) =>
      week
        ? {
            days: week.days.map((day) => ({
              ...day,
              exercises: day.exercises.map((e) => (ids.includes(e.id) ? { ...e, isDone } : e)),
            })),
          }
        : week,
    );
  }
}
