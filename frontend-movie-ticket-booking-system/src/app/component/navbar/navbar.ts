import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css',
})
export class Navbar {
  auth = inject(AuthService);
  private router = inject(Router);

  logout() {
    this.auth.logout();
    this.router.navigate(['/']);
  }

  goHome() {
    this.router.navigate(['/']);
  }
}
