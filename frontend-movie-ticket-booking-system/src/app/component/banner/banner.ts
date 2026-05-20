import { Component, signal, OnInit, OnDestroy } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-banner',
  imports: [NgClass],
  templateUrl: './banner.html',
  styleUrl: './banner.css',
})
export class Banner implements OnInit, OnDestroy {
  posters = [
    { url: 'https://placehold.co/1200x400/1a1a2e/ffffff?text=Movie+1', alt: 'Movie 1' },
    { url: 'https://placehold.co/1200x280/16213e/ffffff?text=Movie+2', alt: 'Movie 2' },
    { url: 'https://placehold.co/1200x280/0f3460/ffffff?text=Movie+3', alt: 'Movie 3' },
  ];

  currentIndex = signal(0);
  private timer: ReturnType<typeof setInterval> | null = null;
  private dragStartX = 0;
  private isDragging = false;

  ngOnInit() {
    this.startTimer();
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
    if (Math.abs(diff) < 50) return; // ระยะน้อยเกินไป ไม่เปลี่ยน

    if (diff > 0) {
      // ลากซ้าย → ไปหน้าถัดไป
      this.goTo((this.currentIndex() + 1) % this.posters.length);
    } else {
      // ลากขวา → ไปหน้าก่อนหน้า
      this.goTo((this.currentIndex() - 1 + this.posters.length) % this.posters.length);
    }
  }

  onMouseLeave() {
    this.isDragging = false;
  }

  private startTimer() {
    this.timer = setInterval(() => {
      this.currentIndex.update(i => (i + 1) % this.posters.length);
    }, 5000);
  }

  private stopTimer() {
    if (this.timer) clearInterval(this.timer);
  }
}
