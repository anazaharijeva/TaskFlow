import { useState } from 'react';
import type { Task, TaskStatus } from '../types';

const STATUS_OPTIONS: TaskStatus[] = ['Todo', 'InProgress', 'Completed'];
const PRIORITY_LABELS: Record<number, string> = { 0: 'Low', 1: 'Medium', 2: 'High', 3: 'Urgent' };

function normalizeStatus(s: TaskStatus | number): TaskStatus {
  if (typeof s === 'number') return STATUS_OPTIONS[s] ?? 'Todo';
  if (s === 'Todo' || s === 'InProgress' || s === 'Completed') return s;
  return 'Todo';
}

function statusLabel(status: TaskStatus): string {
  return status === 'InProgress' ? 'In Progress' : status;
}

function priorityLabel(p: Task['priority']): string {
  if (typeof p === 'number') return PRIORITY_LABELS[p] ?? 'Medium';
  return String(p ?? 'Medium');
}

/**
 * TaskList - Edit button opens dropdown, Confirm saves changes.
 * When marking as Completed, shows optional completion feedback field.
 */
interface TaskListProps {
  tasks: Task[];
  onStatusChange?: (taskId: string, status: TaskStatus, completionNote?: string) => void;
}

export function TaskList({ tasks, onStatusChange }: TaskListProps) {
  const [editingId, setEditingId] = useState<string | null>(null);
  const [draftStatus, setDraftStatus] = useState<Record<string, TaskStatus>>({});
  const [completionNote, setCompletionNote] = useState<string>('');

  const handleEdit = (task: Task) => {
    setEditingId(task.id);
    setDraftStatus((prev) => ({ ...prev, [task.id]: normalizeStatus(task.status) }));
    setCompletionNote(task.completionNote ?? '');
  };

  const handleConfirm = (taskId: string) => {
    const status = draftStatus[taskId];
    if (status) {
      const note = status === 'Completed' ? completionNote.trim() || undefined : undefined;
      onStatusChange?.(taskId, status, note);
    }
    setEditingId(null);
    setDraftStatus((prev) => {
      const next = { ...prev };
      delete next[taskId];
      return next;
    });
    setCompletionNote('');
  };

  const handleCancel = () => {
    setEditingId(null);
    setDraftStatus({});
    setCompletionNote('');
  };

  return (
    <div style={styles.container}>
      {tasks.length === 0 ? (
        <p style={styles.empty}>No tasks yet. Create one to get started!</p>
      ) : (
        tasks.map((t) => {
          const status = normalizeStatus(t.status);
          const isEditing = editingId === t.id;
          const currentStatus = isEditing ? (draftStatus[t.id] ?? status) : status;

          return (
            <div key={t.id} style={{ ...styles.card, ...getStatusBorder(currentStatus) }}>
              <div style={styles.header}>
                <h3 style={styles.title}>{t.title}</h3>
                <span style={{ ...styles.statusBadge, ...getStatusBadgeStyle(currentStatus) }}>
                  {statusLabel(currentStatus)}
                </span>
                <div style={styles.actions}>
                  {isEditing ? (
                    <>
                      <select
                        value={currentStatus}
                        onChange={(e) =>
                          setDraftStatus((prev) => ({
                            ...prev,
                            [t.id]: e.target.value as TaskStatus,
                          }))
                        }
                        style={styles.select}
                        autoFocus
                      >
                        {STATUS_OPTIONS.map((s) => (
                          <option key={s} value={s}>
                            {s}
                          </option>
                        ))}
                      </select>
                      {currentStatus === 'Completed' && (
                        <div style={styles.completionNoteWrap}>
                          <label style={styles.completionLabel}>Completion feedback (optional):</label>
                          <textarea
                            value={completionNote}
                            onChange={(e) => setCompletionNote(e.target.value)}
                            placeholder="Add a note about how the task was completed..."
                            style={styles.completionTextarea}
                            rows={2}
                          />
                        </div>
                      )}
                      <button
                        type="button"
                        onClick={() => handleConfirm(t.id)}
                        style={styles.confirmBtn}
                      >
                        Confirm
                      </button>
                      <button
                        type="button"
                        onClick={handleCancel}
                        style={styles.cancelBtn}
                      >
                        Cancel
                      </button>
                    </>
                  ) : (
                    <button
                      type="button"
                      onClick={() => handleEdit(t)}
                      style={styles.editBtn}
                    >
                      Edit
                    </button>
                  )}
                </div>
              </div>
              <div style={styles.meta}>
                {(t.priority != null || t.tags) && (
                  <span style={styles.metaTags}>
                    {t.priority != null && (
                      <span style={{ ...styles.priorityBadge, ...getPriorityStyle(t.priority) }}>
                        {priorityLabel(t.priority)}
                      </span>
                    )}
                    {t.tags && (
                      <span style={styles.tags}>{t.tags}</span>
                    )}
                  </span>
                )}
                {t.assigneeName && (
                  <span style={styles.assignee}>@{t.assigneeName}</span>
                )}
              </div>
              {t.description && (
                <p style={styles.description}>{t.description}</p>
              )}
              {(t.startedByName || t.completedByName) && (
                <div style={styles.meta}>
                  {t.startedByName && (
                    <span style={styles.metaBy}>
                      Started by {t.startedByName}
                      {t.startedAt && (
                        <span style={styles.metaDate}>
                          {' '}({new Date(t.startedAt).toLocaleDateString()})
                        </span>
                      )}
                    </span>
                  )}
                  {t.completedByName && (
                    <span style={styles.metaBy}>
                      Completed by {t.completedByName}
                      {t.completedAt && (
                        <span style={styles.metaDate}>
                          {' '}({new Date(t.completedAt).toLocaleDateString()})
                        </span>
                      )}
                    </span>
                  )}
                </div>
              )}
              {t.completionNote && (
                <p style={styles.completionNoteDisplay}>
                  <strong>Completion note:</strong> {t.completionNote}
                </p>
              )}
            </div>
          );
        })
      )}
    </div>
  );
}

