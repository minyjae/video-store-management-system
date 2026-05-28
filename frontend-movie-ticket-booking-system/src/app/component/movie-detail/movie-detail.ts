import { Component, computed, inject, signal } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { forkJoin, switchMap, map } from 'rxjs';
import { MovieService } from '../../core/movie.service';
import { ShowtimeService } from '../../core/showtime.service';
import { SeatService } from '../../core/seat.service';
import { BookingService } from '../../core/booking.service';
import { AuthService } from '../../core/auth.service';
import { Showtime } from '../../core/showtime.model';
import { Seat, SeatStatus, SeatType } from '../../core/seat.model';
import { Ticket } from '../../core/ticket.model';

interface SeatRow {
  rowKey: string;
  seats: Seat[];
  isVip: boolean;
  couples: [Seat, Seat][];
}

@Component({
  selector: 'app-movie-detail',
  imports: [DatePipe, DecimalPipe],
  templateUrl: './movie-detail.html',
  styleUrl: './movie-detail.css',
})
export class MovieDetail {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private movieService = inject(MovieService);
  private showtimeService = inject(ShowtimeService);
  private seatService = inject(SeatService);
  private bookingService = inject(BookingService);
  private auth = inject(AuthService);

  SeatStatus = SeatStatus;
  SeatType = SeatType;

  private movieId$ = this.route.paramMap.pipe(map(p => p.get('id') ?? ''));

  movie = toSignal(this.movieId$.pipe(switchMap(id => this.movieService.getById(id))));
  showtimes = toSignal(
    this.movieId$.pipe(switchMap(id => this.showtimeService.getByMovieId(id))),
    { initialValue: [] }
  );

  selectedShowtime = signal<Showtime | null>(null);
  seats = signal<Seat[]>([]);
  selectedSeatIds = signal<Set<string>>(new Set());
  isLoadingSeats = signal(false);
  isBooking = signal(false);
  bookingError = signal('');
  bookedTickets = signal<Ticket[]>([]);

  // Group seats by row, detect VIP rows, build couples for VIP
  seatsByRow = computed<SeatRow[]>(() => {
    const grouped = new Map<string, Seat[]>();
    for (const seat of this.seats()) {
      const row = seat.seatCode.charAt(0);
      if (!grouped.has(row)) grouped.set(row, []);
      grouped.get(row)!.push(seat);
    }
    return [...grouped.entries()]
      .sort((a, b) => b[0].localeCompare(a[0]))
      .map(([rowKey, rowSeats]) => {
        rowSeats.sort((a, b) => parseInt(a.seatCode.slice(1)) - parseInt(b.seatCode.slice(1)));
        const isVip = rowSeats.every(s => s.type === SeatType.VIP);
        const couples: [Seat, Seat][] = [];
        if (isVip) {
          for (let i = 0; i + 1 < rowSeats.length; i += 2) {
            couples.push([rowSeats[i], rowSeats[i + 1]]);
          }
        }
        return { rowKey, seats: rowSeats, isVip, couples };
      });
  });

  selectedSeats = computed(() => this.seats().filter(s => this.selectedSeatIds().has(s.id)));
  selectedSeatCodes = computed(() => this.selectedSeats().map(s => s.seatCode).join(', '));
  totalPrice = computed(() => this.selectedSeats().reduce((sum, s) => sum + s.price, 0));

  selectShowtime(showtime: Showtime) {
    if (this.selectedShowtime()?.id === showtime.id) return;
    this.selectedShowtime.set(showtime);
    this.selectedSeatIds.set(new Set());
    this.seats.set([]);
    this.bookingError.set('');
    this.isLoadingSeats.set(true);
    this.seatService.getByShowtimeId(showtime.id).subscribe({
      next: seats => {
        this.seats.set(seats);
        this.isLoadingSeats.set(false);
      },
      error: () => {
        this.bookingError.set('ไม่สามารถโหลดข้อมูลที่นั่งได้ กรุณาลองใหม่อีกครั้ง');
        this.isLoadingSeats.set(false);
      },
    });
  }

  // Toggle individual normal seat
  toggleSeat(seat: Seat) {
    if (seat.status !== SeatStatus.Available) return;
    const ids = new Set(this.selectedSeatIds());
    if (ids.has(seat.id)) ids.delete(seat.id);
    else ids.add(seat.id);
    this.selectedSeatIds.set(ids);
  }

  // Toggle VIP couple (both seats together)
  toggleCouple(s1: Seat, s2: Seat) {
    if (s1.status !== SeatStatus.Available || s2.status !== SeatStatus.Available) return;
    const ids = new Set(this.selectedSeatIds());
    const bothSelected = ids.has(s1.id) && ids.has(s2.id);
    if (bothSelected) { ids.delete(s1.id); ids.delete(s2.id); }
    else { ids.add(s1.id); ids.add(s2.id); }
    this.selectedSeatIds.set(ids);
  }

  getSeatClass(seat: Seat): string {
    if (this.selectedSeatIds().has(seat.id)) return 'seat seat-selected';
    if (seat.status === SeatStatus.Booked) return 'seat seat-booked';
    if (seat.status === SeatStatus.Locked) return 'seat seat-locked';
    return 'seat seat-available';
  }

  getCoupleSeatClass(s1: Seat, s2: Seat): string {
    const base = 'couple-seat ';
    if (s1.status === SeatStatus.Booked || s2.status === SeatStatus.Booked) return base + 'seat-booked';
    if (s1.status === SeatStatus.Locked || s2.status === SeatStatus.Locked) return base + 'seat-locked';
    if (this.selectedSeatIds().has(s1.id) && this.selectedSeatIds().has(s2.id)) return base + 'seat-selected';
    return base + 'seat-vip';
  }

  bookSeats() {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/login']);
      return;
    }
    const seats = this.selectedSeats();
    const showtime = this.selectedShowtime();
    if (seats.length === 0 || !showtime) return;

    this.isBooking.set(true);
    this.bookingError.set('');

    forkJoin(seats.map(s => this.bookingService.book(s.id, showtime.id))).subscribe({
      next: tickets => {
        const bookedIds = new Set(seats.map(s => s.id));
        this.seats.update(all =>
          all.map(s => bookedIds.has(s.id) ? { ...s, status: SeatStatus.Booked } : s)
        );
        this.selectedSeatIds.set(new Set());
        this.bookedTickets.set(tickets);
        this.isBooking.set(false);
      },
      error: err => {
        const body = err.error;
        // Middleware returns { error: "...", statusCode }
        // ASP.NET Core model binding returns { title: "...", errors: {...} }
        const message = (typeof body?.error === 'string' ? body.error : null)
          ?? body?.title
          ?? (body?.errors ? Object.values(body.errors as Record<string, string[]>).flat().join(', ') : null)
          ?? 'เกิดข้อผิดพลาดในการจอง';
        this.bookingError.set(message);
        this.isBooking.set(false);
      },
    });
  }

  closeSuccessModal() {
    this.bookedTickets.set([]);
  }

}
