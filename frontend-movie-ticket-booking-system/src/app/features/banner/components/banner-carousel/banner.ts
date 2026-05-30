import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { NgClass } from '@angular/common';
import { BannerService } from '../../services/banner.service';
import { Banner as BannerItem } from '../../models/banner.model';

@Component({
  selector: 'app-banner',
  imports: [NgClass],
  templateUrl: './banner.html',
  styleUrl: './banner.css',
})
export class Banner implements OnInit, OnDestroy {
  private readonly bannerService = inject(BannerService);

  banners = signal<BannerItem[]>([]);
  currentIndex = signal(0);
  private timer: ReturnType<typeof setInterval> | null = null;
  private dragStartX = 0;
  private isDragging = false;

  stripWidth = computed(() => this.banners().length * 100);
  slideWidth = computed(() => (this.banners().length > 0 ? 100 / this.banners().length : 100));

  ngOnInit() {
    this.bannerService.getAll().subscribe({
      next: (data) => {
        this.banners.set(data);
        this.startTimer();
      },
    });
  }

  ngOnDestroy() {
    this.stopTimer();
  }

  goTo(index: number) {
    this.currentIndex.set(index);
    this.stopTimer();
    this.startTimer();
  }

  onMouseDown(e: MouseEvent) {
    this.dragStartX = e.clientX;
    this.isDragging = true;
  }

  onMouseUp(e: MouseEvent) {
    if (!this.isDragging) return;
    this.isDragging = false;
    const diff = this.dragStartX - e.clientX;
    if (Math.abs(diff) < 50) return;
    const len = this.banners().length;
    if (diff > 0) {
      this.goTo((this.currentIndex() + 1) % len);
    } else {
      this.goTo((this.currentIndex() - 1 + len) % len);
    }
  }

  onMouseLeave() {
    this.isDragging = false;
  }

  private startTimer() {
    if (this.banners().length === 0) return;
    this.timer = setInterval(() => {
      this.currentIndex.update((i) => (i + 1) % this.banners().length);
    }, 5000);
  }

  private stopTimer() {
    if (this.timer) clearInterval(this.timer);
  }
}
