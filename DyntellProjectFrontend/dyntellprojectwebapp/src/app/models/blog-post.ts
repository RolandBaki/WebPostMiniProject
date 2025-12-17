import { Comment } from './comment';

export interface BlogPost {
  id: number;
  title: string;
  content: string;
  postType?: number; // Backend enum (1=Gyermek, 2=Kozelet, 3=Sport)
  createdDate?: string;
  createdByUsername?: string;
  comments?: Comment[]; // Comments array -> backend
}

