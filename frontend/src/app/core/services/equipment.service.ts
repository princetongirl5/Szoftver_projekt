import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { Equipment } from '../models/equipment';

@Injectable({ providedIn: 'root' })
export class EquipmentService {
  private readonly http = inject(HttpClient);

  getAll(): Observable<Equipment[]> {
    return this.http.get<Equipment[]>('/api/equipment');
  }
}