function getStatusBorder(status: TaskStatus): React.CSSProperties {
  const colors: Record<TaskStatus, string> = {
    Todo: '#94a3b8',
    InProgress: '#fbbf24',
    Completed: '#4ade80',
  };
  return { borderLeftColor: colors[status] ?? '#94a3b8' };
}

function getPriorityStyle(p: Task['priority']): React.CSSProperties {
  const n = typeof p === 'number' ? p : 1;
  const colors: Record<number, string> = { 0: '#94a3b8', 1: '#60a5fa', 2: '#fbbf24', 3: '#ef4444' };
  return { backgroundColor: `${colors[n] ?? '#60a5fa'}33`, color: colors[n] ?? '#60a5fa' };
}

function getStatusBadgeStyle(status: TaskStatus): React.CSSProperties {
  const styles: Record<TaskStatus, React.CSSProperties> = {
    Todo: { backgroundColor: 'rgba(148,163,184,0.2)', color: '#94a3b8' },
    InProgress: { backgroundColor: 'rgba(251,191,36,0.2)', color: '#fbbf24' },
    Completed: { backgroundColor: 'rgba(74,222,128,0.2)', color: '#4ade80' },
  };
  return styles[status] ?? styles.Todo;
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '0.75rem',
  },
  card: {
    padding: '1rem',
    backgroundColor: 'var(--bg-widget)',
    borderRadius: '8px',
    borderLeft: '4px solid var(--accent)',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    gap: '1rem',
  },
  title: {
    margin: 0,
    fontSize: '1.1rem',
    flex: 1,
  },
  statusBadge: {
    padding: '0.25rem 0.6rem',
    borderRadius: '4px',
    fontSize: '0.85rem',
    fontWeight: 500,
  },
  actions: {
    display: 'flex',
    alignItems: 'center',
    gap: '0.5rem',
  },
  select: {
    padding: '0.5rem 0.75rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
    fontSize: '0.9rem',
    cursor: 'pointer',
    minWidth: '120px',
  },
  editBtn: {
    padding: '0.4rem 0.8rem',
    backgroundColor: 'var(--accent-dim)',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    color: 'var(--text)',
    fontSize: '0.85rem',
    cursor: 'pointer',
  },
  confirmBtn: {
    padding: '0.4rem 0.8rem',
    backgroundColor: '#4ade80',
    border: 'none',
    borderRadius: '4px',
    color: '#0f0f23',
    fontSize: '0.85rem',
    cursor: 'pointer',
  },
  cancelBtn: {
    padding: '0.4rem 0.8rem',
    backgroundColor: 'transparent',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    color: 'var(--text-muted)',
    fontSize: '0.85rem',
    cursor: 'pointer',
  },
  meta: { display: 'flex', gap: '0.5rem', alignItems: 'center', marginTop: '0.5rem', flexWrap: 'wrap' },
  metaTags: { display: 'flex', gap: '0.5rem', alignItems: 'center' },
  metaBy: { fontSize: '0.8rem', color: 'var(--text-muted)' },
  metaDate: { fontSize: '0.75rem', opacity: 0.8 },
  priorityBadge: { padding: '0.2rem 0.5rem', borderRadius: '4px', fontSize: '0.75rem' },
  tags: { fontSize: '0.85rem', color: '#94a3b8' },
  assignee: { fontSize: '0.85rem', color: '#a78bfa' },
  completionNoteWrap: { width: '100%', marginTop: '0.5rem' },
  completionLabel: { display: 'block', fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '0.25rem' },
  completionTextarea: {
    width: '100%',
    padding: '0.5rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
    fontSize: '0.9rem',
    resize: 'vertical',
  },
  completionNoteDisplay: { margin: '0.5rem 0 0', fontSize: '0.9rem', color: 'var(--text-dim)', fontStyle: 'italic' },
  description: {
    margin: '0.5rem 0 0',
    fontSize: '0.9rem',
    color: 'var(--text-dim)',
  },
  empty: {
    color: 'var(--text-muted)',
    textAlign: 'center',
    padding: '2rem',
  },
};
