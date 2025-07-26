import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { of, Observable } from 'rxjs';

export interface User {
  id: string;
  firstName: string;
  lastName: string;
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private loggedIn = false;
  user: User | null = null;

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<boolean> {
    return this.http.post('/api/login', { username, password }).pipe(
      map(() => {
        this.loggedIn = true;
        return true;
      }),
      catchError(() => {
        this.loggedIn = false;
        return of(false);
      })
    );
  }

  logout(): void {
    this.loggedIn = false;
  }

  isLoggedIn(): boolean {
    return this.loggedIn;
  }

  getUserDetails(): Observable<boolean> {
    return this.http.get<User>('/api/users/me').pipe(
      map((user) => {
        this.loggedIn = true;
        this.user = user;
        return true;
      }),
      catchError(() => {
        this.loggedIn = false;
        this.user = null;
        return of(false);
      })
    );
  }
}
