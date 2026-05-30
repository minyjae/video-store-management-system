import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { toSignal } from '@angular/core/rxjs-interop';
import { AuthService } from '../../auth/services/auth.service';
import { MovieService } from '../../movie/services/movie.service';
import { BannerService } from '../../banner/services/banner.service';
import { MovieCategory, Movie } from '../../movie/models/movie.model';
import { Banner } from '../../banner/models/banner.model';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-admin-dashboard',
  imports: [FormsModule, NgSelectModule],
  templateUrl: './dashboard.html',
})
export class AdminDashboard {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private movieService = inject(MovieService);
  private bannerService = inject(BannerService);

  private readonly moviesUrl = 'http://localhost:5074/api/movies';
  private readonly showtimesUrl = 'http://localhost:5074/api/showtimes';

  movies = toSignal(this.movieService.getAll(), { initialValue: [] });
  categories = Object.values(MovieCategory);

  activeTab = signal<'banner' | 'movie' | 'showtime'>('movie');

  // ── Movie form ──
  movieForm = { title: '', plot: '', price: null as number | null, durationHours: 0, durationMinutes: 0, durationSeconds: 0, category: null as MovieCategory | null };
  movieMessage = signal('');
  movieError = signal('');
  posterFile: File | null = null;
  posterPreviewUrl = signal('');

  // ── Showtime form ──
  showtimeForm = { movieId: '', screenId: '', startTime: '' };
  showtimeMessage = signal('');
  showtimeError = signal('');

  // ── Banner tab ──
  banners = signal<Banner[]>([]);
  bannerMessage = signal('');
  bannerError = signal('');
  bannerLoading = signal(false);

  bannerFile: File | null = null;
  bannerPreviewUrl = signal('');
  bannerForm = { title: '', tagline: '', genre: null as string | null };

  editingBanner = signal<Banner | null>(null);
  editForm = { title: '', tagline: '', genre: null as string | null };

  private get headers(): HttpHeaders {
    return new HttpHeaders({ Authorization: `Bearer ${this.auth.getToken()}` });
  }

  // ── Banner methods ──
  onBannerTabOpen() {
    this.bannerService.getAll().subscribe({ next: (data) => this.banners.set(data) });
  }

  onBannerSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.bannerFile = file;
    this.bannerPreviewUrl.set(URL.createObjectURL(file));
  }

  uploadBanner() {
    if (!this.bannerFile) { this.bannerError.set('กรุณาเลือกไฟล์รูปภาพ'); return; }
    if (!this.bannerForm.title) { this.bannerError.set('กรุณากรอกชื่อ banner'); return; }

    this.bannerMessage.set('');
    this.bannerError.set('');
    this.bannerLoading.set(true);

    this.bannerService.upload(
      { ...this.bannerForm, genre: this.bannerForm.genre ?? '', image: this.bannerFile },
      this.headers,
    ).subscribe({
      next: (banner) => {
        this.banners.update((list) => [...list, banner]);
        this.bannerFile = null;
        this.bannerPreviewUrl.set('');
        this.bannerForm = { title: '', tagline: '', genre: '' };
        this.bannerMessage.set('เพิ่ม banner สำเร็จ');
        this.bannerLoading.set(false);
      },
      error: (err) => {
        this.bannerError.set(err.error?.error ?? err.error?.message ?? 'เกิดข้อผิดพลาด');
        this.bannerLoading.set(false);
      },
    });
  }

  startEdit(banner: Banner) {
    this.editingBanner.set(banner);
    this.editForm = { title: banner.title, tagline: banner.tagline, genre: banner.genre };
  }

  cancelEdit() {
    this.editingBanner.set(null);
  }

  saveEdit() {
    const banner = this.editingBanner();
    if (!banner) return;
    this.bannerMessage.set('');
    this.bannerError.set('');

    this.bannerService.update({ id: banner.id, ...this.editForm, genre: this.editForm.genre ?? undefined }, this.headers).subscribe({
      next: (updated) => {
        this.banners.update((list) => list.map((b) => (b.id === updated.id ? updated : b)));
        this.editingBanner.set(null);
        this.bannerMessage.set('แก้ไข banner สำเร็จ');
      },
      error: (err) => this.bannerError.set(err.error?.error ?? 'เกิดข้อผิดพลาด'),
    });
  }

  deleteBanner(id: string) {
    this.bannerMessage.set('');
    this.bannerError.set('');

    this.bannerService.delete(id, this.headers).subscribe({
      next: () => {
        this.banners.update((list) => list.filter((b) => b.id !== id));
        if (this.editingBanner()?.id === id) this.editingBanner.set(null);
        this.bannerMessage.set('ลบ banner สำเร็จ');
      },
      error: (err) => this.bannerError.set(err.error?.error ?? 'เกิดข้อผิดพลาด'),
    });
  }

  // ── Movie methods ──
  onPosterSelected(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    this.posterFile = file;
    this.posterPreviewUrl.set(URL.createObjectURL(file));
  }

  submitMovie() {
    this.movieMessage.set('');
    this.movieError.set('');

    if (!this.posterFile) { this.movieError.set('กรุณาเลือกโปสเตอร์หนัง'); return; }

    const form = new FormData();
    form.append('title', this.movieForm.title);
    form.append('plot', this.movieForm.plot);
    form.append('price', String(this.movieForm.price ?? 0));
    form.append('duration', [
      this.movieForm.durationHours,
      this.movieForm.durationMinutes,
      this.movieForm.durationSeconds,
    ].map(v => String(v).padStart(2, '0')).join(':'));
    form.append('category', this.movieForm.category ?? '');
    form.append('poster', this.posterFile);

    this.http.post<Movie>(this.moviesUrl, form, { headers: this.headers }).subscribe({
      next: () => this.resetMovieForm(),
      error: (err) => this.movieError.set(err.error?.error ?? err.error?.message ?? 'เกิดข้อผิดพลาด'),
    });
  }

  private resetMovieForm() {
    this.movieMessage.set('เพิ่มหนังสำเร็จ');
    this.movieForm = { title: '', plot: '', price: null, durationHours: 0, durationMinutes: 0, durationSeconds: 0, category: null };
    this.posterFile = null;
    this.posterPreviewUrl.set('');
  }

  // ── Showtime methods ──
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
      error: (err) => this.showtimeError.set(err.error?.error ?? err.error?.message ?? 'เกิดข้อผิดพลาด'),
    });
  }
}
