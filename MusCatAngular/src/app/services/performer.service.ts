import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class PerformerService {
  uri = 'http://localhost:57115/api';
  page = 0;
  size = 8;

  constructor(private http: HttpClient) { }

  getPerformers(): any {
    return this.http.get(`${this.uri}/performers?page=${this.page}&size=${this.size}`);
  }

  getAlbums(performerId: number): any {
    return this.http.get(`${this.uri}/performers/${performerId}/albums`);
  }

  getSongs(albumId: number): any {
    return this.http.get(`${this.uri}/albums/${albumId}/songs`);
  }
}
