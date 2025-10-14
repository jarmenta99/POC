import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = this.getApiUrl();

  private getApiUrl(): string {
    // Use proxy in development, direct URL in production
    if (window.location.hostname === 'localhost') {
      return '/api'; // Use proxy
    }
    return 'https://your-production-api.azurewebsites.net/api';
  }

  getTopics(environment: string = 'Qa'): Observable<any> {
    const url = `${this.baseUrl}/Topics?environment=${environment}`;
    console.log('Topics URL:', url);
    console.log('Base URL:', this.baseUrl);
    console.log('Environment:', environment);
    return this.http.get(url);
  }

  getMessages(request: any): Observable<any> {
    const url = `${this.baseUrl}/Messages`;
    console.log('Messages URL:', url);
    console.log('Messages request:', request);
    
    // Option 1: Use POST (recommended)
    return this.http.post(url, request, {
      headers: {
        'Content-Type': 'application/json'
      }
    });
  }

  constructor(private http: HttpClient) {}
}