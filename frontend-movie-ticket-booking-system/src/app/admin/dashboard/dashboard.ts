import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../core/auth.service';
import { MovieService } from '../../core/movie.service';
import { MovieCategory } from '../../core/movie.model';
import { Navbar } from '../../component/navbar/navbar';

@Component({
  selector: 'app-admin-dashboard',
  imports: [FormsModule, Navbar],
  templateUrl: './dashboard.html',
})
export class AdminDashboard {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private movieService = inject(MovieService);

  private readonly moviesUrl = 'http://localhost:5074/api/movies';
  private readonly showtimesUrl = 'http://localhost:5074/api/showtimes';

  movies = toSignal(this.movieService.getAll(), { initialValue: [] });
  categories = Object.values(MovieCategory);

  activeTab = signal<'movie' | 'showtime'>('movie');

  // Movie form
  movieForm = { title: '', plot: '', price: 0, duration: '', category: MovieCategory.Action };
  movieMessage = signal('');
  movieError = signal('');

  // Showtime form
  showtimeForm = { movieId: '', screenId: '', startTime: '' };
  showtimeMessage = signal('');
  showtimeError = signal('');

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  submitMovie() {
    this.movieMessage.set('');
    this.movieError.set('');

    const body = {
      title: this.movieForm.title,
      plot: this.movieForm.plot,
      price: this.movieForm.price,
      duration: this.movieForm.duration + ':00',  // HH:MM → HH:MM:SS
      category: this.movieForm.category,
    };

    this.http.post(this.moviesUrl, body, { headers: this.headers }).subscribe({
      next: () => {
        this.movieMessage.set('เพิ่มหนังสำเร็จ');
        this.movieForm = { title: '', plot: '', price: 0, duration: '', category: MovieCategory.Action };
      },
      error: (err) => this.movieError.set(err.error?.message ?? 'เกิดข้อผิดพลาด'),
    });
  }

  submitShowtime() {
    this.showtimeMessage.set('');
    this.showtimeError.set('');

    const body = {
      movieId: this.showtimeForm.movieId,
      screenId: this.showtimeForm.screenId,
      startTime: this.showtimeForm.startTime,
    };

    this.http.post(this.showtimesUrl, body, { headers: this.headers }).subscribe({
      next: () => {
        this.showtimeMessage.set('เพิ่มรอบฉายสำเร็จ');
        this.showtimeForm = { movieId: '', screenId: '', startTime: '' };
      },
      error: (err) => this.showtimeError.set(err.error?.message ?? 'เกิดข้อผิดพลาด'),
    });
  }
}
