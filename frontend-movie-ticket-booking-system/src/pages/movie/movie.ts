import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { MovieService } from '../../app/features/movie/services/movie.service';
import { Movie as MovieModel } from '../../app/features/movie/models/movie.model';

interface CategoryGroup {
  category: string;
  movies: MovieModel[];
}

@Component({
  selector: 'app-movie',
  imports: [RouterLink],
  templateUrl: './movie.html',
  styleUrl: './movie.css',
})
export class Movie {
  private router = inject(Router);
  private movieService = inject(MovieService);

  allMovies = toSignal(this.movieService.getAll(), { initialValue: [] });

  moviesByCategory = computed<CategoryGroup[]>(() => {
    const grouped = new Map<string, MovieModel[]>();
    for (const movie of this.allMovies()) {
      if (!grouped.has(movie.category)) grouped.set(movie.category, []);
      grouped.get(movie.category)!.push(movie);
    }
    return [...grouped.entries()].map(([category, movies]) => ({ category, movies }));
  });

  categoryLabel(cat: string): string {
    return cat.replace(/_/g, ' ');
  }

  // per-strip drag state
  private dragEl: HTMLElement | null = null;
  private dragStartX = 0;
  private scrollStartLeft = 0;
  private isDragging = false;
  private hasDragged = false;

  startDrag(e: MouseEvent, el: HTMLElement) {
    this.dragEl = el;
    this.isDragging = true;
    this.hasDragged = false;
    this.dragStartX = e.clientX;
    this.scrollStartLeft = el.scrollLeft;
  }

  onDrag(e: MouseEvent) {
    if (!this.isDragging || !this.dragEl) return;
    const diff = e.clientX - this.dragStartX;
    if (Math.abs(diff) > 5) this.hasDragged = true;
    e.preventDefault();
    this.dragEl.scrollLeft = this.scrollStartLeft - diff;
  }

  endDrag() {
    this.isDragging = false;
    this.dragEl = null;
  }

  onMovieClick(id: string) {
    if (this.hasDragged) return;
    this.router.navigate(['/movies', id]);
  }
}
