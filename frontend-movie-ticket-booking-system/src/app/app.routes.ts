import { Routes } from '@angular/router';
import { Home } from './home/home';
import { MovieDetail } from './component/movie-detail/movie-detail';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { AdminDashboard } from './admin/dashboard/dashboard';
import { adminGuard } from './core/admin.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'movies/:id', component: MovieDetail },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'admin', component: AdminDashboard, canActivate: [adminGuard] },
];
