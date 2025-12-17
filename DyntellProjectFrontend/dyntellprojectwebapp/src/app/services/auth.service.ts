import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  isLoggedIn(): boolean {
    return !!localStorage.getItem('authToken');
  }

  logout(): void {
    localStorage.removeItem('authToken');
    localStorage.removeItem('username');
    localStorage.removeItem('role');
    localStorage.removeItem('ageGroup');
  }

  isAdmin(): boolean {
    const role = localStorage.getItem('role');
    return role === 'Admin';
  }

  getUserInfo(): { username: string | null; role: string | null; ageGroup: string | null } {
    return {
      username: localStorage.getItem('username'),
      role: localStorage.getItem('role'),
      ageGroup: localStorage.getItem('ageGroup')
    };
  }
}

