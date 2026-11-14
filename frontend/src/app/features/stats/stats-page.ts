import { Component, computed, inject, linkedSignal, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import {
  CalendarDays,
  ChartColumn,
  CircleAlert,
  ClipboardList,
  LucideAngularModule,
} from 'lucide-angular';

import { DayStatus, ExerciseHistoryEntry } from '../../core/models/stats';
import { StatsService } from '../../core/services/stats.service';
import { NavItem, TopNav } from '../../shared/components/top-nav/top-nav';
import { NOW, WEEK_DAYS, weekRange } from '../workout-plan/week';
import { formatNumber, monthDay, parseDate, seriesOf, sparkline } from './stats';

const RANGES: { label: string; weeks: number | null }[] = [
  { label: '4 hét', weeks: 4 },
  { label: '12 hét', weeks: 12 },
  { label: 'Összes', weeks: null },
];

const STATUS_LABEL: Record<DayStatus, string> = {
  Done: 'edzés kész',
  Partial: 'hiányos edzés',
  Missed: 'kimaradt',
  Rest: 'pihenőnap',
  Planned: 'tervezett',
};

const SPARK_WIDTH = 240;
const SPARK_HEIGHT = 48;

// Figma "Stats v5". The right column shows one exercise's history instead of the
// 1RM chart: the backend logs the actual reps and weight per exercise, not a 1RM.
@Component({
  selector: 'app-stats-page',
  imports: [RouterLink, LucideAngularModule, TopNav],
  templateUrl: './stats-page.html',
  styleUrl: './stats-page.css',
})
export class StatsPage {
  private readonly statsService = inject(StatsService);
  private readonly today = inject(NOW)();

  protected readonly navItems: NavItem[] = [
    { label: 'Tervezés', path: '/plan', icon: ClipboardList },
    { label: 'Heti terv', path: '/week', icon: CalendarDays },
    { label: 'Statisztika', path: '/stats', icon: ChartColumn },
  ];

  protected readonly ranges = RANGES;
  protected readonly weekDays = WEEK_DAYS;
  // 4 weeks first: a new account has one week, 12 would be mostly empty rows.
  protected readonly weeks = signal<number | null>(4);

  protected readonly stats = rxResource({
    params: () => ({ weeks: this.weeks() }),
    stream: ({ params }) => this.statsService.get(params.weeks),
  });
  protected readonly exercises = rxResource({
    stream: () => this.statsService.getExercises(),
  });

  /** "okt. 19 - nov. 15 · 4 hét" */
  protected readonly range = computed(() => {
    if (!this.stats.hasValue()) return '';
    const s = this.stats.value();
    if (!s.weeks) return '';
    return `${weekRange([parseDate(s.from), parseDate(s.to)])} · ${s.weeks} hét`;
  });

  protected readonly log = computed(() =>
    (this.stats.hasValue() ? this.stats.value().log : []).map((week) => ({
      label: monthDay(parseDate(week.weekStart)),
      completed: week.completed,
      planned: week.planned,
      days: week.days.map((status, i) => ({
        status,
        label: `${WEEK_DAYS[i].label}: ${STATUS_LABEL[status]}`,
      })),
    })),
  );

  protected readonly exerciseList = computed(() =>
    this.exercises.hasValue() ? this.exercises.value() : [],
  );
  /** Starts on the first exercise that was done at least once; the select changes it. */
  protected readonly selectedId = linkedSignal<number | null>(() => {
    const list = this.exerciseList();
    return (list.find((e) => e.timesDone > 0) ?? list[0])?.exerciseId ?? null;
  });
  protected readonly history = rxResource({
    params: () => this.selectedId() ?? undefined,
    stream: ({ params }) => this.statsService.getExerciseHistory(params),
  });

  protected readonly detail = computed(() => {
    const history = this.history.hasValue() ? this.history.value() : [];
    const series = seriesOf(history);
    const values = series.values;
    return {
      unit: series.unit,
      last: values.length ? formatNumber(values[values.length - 1]) : null,
      // No change is not worth a green number.
      delta:
        values.length > 1 && values[values.length - 1] !== values[0]
          ? values[values.length - 1] - values[0]
          : null,
      spark: sparkline(values, SPARK_WIDTH, SPARK_HEIGHT),
      // Newest first in the list.
      rows: history.map((h) => this.historyRow(h)).reverse(),
    };
  });

  protected readonly sparkWidth = SPARK_WIDTH;
  protected readonly sparkHeight = SPARK_HEIGHT;
  protected readonly formatNumber = formatNumber;
  protected readonly CircleAlert = CircleAlert;

  selectExercise(value: string): void {
    this.selectedId.set(Number(value));
  }

  private historyRow(h: ExerciseHistoryEntry) {
    const date = parseDate(h.date);
    const planned = `${h.sets} × ${h.reps}${h.weightKg === null ? '' : ` · ${formatNumber(h.weightKg)} kg`}`;
    const actual = [
      h.actualReps === null ? null : `${h.actualReps} ism.`,
      h.actualWeightKg === null ? null : `${formatNumber(h.actualWeightKg)} kg`,
    ].filter((part) => part !== null);
    const isPast =
      date < new Date(this.today.getFullYear(), this.today.getMonth(), this.today.getDate());
    return {
      date: monthDay(date),
      planned,
      actual: actual.join(', '),
      status: h.isDone ? 'Kész' : isPast ? 'Kimaradt' : 'Tervezett',
      isDone: h.isDone,
    };
  }
}
