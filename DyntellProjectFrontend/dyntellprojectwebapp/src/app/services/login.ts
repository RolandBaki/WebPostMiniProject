import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

const API_URL = 'https://localhost:7201/api';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  ageGroup: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  ageGroup: number; // 1=Gyerek, 2=Felnott, 3=Idos
}

export interface RegisterResponse {
  message?: string;
  errors?: string[];
}

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/auth/login`, {
      username: credentials.username,
      password: credentials.password
    });
  }

  register(data: RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/auth/register`, {
      username: data.username,
      email: data.email,
      password: data.password,
      ageGroup: data.ageGroup
    });
  }
}
