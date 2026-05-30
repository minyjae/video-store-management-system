import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';
import { AuthService } from '../../auth/services/auth.service';

interface WalletResponse {
  userId: string;
  balance: number;
}

@Injectable({ providedIn: 'root' })
export class WalletService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private readonly baseUrl = 'http://localhost:5074/api/wallet';

  private _balance = signal<number | null>(null);
  balance = this._balance.asReadonly();

  private get options() {
    return { headers: { Authorization: `Bearer ${this.auth.getToken()}` } };
  }

  loadBalance() {
    this.http.get<WalletResponse>(`${this.baseUrl}/balance`, this.options)
      .subscribe({ next: res => this._balance.set(res.balance) });
  }

  deposit(amount: number) {
    return this.http.post<WalletResponse>(
      `${this.baseUrl}/deposit`, { amount }, this.options
    ).pipe(tap(res => this._balance.set(res.balance)));
  }

  clearBalance() {
    this._balance.set(null);
  }
}
