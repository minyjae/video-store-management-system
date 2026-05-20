import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/auth';

  private _isLoggedIn = signal(!!localStorage.getItem('token'));
  private _username = signal(localStorage.getItem('username') ?? '');

  isLoggedIn = this._isLoggedIn.asReadonly();
  username = this._username.asReadonly();

  login(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.baseUrl}/login`, { username, password });
  }

  register(username: string, password: string) {
    return this.http.post<{ token: string }>(`${this.baseUrl}/reg`, { username, password });
  }

  saveToken(token: string, username: string) {
    localStorage.setItem('token', token);
    localStorage.setItem('username', username);
    this._isLoggedIn.set(true);
    this._username.set(username);
  }

  getToken() {
    return localStorage.getItem('token');
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('username');
    this._isLoggedIn.set(false);
    this._username.set('');
  }
}
