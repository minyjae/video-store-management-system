import { Routes } from '@angular/router';
import { Home } from '../pages/home/home';
import { Movie } from '../pages/movie/movie';
import { MovieDetail } from './features/movie/pages/movie-detail/movie-detail';
import { Login } from './features/auth/pages/login/login';
import { Register } from './features/auth/pages/register/register';
import { AdminDashboard } from './features/admin/dashboard/dashboard';
import { adminGuard } from './core/admin.guard';

export const routes: Routes = [
  { path: '', component: Home },
  { path: 'movies/:id', component: MovieDetail },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'admin', component: AdminDashboard, canActivate: [adminGuard] },
  { path: 'movies', component: Movie },
];
