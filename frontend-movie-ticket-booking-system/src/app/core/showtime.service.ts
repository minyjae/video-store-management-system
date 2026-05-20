import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Showtime } from './showtime.model';

@Injectable({ providedIn: 'root' })
export class ShowtimeService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/showtimes';

  getByMovieId(movieId: string) {
    return this.http.get<Showtime[]>(`${this.baseUrl}/movie/${movieId}`);
  }
}
