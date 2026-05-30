import { Component, effect, ElementRef, inject, ViewChild } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { Footer } from '../../app/shared/footer/footer';
import { Banner } from '../../app/features/banner/components/banner-carousel/banner';
import { MovieService } from '../../app/features/movie/services/movie.service';

@Component({
  selector: 'app-home',
  imports: [Footer, Banner],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  @ViewChild('movieStrip') movieStrip!: ElementRef<HTMLDivElement>;

  private router = inject(Router);
  private movieService = inject(MovieService);

  movies = toSignal(this.movieService.getAll(), { initialValue: [] });
  constructor() {
    effect(() => console.log(this.movies()));
  }
  
  private dragStartX = 0;
  private scrollStartLeft = 0;
  private isDragging = false;
  private hasDragged = false;

  onMouseDown(e: MouseEvent) {
    this.isDragging = true;
    this.hasDragged = false;
    this.dragStartX = e.clientX;
    this.scrollStartLeft = this.movieStrip.nativeElement.scrollLeft;
  }

  onMouseMove(e: MouseEvent) {
    if (!this.isDragging) return;
    const diff = e.clientX - this.dragStartX;
    if (Math.abs(diff) > 5) this.hasDragged = true;
    e.preventDefault();
    this.movieStrip.nativeElement.scrollLeft = this.scrollStartLeft - diff;
  }

  onDragEnd() {
    this.isDragging = false;
  }

  onMovieClick(id: string) {
    if (this.hasDragged) return;
    this.router.navigate(['/movies', id]);
  }
}
