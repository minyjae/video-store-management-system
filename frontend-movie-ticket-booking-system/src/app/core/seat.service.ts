import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Seat } from './seat.model';

@Injectable({ providedIn: 'root' })
export class SeatService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/seats';

  getByShowtimeId(showtimeId: string) {
    return this.http.get<Seat[]>(`${this.baseUrl}/showtime/${showtimeId}`);
  }
}
