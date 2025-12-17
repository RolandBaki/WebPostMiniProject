import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Comment } from '../models/comment';

const API_URL = 'https://localhost:7201/api';

export interface CreateCommentRequest {
  content: string;
  blogPostId: number;
  parentCommentId?: number | null;
}

@Injectable({
  providedIn: 'root',
})
export class CommentService {
  constructor(private http: HttpClient) {}

  create(comment: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${API_URL}/Comment`, {
      content: comment.content,
      blogPostId: comment.blogPostId,
      parentCommentId: comment.parentCommentId ?? null
    });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/Comment/${id}`);
  }
}

