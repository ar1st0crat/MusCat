import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Performer } from '../components/performer/performer.component';

@Injectable({
  providedIn: 'root'
})
export class PerformerService {
  uri = 'http://localhost:57115/api';
  httpOptions = {
    headers: new HttpHeaders({
      'Content-Type':  'application/json'
    })
  };

  page = 0;
  size = 8;

  constructor(private http: HttpClient) { }

  getPerformers(): Observable<any> {
    return this.http.get(`${this.uri}/performers?page=${this.page}&size=${this.size}`);
  }

  searchPerformers(searchPattern: string): Observable<any> {
    return this.http.get(`${this.uri}/performers?page=${this.page}&size=${this.size}&search=${searchPattern}`);
  }

  addPerformer(performer: Performer): Observable<any> {
    return this.http.post(`${this.uri}/performers`, JSON.stringify(performer), this.httpOptions);
  }

  updatePerformer(performerId: number, performer: Performer): Observable<any> {
    return this.http.put(`${this.uri}/performers/${performerId}`, JSON.stringify(performer), this.httpOptions);
  }

  deletePerformer(performerId: number): Observable<any> {
    return this.http.delete(`${this.uri}/performers/${performerId}`);
  }

  getAlbums(performerId: number): any {
    return this.http.get(`${this.uri}/performers/${performerId}/albums`);
  }

  getSongs(albumId: number): any {
    return this.http.get(`${this.uri}/albums/${albumId}/songs`);
  }

  getCountries(): Observable<any> {
    return this.http.get(`${this.uri}/countries`);
  }
}
