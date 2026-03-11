import { useEffect, useState, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import { useProjectNotifications } from '../hooks/useProjectNotifications';
import { TaskList } from '../components/TaskList';
import type { Task, TaskStatus, TaskFilter, ProjectMember } from '../types';

const STATUS_MAP: Record<TaskStatus, number> = { Todo: 0, InProgress: 1, Completed: 2 };
const PRIORITY_MAP: Record<string, number> = { Low: 0, Medium: 1, High: 2, Urgent: 3 };

export function ProjectPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { userId } = useAuth();
  const [tasks, setTasks] = useState<Task[]>([]);
  const [projectName, setProjectName] = useState('');
  const [projectOwnerId, setProjectOwnerId] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [newTaskTitle, setNewTaskTitle] = useState('');
  const [newTaskPriority, setNewTaskPriority] = useState<number>(1);
  const [newTaskTags, setNewTaskTags] = useState('');
  const [newTaskAssignee, setNewTaskAssignee] = useState<string>('');
  const [assignableUsers, setAssignableUsers] = useState<ProjectMember[]>([]);
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [filter, setFilter] = useState<TaskFilter>({ sortBy: 'CreatedAt', sortDesc: true });
  const [creating, setCreating] = useState(false);
  const [showShare, setShowShare] = useState(false);
  const [newMemberEmail, setNewMemberEmail] = useState('');
  const [importing, setImporting] = useState(false);
  const [notifications, setNotifications] = useState<Array<{ id: string; message: string }>>([]);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const isOwner = userId && projectOwnerId && userId === projectOwnerId;

  const handleTaskNotification = (message: string, data?: { taskId: string; task?: unknown }) => {
    const id = crypto.randomUUID();
    setNotifications((prev) => [...prev, { id, message }]);
    if (data?.task) fetchData();
    setTimeout(() => {
      setNotifications((prev) => prev.filter((n) => n.id !== id));
    }, 5000);
  };

  useProjectNotifications(id ?? undefined, handleTaskNotification);

  const fetchData = async () => {
    if (!id) return;
    try {
      const params = new URLSearchParams();
      if (filter.status != null) params.set('status', String(filter.status));
      if (filter.priority != null) params.set('priority', String(filter.priority));
      if (filter.tag) params.set('tag', filter.tag);
      if (filter.sortBy) params.set('sortBy', filter.sortBy);
      if (filter.sortDesc != null) params.set('sortDesc', String(filter.sortDesc));
      const qs = params.toString();
      const [tasksRes, projectRes, usersRes, membersRes] = await Promise.all([
        api.get<Task[]>(`/tasks/project/${id}${qs ? `?${qs}` : ''}`),
        api.get<{ name: string; ownerId: string }>(`/projects/${id}`),
        api.get<ProjectMember[]>(`/projects/${id}/assignable`).catch(() => ({ data: [] })),
        api.get<ProjectMember[]>(`/projects/${id}/members`).catch(() => ({ data: [] })),
      ]);
      setTasks(tasksRes.data);
      setProjectName(projectRes.data.name);
      setProjectOwnerId(projectRes.data.ownerId);
      setAssignableUsers(usersRes.data ?? []);
      setMembers(membersRes.data ?? []);
    } catch {
      setTasks([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [id, filter.status, filter.priority, filter.tag, filter.sortBy, filter.sortDesc]);

  const handleStatusChange = async (taskId: string, status: TaskStatus, completionNote?: string) => {
    const task = tasks.find((t) => t.id === taskId);
    if (!task) return;
    const statusNum = STATUS_MAP[status];
    const priority = typeof task.priority === 'number' ? task.priority : PRIORITY_MAP[task.priority as string] ?? 1;
    try {
      const { data } = await api.put<Task>(`/tasks/${taskId}`, {
        title: task.title,
        description: task.description ?? '',
        dueDate: task.dueDate ? new Date(task.dueDate).toISOString().slice(0, 10) : null,
        status: statusNum,
        priority,
        tags: task.tags ?? '',
        assigneeId: task.assigneeId || null,
        completionNote: status === 'Completed' ? completionNote : undefined,
      });
      setTasks((prev) => prev.map((t) => (t.id === taskId ? data : t)));
    } catch {
      // Error handled by interceptor
    }
  };

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !newTaskTitle.trim()) return;
    setCreating(true);
    try {
      const { data } = await api.post<Task>('/tasks', {
        title: newTaskTitle.trim(),
        description: '',
        projectId: id,
        priority: newTaskPriority,
        tags: newTaskTags.trim(),
        assigneeId: newTaskAssignee || undefined,
      });
      setTasks((prev) => [...prev, data]);
      setNewTaskTitle('');
      setNewTaskTags('');
      setNewTaskAssignee('');
    } catch {
      // Error handled by interceptor
    } finally {
      setCreating(false);
    }
  };

  const handleExport = async (format: 'json' | 'csv') => {
    if (!id) return;
    try {
      const { data } = await api.get(format === 'json' ? `/tasks/project/${id}/export?format=json` : `/tasks/project/${id}/export?format=csv`, {
        responseType: format === 'csv' ? 'blob' : 'json',
      });
      const blob = format === 'json' ? new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' }) : data as Blob;
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `tasks-${projectName}-${new Date().toISOString().slice(0, 10)}.${format}`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      // Error handled by interceptor
    }
  };

  const handleImport = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !id) return;
    setImporting(true);
    try {
      const text = await file.text();
      const format = file.name.endsWith('.csv') ? 'csv' : 'json';
      const { data } = await api.post<{ imported: number }>(`/tasks/project/${id}/import`, { format, content: text });
      if (data.imported > 0) fetchData();
    } catch {
      // Error handled by interceptor
    } finally {
      setImporting(false);
      e.target.value = '';
    }
  };

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!id || !newMemberEmail.trim()) return;
    try {
      await api.post(`/projects/${id}/members`, { email: newMemberEmail.trim(), role: 'Member' });
      setNewMemberEmail('');
      fetchData();
    } catch {
      // Error handled by interceptor
    }
  };

  const handleRemoveMember = async (memberUserId: string) => {
    if (!id) return;
    try {
      await api.delete(`/projects/${id}/members/${memberUserId}`);
      fetchData();
    } catch {
      // Error handled by interceptor
    }
  };

  const handleDeleteProject = async () => {
    if (!id) return;
    if (!window.confirm(`Delete project "${projectName}"? This cannot be undone.`)) return;
    try {
      await api.delete(`/projects/${id}`);
      navigate('/');
    } catch {
      // Error handled by interceptor
    }
  };

  if (loading) {
    return <div style={styles.loading}>Loading...</div>;
  }

  return (
    <div style={styles.container}>
      {notifications.length > 0 && (
        <div style={styles.notifications}>
          {notifications.map((n) => (
            <div key={n.id} style={styles.notification}>
              {n.message}
            </div>
          ))}
        </div>
      )}
      <div style={styles.header}>
        <h1 style={styles.h1}>{projectName}</h1>
        <div style={styles.headerActions}>
          <button type="button" onClick={() => handleExport('json')} style={styles.smallBtn}>Export JSON</button>
          <button type="button" onClick={() => handleExport('csv')} style={styles.smallBtn}>Export CSV</button>
          <input ref={fileInputRef} type="file" accept=".json,.csv" onChange={handleImport} style={{ display: 'none' }} />
          <button type="button" onClick={() => fileInputRef.current?.click()} disabled={importing} style={styles.smallBtn}>
            {importing ? 'Importing...' : 'Import'}
          </button>
          {isOwner && (
            <>
              <button type="button" onClick={() => setShowShare(!showShare)} style={styles.smallBtn}>
                {showShare ? 'Hide Share' : 'Share Project'}
              </button>
              <button type="button" onClick={handleDeleteProject} style={styles.deleteBtn}>
                Delete Project
              </button>
            </>
          )}
        </div>
      </div>

      {/* Share / Members - only for owner */}
      {showShare && isOwner && (
        <div style={styles.shareSection}>
          <h3 style={styles.h3}>Share with users</h3>
          <form onSubmit={handleAddMember} style={styles.shareForm}>
            <input
              type="email"
              placeholder="User email"
              value={newMemberEmail}
              onChange={(e) => setNewMemberEmail(e.target.value)}
              style={styles.input}
            />
            <button type="submit" style={styles.button}>Add</button>
          </form>
          <div style={styles.membersList}>
            {members.map((m) => (
              <div key={m.id} style={styles.memberRow}>
                <span>{m.userName} ({m.userEmail})</span>
                <button type="button" onClick={() => handleRemoveMember(m.userId)} style={styles.removeBtn}>Remove</button>
              </div>
            ))}
            {members.length === 0 && <p style={styles.muted}>No members. Add by email.</p>}
          </div>
        </div>
      )}

      {/* Filter & Sort */}
      <div style={styles.filterRow}>
        <select
          value={filter.status ?? ''}
          onChange={(e) => setFilter((f) => ({ ...f, status: e.target.value ? Number(e.target.value) : undefined }))}
          style={styles.select}
        >
          <option value="">All statuses</option>
          <option value="0">Todo</option>
          <option value="1">In Progress</option>
          <option value="2">Completed</option>
        </select>
        <select
          value={filter.priority ?? ''}
          onChange={(e) => setFilter((f) => ({ ...f, priority: e.target.value ? Number(e.target.value) : undefined }))}
          style={styles.select}
        >
          <option value="">All priorities</option>
          <option value="0">Low</option>
          <option value="1">Medium</option>
          <option value="2">High</option>
          <option value="3">Urgent</option>
        </select>
        <input
          type="text"
          placeholder="Filter by tag"
          value={filter.tag ?? ''}
          onChange={(e) => setFilter((f) => ({ ...f, tag: e.target.value || undefined }))}
          style={styles.tagInput}
        />
        <select
          value={filter.sortBy ?? 'CreatedAt'}
          onChange={(e) => setFilter((f) => ({ ...f, sortBy: e.target.value }))}
          style={styles.select}
        >
          <option value="CreatedAt">Date</option>
          <option value="DueDate">Due date</option>
          <option value="Priority">Priority</option>
          <option value="Title">Title</option>
        </select>
        <label style={styles.checkLabel}>
          <input
            type="checkbox"
            checked={filter.sortDesc ?? true}
            onChange={(e) => setFilter((f) => ({ ...f, sortDesc: e.target.checked }))}
          />
          Desc
        </label>
      </div>

      <form onSubmit={handleCreateTask} style={styles.form}>
        <input
          type="text"
          placeholder="New task title"
          value={newTaskTitle}
          onChange={(e) => setNewTaskTitle(e.target.value)}
          style={styles.input}
        />
        <select
          value={newTaskPriority}
          onChange={(e) => setNewTaskPriority(Number(e.target.value))}
          style={styles.select}
        >
          <option value="0">Low</option>
          <option value="1">Medium</option>
          <option value="2">High</option>
          <option value="3">Urgent</option>
        </select>
        <input
          type="text"
          placeholder="Tags (comma-separated)"
          value={newTaskTags}
          onChange={(e) => setNewTaskTags(e.target.value)}
          style={styles.tagInput}
        />
        <select
          value={newTaskAssignee}
          onChange={(e) => setNewTaskAssignee(e.target.value)}
          style={styles.select}
        >
          <option value="">Unassigned</option>
          {assignableUsers.map((u) => (
            <option key={u.userId} value={u.userId}>{u.userName}</option>
          ))}
        </select>
        <button type="submit" disabled={creating} style={styles.button}>
          Add Task
        </button>
      </form>

      <h2 style={styles.h2}>Tasks</h2>
      <TaskList tasks={tasks} onStatusChange={handleStatusChange} />
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    padding: '2rem',
    maxWidth: '800px',
    margin: '0 auto',
  },
  loading: {
    padding: '2rem',
    textAlign: 'center',
    color: 'var(--text-muted)',
  },
  notifications: {
    position: 'fixed',
    top: '80px',
    right: '1rem',
    zIndex: 1000,
    display: 'flex',
    flexDirection: 'column',
    gap: '0.5rem',
  },
  notification: {
    padding: '0.75rem 1rem',
    backgroundColor: 'var(--accent)',
    color: 'white',
    borderRadius: '8px',
    fontSize: '0.9rem',
    boxShadow: '0 4px 12px rgba(0,0,0,0.2)',
    animation: 'slideIn 0.3s ease',
  },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem', marginBottom: '1.5rem' },
  headerActions: { display: 'flex', gap: '0.5rem', flexWrap: 'wrap' },
  smallBtn: {
    padding: '0.4rem 0.8rem',
    fontSize: '0.9rem',
    backgroundColor: 'var(--accent-dim)',
    border: '1px solid var(--accent)',
    borderRadius: '4px',
    color: 'var(--text)',
    cursor: 'pointer',
  },
  shareSection: {
    padding: '1rem',
    marginBottom: '1.5rem',
    backgroundColor: 'var(--bg-card)',
    borderRadius: '8px',
    border: '1px solid var(--border)',
  },
  shareForm: { display: 'flex', gap: '0.5rem', marginBottom: '1rem' },
  membersList: { display: 'flex', flexDirection: 'column', gap: '0.5rem' },
  memberRow: { display: 'flex', justifyContent: 'space-between', alignItems: 'center' },
  removeBtn: { padding: '0.25rem 0.5rem', fontSize: '0.8rem', backgroundColor: 'transparent', border: '1px solid var(--border)', borderRadius: '4px', color: 'var(--text-muted)', cursor: 'pointer' },
  deleteBtn: {
    padding: '0.4rem 0.8rem',
    fontSize: '0.9rem',
    backgroundColor: 'transparent',
    border: '1px solid #dc2626',
    borderRadius: '4px',
    color: '#ef4444',
    cursor: 'pointer',
  },
  muted: { color: 'var(--text-muted)', margin: 0, fontSize: '0.9rem' },
  h3: { margin: '0 0 0.75rem', fontSize: '1rem', color: 'var(--text)' },
  h1: {
    margin: 0,
    color: 'var(--text)',
  },
  h2: {
    margin: '1.5rem 0 1rem',
    color: 'var(--text-muted)',
    fontSize: '1.25rem',
  },
  filterRow: {
    display: 'flex',
    gap: '0.5rem',
    marginBottom: '1rem',
    flexWrap: 'wrap',
  },
  form: {
    display: 'flex',
    gap: '0.5rem',
    marginBottom: '1.5rem',
    flexWrap: 'wrap',
  },
  select: {
    padding: '0.5rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
    minWidth: '100px',
  },
  tagInput: {
    padding: '0.5rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
    minWidth: '120px',
  },
  checkLabel: { display: 'flex', alignItems: 'center', gap: '0.25rem', color: 'var(--text-muted)', fontSize: '0.9rem' },
  input: {
    flex: 1,
    padding: '0.75rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
  },
  button: {
    padding: '0.75rem 1.25rem',
    backgroundColor: 'var(--accent)',
    border: 'none',
    borderRadius: '4px',
    color: 'white',
    cursor: 'pointer',
  },
};
