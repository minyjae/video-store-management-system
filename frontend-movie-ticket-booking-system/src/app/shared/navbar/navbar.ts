import { Component, effect, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import { AuthService } from '../../features/auth/services/auth.service';
import { WalletService } from '../../features/wallet/services/wallet.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, DecimalPipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  auth = inject(AuthService);
  wallet = inject(WalletService);
  private router = inject(Router);

  showDepositModal = signal(false);
  depositAmount = signal<number | null>(null);
  isDepositing = signal(false);
  depositError = signal('');

  readonly quickAmounts = [100, 300, 500, 1000];

  constructor() {
    effect(() => {
      if (this.auth.isLoggedIn()) {
        this.wallet.loadBalance();
      } else {
        this.wallet.clearBalance();
      }
    });
  }

  logout() {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  goHome() {
    this.router.navigate(['/']);
  }

  goMovie() {
    this.router.navigate(['/movies']);
  }

  openDeposit() {
    this.depositAmount.set(null);
    this.depositError.set('');
    this.showDepositModal.set(true);
  }

  closeDeposit() {
    this.showDepositModal.set(false);
  }

  setQuickAmount(amount: number) {
    this.depositAmount.set(amount);
  }

  confirmDeposit() {
    const amount = this.depositAmount();
    if (!amount || amount <= 0) {
      this.depositError.set('กรุณาระบุจำนวนเงินที่ถูกต้อง');
      return;
    }
    this.isDepositing.set(true);
    this.depositError.set('');
    this.wallet.deposit(amount).subscribe({
      next: () => {
        this.wallet.loadBalance();
        this.isDepositing.set(false);
        this.showDepositModal.set(false);
      },
      error: (err) => {
        const body = err.error;
        const msg = (typeof body?.error === 'string' ? body.error : null)
          ?? (Array.isArray(body?.error) ? (body.error as string[]).join(', ') : null)
          ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่';
        this.depositError.set(msg);
        this.isDepositing.set(false);
      },
    });
  }
}
