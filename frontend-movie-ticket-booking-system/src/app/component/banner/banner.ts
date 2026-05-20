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
    {
      url: 'https://picsum.photos/seed/galaxy/1600/700',
      alt: 'Interstellar Odyssey',
      title: 'Interstellar Odyssey',
      tagline: 'ข้ามขอบเขตแห่งจักรวาล เพื่อค้นหาบ้านใหม่ของมนุษยชาติ',
      genre: 'Sci-Fi',
    },
    {
      url: 'https://picsum.photos/seed/darkforest/1600/700',
      alt: 'Shadow of the Forgotten',
      title: 'Shadow of the Forgotten',
      tagline: 'ในความมืด ความจริงที่ซ่อนอยู่กำลังรอการเปิดเผย',
      genre: 'Thriller',
    },
    {
      url: 'https://picsum.photos/seed/citynight/1600/700',
      alt: 'Neon City',
      title: 'Neon City',
      tagline: 'เมืองที่ไม่เคยหลับ และวีรบุรุษที่ไม่เคยยอมแพ้',
      genre: 'Action',
    },
    {
      url: 'https://picsum.photos/seed/mountainpeak/1600/700',
      alt: 'The Last Summit',
      title: 'The Last Summit',
      tagline: 'ทุกก้าวที่ก้าวขึ้น คือการต่อสู้กับตัวเอง',
      genre: 'Drama',
    },
    {
      url: 'https://picsum.photos/seed/oceanwave/1600/700',
      alt: 'Deep Blue',
      title: 'Deep Blue',
      tagline: 'ความลึกของมหาสมุทร ไม่ลึกเท่าความลับที่ซ่อนอยู่',
      genre: 'Adventure',
    },
  ];

  currentIndex = signal(0);
  private timer: ReturnType<typeof setInterval> | null = null;
  private dragStartX = 0;
  private isDragging = false;

  // Pre-computed widths for correct slide math
  readonly stripWidth = this.posters.length * 100;       // e.g. 500%
  readonly slideWidth = 100 / this.posters.length;        // e.g. 20%

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
