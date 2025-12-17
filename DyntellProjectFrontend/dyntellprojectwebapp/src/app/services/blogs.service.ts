import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BlogPost } from '../models/blog-post';

const API_URL = 'https://localhost:7201/api';

export interface CreateBlogPostRequest {
  title: string;
  content: string;
  postType: number; // 1=Gyermek, 2=Kozelet, 3=Sport
}

@Injectable({
  providedIn: 'root',
})
export class BlogsService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<BlogPost[]> {
    return this.http.get<BlogPost[]>(`${API_URL}/Blog`);
  }

  create(blogPost: CreateBlogPostRequest): Observable<BlogPost> {
    return this.http.post<BlogPost>(`${API_URL}/Blog`, {
      title: blogPost.title,
      content: blogPost.content,
      postType: blogPost.postType
    });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${API_URL}/Blog/${id}`);
  }

  getById(id: number): Observable<BlogPost> {
    return this.http.get<BlogPost>(`${API_URL}/Blog/${id}`);
  }

  update(id: number, blogPost: CreateBlogPostRequest): Observable<BlogPost> {
    return this.http.put<BlogPost>(`${API_URL}/Blog/${id}`, {
      title: blogPost.title,
      content: blogPost.content,
      postType: blogPost.postType
    });
  }
}

