import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  imports: [RouterLink],
  templateUrl: './login.html',
})
export class Login {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = signal('');
  password = signal('');
  isLoading = signal(false);
  error = signal('');

  onSubmit() {
    if (!this.username() || !this.password()) {
      this.error.set('กรุณากรอกข้อมูลให้ครบ');
      return;
    }
    this.isLoading.set(true);
    this.error.set('');

    this.authService.login(this.username(), this.password()).subscribe({
      next: (res) => {
        this.authService.saveToken(res.token, this.username());
        this.router.navigate(['/']);
      },
      error: () => {
        this.error.set('ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง');
        this.isLoading.set(false);
      },
    });
  }
}
