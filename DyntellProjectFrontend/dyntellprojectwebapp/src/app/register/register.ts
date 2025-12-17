import { Component, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Navbar } from '../components/navbar/navbar';
import { RouterModule } from '@angular/router';
import { BackButton } from '../components/back-button/back-button';
import { LoginService } from '../services/login';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [Navbar, RouterModule, BackButton, FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  username: string = '';
  email: string = '';
  password: string = '';
  ageGroup: number = 2; // Default: Felnott
  errorMessage: string = '';
  errors: string[] = [];
  isLoading: boolean = false;

  // AgeGroup options
  ageGroups = [
    { value: 1, label: 'Gyerek' },
    { value: 2, label: 'Felnőtt' },
    { value: 3, label: 'Idős' }
  ];

  constructor(
    private loginService: LoginService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  onSubmit() {
    // Validacio
    if (!this.username || !this.email || !this.password) {
      this.errorMessage = 'Please comlete both fields!';
      return;
    }

    // Email validacio
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(this.email)) {
      this.errorMessage = 'Please give us an email!';
      return;
    }

    // Jelszó hossz validáció
    if (this.password.length < 6) {
      this.errorMessage = 'Password should be min 6 characters!';
      return;
    }

    this.errorMessage = '';
    this.errors = [];
    this.isLoading = true;

    // Register keres kuldese
    this.loginService.register({
      username: this.username,
      email: this.email,
      password: this.password,
      ageGroup: Number(this.ageGroup) // biztosan szamot kuldunk (enum int)
    }).subscribe({
      next: (response) => {
        // Sikeres reg. utan automatikus bejelentkezes (backend LoginResponse-t ad vissza)
        localStorage.setItem('authToken', response.token);
        localStorage.setItem('username', response.username);
        localStorage.setItem('role', response.role);
        localStorage.setItem('ageGroup', response.ageGroup);
        
        this.isLoading = false;
        // -> a home oldalra
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.isLoading = false;
        
        // Hibauzenet kezelese
        const e = error.error;
        console.error('Register error:', error);
        console.error('Error response:', e);
        
        if (e?.errors && Array.isArray(e.errors)) {
          this.errors = e.errors;
          this.errorMessage = e.message || 'Unable to register.';
        } else if (e?.errors && typeof e.errors === 'object') {
          // Model validation dictionary -> object to array
          this.errors = Object.values(e.errors).flat() as string[];
          this.errorMessage = e.title || e.message || 'Unable to register.';
        } else if (e?.message) {
          this.errorMessage = e.message;
          this.errors = [];
        } else {
          this.errorMessage = 'Something went wrong, please try again!';
          this.errors = [];
        }
        
        // Change detection trigger
        this.cdr.detectChanges();
      }
    });
  }
}
