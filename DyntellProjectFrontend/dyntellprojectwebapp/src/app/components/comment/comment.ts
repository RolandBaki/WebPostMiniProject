import { Component, Input, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CommentService } from '../../services/comment.service';
import { AuthService } from '../../services/auth.service';
import { BlogsService } from '../../services/blogs.service';
import { Comment } from '../../models/comment';
import { BlogPost } from '../../models/blog-post';

@Component({
  selector: 'app-comments',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './comment.html',
  styleUrl: './comment.css',
})
export class Comments implements OnInit {
  @Input() postId!: number;
  @Input() post?: BlogPost;
  
  comments: Comment[] = [];
  newCommentContent: string = '';
  showCommentForm: boolean = false; // Komment form megjelenitese
  replyingTo: number | null = null; // Melyik kommentre valaszolunk
  replyContent: { [key: number]: string } = {}; // Valasz tartalma komment ID alapjan
  isLoading: boolean = false;
  errorMessage: string = '';
  MAX_DEPTH = 3; // Maximum 3 szint valaszok

  constructor(
    private commentService: CommentService,
    private blogsService: BlogsService,
    public authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Ha van post objektum, akkor a kommenteket onnan olvassuk
    if (this.post?.comments) {
      this.comments = this.post.comments;
    } else {
      // Ha nincs, akkor ujra kell tolteni a blogpostot
      this.loadComments();
    }
  }

  loadComments(): void {
    // Ujra betoltjuk a blogpostot a kommentekkel
    this.blogsService.getById(this.postId).subscribe({
      next: (post) => {
        this.comments = post.comments || [];
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('Error loading comments:', err);
      }
    });
  }

  toggleCommentForm(): void {
    this.showCommentForm = !this.showCommentForm;
    if (!this.showCommentForm) {
      this.newCommentContent = ''; // Torles ha bezarjuk a formot
    }
  }

  onSubmitComment(): void {
    if (!this.newCommentContent.trim()) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.commentService.create({
      content: this.newCommentContent,
      blogPostId: this.postId
    }).subscribe({
      next: () => {
        this.newCommentContent = '';
        this.showCommentForm = false; // Bezarjuk a formot
        this.isLoading = false;
        this.loadComments(); // Frissitjuk a kommenteket
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Error while Creating a Comment.';
        this.cdr.detectChanges();
        console.error('Create comment error:', error);
      }
    });
  }

  onSubmitReply(parentCommentId: number): void {
    const replyText = this.replyContent[parentCommentId];
    if (!replyText || !replyText.trim()) {
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.commentService.create({
      content: replyText,
      blogPostId: this.postId,
      parentCommentId: parentCommentId
    }).subscribe({
      next: () => {
        this.replyContent[parentCommentId] = '';
        this.replyingTo = null;
        this.isLoading = false;
        this.loadComments(); // Frissitjuk a kommenteket
        this.cdr.detectChanges();
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Error while Creating response.';
        this.cdr.detectChanges();
        console.error('Create reply error:', error);
      }
    });
  }

  toggleReply(commentId: number): void {
    if (this.replyingTo === commentId) {
      this.replyingTo = null;
      delete this.replyContent[commentId];
    } else {
      this.replyingTo = commentId;
      if (!this.replyContent[commentId]) {
        this.replyContent[commentId] = '';
      }
    }
  }

  canReply(comment: Comment): boolean {
    return comment.depth < this.MAX_DEPTH;
  }

  deleteComment(id: number): void {
    if (confirm('Do you want to delete this comment?')) {
      this.commentService.delete(id).subscribe({
        next: () => {
          this.loadComments(); // Frissitjuk a kommenteket
          this.cdr.detectChanges();
        },
        error: (error) => {
          this.errorMessage = 'Error while deleting Comment.';
          this.cdr.detectChanges();
          console.error('Delete comment error:', error);
        }
      });
    }
  }

  isCommentOwner(comment: Comment): boolean {
    const currentUsername = this.authService.getUserInfo().username;
    return comment.username === currentUsername;
  }
}
