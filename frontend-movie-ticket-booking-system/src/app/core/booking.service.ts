import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { Ticket } from './ticket.model';

@Injectable({ providedIn: 'root' })
export class BookingService {
  private readonly baseUrl = 'http://localhost:5074/api/bookings';
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private get options() {
    return { headers: { Authorization: `Bearer ${this.auth.getToken()}` } };
  }

  book(seatId: string, showtimeId: string): Observable<Ticket> {
    return this.http.post<Ticket>(this.baseUrl, { seatId, showtimeId }, this.options);
  }
}
