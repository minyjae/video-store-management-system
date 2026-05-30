import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/auth';

  private _isLoggedIn = signal(!!localStorage.getItem('token'));
  private _username = signal(localStorage.getItem('username') ?? '');
  private _role = signal(localStorage.getItem('role') ?? '');

  isLoggedIn = this._isLoggedIn.asReadonly();
  username = this._username.asReadonly();
  isAdmin = computed(() => this._role() === 'Admin');

  login(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.baseUrl}/login`, { username, password });
  }

  register(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.baseUrl}/reg`, { username, password });
  }

  saveToken(token: string, username: string) {
    const role = this.parseRole(token);
    localStorage.setItem('token', token);
    localStorage.setItem('username', username);
    localStorage.setItem('role', role);
    this._isLoggedIn.set(true);
    this._username.set(username);
    this._role.set(role);
  }

  getToken() {
    return localStorage.getItem('token');
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    this._isLoggedIn.set(false);
    this._username.set('');
    this._role.set('');
  }

  private parseRole(token: string): string {
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['role'] ?? '';
    } catch {
      return '';
    }
  }
}
