export interface User {
  id: string;
  name: string;
  email: string;
}

export interface Project {
  id: string;
  title: string;
  description: string | null;
  emoji: string | null;
  coverImageUrl: string | null;
  createdAt: string;
  updatedAt: string;
  taskCount: number;
  completedTaskCount: number;
  ownerId: string;
}

export interface Task {
  id: string;
  title: string;
  description: string | null;
  priority: string;
  status: string;
  type: string | null;
  dueDate: string | null;
  completedAt: string | null;
  completedById: string | null;
  completedByName: string | null;
  projectId: string;
  createdById: string;
  createdByName: string;
  createdAt: string;
  updatedAt: string;
}

export interface Comment {
  id: string;
  body: string;
  authorId: string;
  authorName: string;
  taskId: string | null;
  projectId: string | null;
  createdAt: string;
}

export interface ProjectMemberResponse {
  userId: string;
  name: string;
  email: string;
  role: string;
}
