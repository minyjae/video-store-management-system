import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Movie } from './movie.model';

@Injectable({ providedIn: 'root' })
export class MovieService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/movies';

  getAll() {
    return this.http.get<Movie[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<Movie>(`${this.baseUrl}/${id}`);
  }
}
