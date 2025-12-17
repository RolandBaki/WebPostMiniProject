import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../../services/auth.service';
import { BlogsService } from '../../services/blogs.service';
import { BlogPost } from '../../models/blog-post';
import { Comments } from '../comment/comment';

@Component({
  selector: 'app-blogs',
  standalone: true,
  imports: [CommonModule, Comments],
  templateUrl: './blogs.html',
  styleUrl: './blogs.css',
})
export class Blogs implements OnInit {
  posts: BlogPost[] = [];
  loading = false;
  errorMessage = '';
  expandedPosts: Set<number> = new Set(); // nyitott postok nyilvantartasa

  constructor(
    public authService: AuthService,
    private blogsService: BlogsService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  // Helper method postType -> string
  getPostTypeName(postType?: number): string {
    if (postType === undefined || postType === null) return '';
    const types: { [key: number]: string } = {
      0: 'Gyermek',
      1: 'Közelet',
      2: 'Sport'
    };
    return types[postType] || '';
  }

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.fetchPosts();
    }
  }

  fetchPosts(): void {
    this.loading = true;
    this.errorMessage = '';

    this.blogsService
      .getAll()
      .pipe(
        finalize(() => {
          this.loading = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: (posts) => {
          this.posts = posts ?? [];
          this.loading = false;
          if (this.posts.length === 0) {
            console.info('Blog fetch: 0 posts');
          } else {
            console.info(`Blog fetch: ${this.posts.length} post(s)`);
          }
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.errorMessage = 'Error while getting the blogs.';
          this.loading = false;
          this.cdr.detectChanges();
        },
      });
  }

  deleteBlog(id: number): void {
    // Megerosito popup
    if (confirm('Do you want to delete the post?')) {
      this.blogsService.delete(id).subscribe({
        next: () => {
          this.fetchPosts();
        },
        error: (err: any) => {
          this.errorMessage = 'Eerror while deleting blogs.';
          this.cdr.detectChanges();
          console.error('Delete error:', err);
        },
      });
    }
  }

  editBlog(id: number): void {
    this.router.navigate(['/edit-blog', id]);
  }

  toggleExpand(postId: number): void {
    if (this.expandedPosts.has(postId)) {
      this.expandedPosts.delete(postId);
    } else {
      this.expandedPosts.add(postId);
    }
  }

  isExpanded(postId: number): boolean {
    return this.expandedPosts.has(postId);
  }

  getShortContent(content: string, maxLength: number = 200): string {
    if (content.length <= maxLength) {
      return content;
    }
    return content.substring(0, maxLength) + '...';
  }
}
