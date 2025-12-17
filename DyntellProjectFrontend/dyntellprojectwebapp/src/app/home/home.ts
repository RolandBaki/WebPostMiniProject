import { Component } from '@angular/core';
import { Navbar } from '../components/navbar/navbar';
import { Blogs } from '../components/blogs/blogs';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [Navbar, Blogs],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
}
