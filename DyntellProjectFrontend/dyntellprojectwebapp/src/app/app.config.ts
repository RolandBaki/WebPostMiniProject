import { ApplicationConfig } from '@angular/core';
import { provideRouter, Routes } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { Home } from './home/home';
import { Login } from './login/login';
import { Register } from './register/register';
import { authInterceptor } from './interceptors/auth.interceptor';
import { BlogpostForm } from './blogpost-form/blogpost-form';

const routes: Routes = [
  { path: '', component: Home },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'add-blog', component: BlogpostForm },
  { path: 'edit-blog/:id', component: BlogpostForm },
  { path: '**', redirectTo: '' }
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor]))
  ]
};
