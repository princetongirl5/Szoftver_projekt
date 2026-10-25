import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';

import { GeneratePlanRequest, WeekPlan } from '../models/plan';
import { HttpPlanService } from './http-plan.service';

describe('HttpPlanService', () => {
  let service: HttpPlanService;
  let http: HttpTestingController;

  const week: WeekPlan = { days: [] };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [HttpPlanService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(HttpPlanService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('posts the answers to generate', async () => {
    const request: GeneratePlanRequest = {
      goal: 'Strength',
      location: 'Gym',
      equipmentIds: [],
      days: ['Monday', 'Thursday'],
      sessionMinutes: 60,
      difficulty: 'Medium',
    };
    const result = firstValueFrom(service.generate(request));

    const req = http.expectOne('/api/plans/generate');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(week);

    expect(await result).toEqual(week);
  });

  it('gets the current week', async () => {
    const result = firstValueFrom(service.getCurrentWeek());

    const req = http.expectOne('/api/plans/current');
    expect(req.request.method).toBe('GET');
    req.flush(week);

    expect(await result).toEqual(week);
  });

  it('patches one exercise', async () => {
    const result = firstValueFrom(service.setExerciseDone(42, true));

    const req = http.expectOne('/api/plans/exercises/42');
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual({ isDone: true });
    req.flush(null, { status: 204, statusText: 'No Content' });

    await result;
  });
});
