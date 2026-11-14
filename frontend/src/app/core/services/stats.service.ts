import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { ExerciseHistoryEntry, ExerciseStats, Stats } from '../models/stats';

@Injectable({ providedIn: 'root' })
export class StatsService {
  private readonly http = inject(HttpClient);

  /** weeks: the last N weeks, or null for every week since the first plan. */
  get(weeks: number | null): Observable<Stats> {
    return this.http.get<Stats>('/api/stats', { params: weeks === null ? {} : { weeks } });
  }

  getExercises(): Observable<ExerciseStats[]> {
    return this.http.get<ExerciseStats[]>('/api/stats/exercises');
  }

  getExerciseHistory(exerciseId: number): Observable<ExerciseHistoryEntry[]> {
    return this.http.get<ExerciseHistoryEntry[]>(`/api/stats/exercises/${exerciseId}`);
  }
}
