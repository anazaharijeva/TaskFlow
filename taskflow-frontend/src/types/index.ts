/**
 * Shared TypeScript types for the TaskFlow application.
 * These mirror the DTOs from the backend API.
 */

export type TaskStatus = 'Todo' | 'InProgress' | 'Completed';
export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export interface Task {
  id: string;
  title: string;
  description: string;
  status: TaskStatus | number;
  createdAt: string;
  dueDate?: string;
  projectId: string;
  priority: TaskPriority | number;
  tags: string;
  assigneeId?: string;
  assigneeName?: string;
  completionNote?: string;
  startedByName?: string;
  startedAt?: string;
  completedByName?: string;
  completedAt?: string;
}

export interface Project {
  id: string;
  name: string;
  description: string;
  ownerId: string;
  taskCount: number;
  isArchived?: boolean;
}

export interface TaskFilter {
  status?: number;
  priority?: number;
  tag?: string;
  sortBy?: string;
  sortDesc?: boolean;
}

export interface ProjectMember {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  role: string;
}

export interface TaskComment {
  id: string;
  content: string;
  createdAt: string;
  userId: string;
  userName: string;
}

export interface PeriodStats {
  completed: number;
  total: number;
  periodLabel: string;
}

export interface Streak {
  currentStreakDays: number;
  message: string;
}

export interface GoalProgress {
  targetPerDay: number;
  completedToday: number;
  goalMet: boolean;
  message: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
}

export interface ProjectTaskStats {
  projectName: string;
  todo: number;
  inProgress: number;
  completed: number;
}

export interface DashboardMetrics {
  totalProjects: number;
  tasksCompleted: number;
  tasksInProgress: number;
  tasksTodo: number;
  productivityScore: string;
  projectStats: ProjectTaskStats[];
}
