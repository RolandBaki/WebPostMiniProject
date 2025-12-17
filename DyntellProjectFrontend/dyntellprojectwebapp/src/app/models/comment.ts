export interface Comment {
  id: number;
  content: string;
  createdDate: string;
  username: string;
  parentCommentId?: number | null;
  depth: number;
  replies: Comment[];
}

