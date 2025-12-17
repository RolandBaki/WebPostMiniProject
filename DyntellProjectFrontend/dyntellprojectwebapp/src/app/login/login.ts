import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Navbar } from '../components/navbar/navbar';
import { BackButton } from '../components/back-button/back-button';
import { LoginService } from '../services/login';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [Navbar, BackButton, FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  username: string = '';
  password: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private loginService: LoginService,
    private router: Router
  ) {}

  onSubmit() {
    // Validacio
    if (!this.username || !this.password) {
      this.errorMessage = 'Complete both fields!';
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    // Login keres kuldese
    this.loginService.login({
      username: this.username,
      password: this.password
    }).subscribe({
      next: (response) => {
        // Token tarolasa localStorage-ba
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
        if (error.status === 401) {
          this.errorMessage = 'Wrong username or password!';
        } else {
          this.errorMessage = 'Something went wrong, please try again!';
        }
        console.error('Login error:', error);
      }
    });
  }
}
