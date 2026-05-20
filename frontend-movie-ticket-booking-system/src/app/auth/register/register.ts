import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-register',
  imports: [RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  username = signal('');
  password = signal('');
  confirmPassword = signal('');
  isLoading = signal(false);
  error = signal('');

  onSubmit() {
    if (!this.username() || !this.password() || !this.confirmPassword()) {
      this.error.set('กรุณากรอกข้อมูลให้ครบ');
      return;
    }
    if (this.password() !== this.confirmPassword()) {
      this.error.set('รหัสผ่านไม่ตรงกัน');
      return;
    }
    this.isLoading.set(true);
    this.error.set('');

    this.authService.register(this.username(), this.password()).subscribe({
      next: (res) => {
        this.authService.saveToken(res.token, this.username());
        this.router.navigate(['/']);
      },
      error: () => {
        this.error.set('ชื่อผู้ใช้นี้ถูกใช้แล้ว หรือข้อมูลไม่ถูกต้อง');
        this.isLoading.set(false);
      },
    });
  }
}
