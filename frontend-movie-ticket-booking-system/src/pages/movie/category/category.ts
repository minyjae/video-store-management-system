import { Component, computed, inject } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { MovieService } from '../../../app/features/movie/services/movie.service';

@Component({
  selector: 'app-movie-category',
  imports: [RouterLink],
  templateUrl: './category.html',
})
export class MovieCategory {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private movieService = inject(MovieService);

  categoryId = toSignal(
    this.route.paramMap.pipe(map(p => p.get('id') ?? '')),
    { initialValue: '' }
  );

  allMovies = toSignal(this.movieService.getAll(), { initialValue: [] });

  movies = computed(() =>
    this.allMovies().filter(m => m.category === this.categoryId())
  );

  categoryLabel(): string {
    return this.categoryId().replace(/_/g, ' ');
  }

  onMovieClick(id: string) {
    this.router.navigate(['/movies', id]);
  }
}
