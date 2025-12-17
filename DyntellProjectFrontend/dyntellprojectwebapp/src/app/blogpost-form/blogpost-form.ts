import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Navbar } from '../components/navbar/navbar';
import { BackButton } from '../components/back-button/back-button';
import { BlogsService } from '../services/blogs.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-blogpost-form',
  standalone: true,
  imports: [Navbar, BackButton, FormsModule, CommonModule],
  templateUrl: './blogpost-form.html',
  styleUrl: './blogpost-form.css',
})
export class BlogpostForm implements OnInit {
  title: string = '';
  content: string = '';
  postType: number = 1; // Default: Gyermek
  errorMessage: string = '';
  isLoading: boolean = false;
  isEditMode: boolean = false;
  blogPostId: number | null = null;

  // PostType options
  postTypes = [
    { value: 1, label: 'Gyermek' },
    { value: 2, label: 'Közelet' },
    { value: 3, label: 'Sport' }
  ];

  constructor(
    private blogsService: BlogsService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Legyen ID
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.blogPostId = parseInt(id, 10);
      this.loadBlogPost(this.blogPostId);
    }
  }

  loadBlogPost(id: number): void {
    this.isLoading = true;
    this.blogsService.getById(id).subscribe({
      next: (post) => {
        this.title = post.title;
        this.content = post.content;
        this.postType = post.postType ?? 1;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.errorMessage = 'Error while Loading Post.';
        this.isLoading = false;
        this.cdr.detectChanges();
        console.error('Load blogpost error:', error);
      }
    });
  }

  onSubmit() {
    // Validáció
    if (!this.title || !this.content) {
      this.errorMessage = 'Pease fill out both fields!';
      return;
    }

    if (this.title.length > 200) {
      this.errorMessage = 'Tile can be Max 200 Characters!';
      return;
    }

    this.errorMessage = '';
    this.isLoading = true;

    const blogPostData = {
      title: this.title,
      content: this.content,
      postType: Number(this.postType)
    };

    // Ha szerkesztes akkor PUT, ellenben POST
    if (this.isEditMode && this.blogPostId) {
      this.blogsService.update(this.blogPostId, blogPostData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.cdr.detectChanges();
          // -> Home
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Hiba történt a blogpost frissítése során.';
          this.cdr.detectChanges();
          console.error('Blogpost update error:', error);
        }
      });
    } else {
      // Uj post letrehozas
      this.blogsService.create(blogPostData).subscribe({
        next: (response) => {
          this.isLoading = false;
          this.cdr.detectChanges();
          // -> Home
          this.router.navigate(['/']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Error while Creating post.';
          this.cdr.detectChanges();
          console.error('Blogpost create error:', error);
        }
      });
    }
  }
}
