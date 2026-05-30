import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Banner } from '../models/banner.model';
import { Observable } from 'rxjs';

export interface CreateBannerPayload {
  title: string;
  tagline: string;
  genre: string;
  image: File;
}

export interface UpdateBannerPayload {
  id: string;
  title?: string;
  tagline?: string;
  genre?: string;
}

@Injectable({ providedIn: 'root' })
export class BannerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5074/api/banners';

  getAll(): Observable<Banner[]> {
    return this.http.get<Banner[]>(this.baseUrl);
  }

  upload(payload: CreateBannerPayload, headers: HttpHeaders): Observable<Banner> {
    const form = new FormData();
    form.append('title', payload.title);
    form.append('tagline', payload.tagline);
    form.append('genre', payload.genre);
    form.append('image', payload.image);
    return this.http.post<Banner>(this.baseUrl, form, { headers });
  }

  update(payload: UpdateBannerPayload, headers: HttpHeaders): Observable<Banner> {
    return this.http.put<Banner>(this.baseUrl, payload, { headers });
  }

  delete(id: string, headers: HttpHeaders): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`, { headers });
  }
}
