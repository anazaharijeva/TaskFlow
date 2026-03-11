import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { api } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import { ProjectCard } from '../components/ProjectCard';
import type { Project, DashboardMetrics, PeriodStats, Streak, GoalProgress } from '../types';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';

/**
 * Dashboard - Main page showing projects and productivity metrics.
 * Uses Recharts for the productivity visualization.
 */
export function Dashboard() {
  const location = useLocation();
  const { userId } = useAuth();
  const [projects, setProjects] = useState<Project[]>([]);
  const [metrics, setMetrics] = useState<DashboardMetrics | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [newName, setNewName] = useState('');
  const [newDesc, setNewDesc] = useState('');
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [seeding, setSeeding] = useState(false);
  const [includeArchived, setIncludeArchived] = useState(false);
  const [weekly, setWeekly] = useState<PeriodStats | null>(null);
  const [monthly, setMonthly] = useState<PeriodStats | null>(null);
  const [streak, setStreak] = useState<Streak | null>(null);
  const [goal, setGoal] = useState<GoalProgress | null>(null);

  const fetchData = async () => {
    try {
      const [projectsRes, metricsRes, weeklyRes, monthlyRes, streakRes, goalRes] = await Promise.all([
        api.get<Project[]>(`/projects?includeArchived=${includeArchived}`),
        api.get<DashboardMetrics>('/analytics/dashboard'),
        api.get<PeriodStats>('/analytics/weekly').catch(() => ({ data: null })),
        api.get<PeriodStats>('/analytics/monthly').catch(() => ({ data: null })),
        api.get<Streak>('/analytics/streak').catch(() => ({ data: null })),
        api.get<GoalProgress>('/analytics/goal?target=10').catch(() => ({ data: null })),
      ]);
      setProjects(projectsRes.data);
      setMetrics(metricsRes.data);
      setWeekly(weeklyRes.data);
      setMonthly(monthlyRes.data);
      setStreak(streakRes.data);
      setGoal(goalRes.data);
    } catch {
      setProjects([]);
      setMetrics(null);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [location.pathname, includeArchived]);

  const handleSeedDemo = async () => {
    setSeeding(true);
    setCreateError(null);
    try {
      const { data } = await api.post<{ projectId: string; projectName: string; tasksCreated: number }>('/seed/demo');
      setShowCreate(false);
      await fetchData();
      window.location.href = `/projects/${data.projectId}`;
    } catch (err: unknown) {
      const res = (err as { response?: { data?: { message?: string } } })?.response;
      setCreateError(res?.data?.message ?? 'Failed to create demo project.');
    } finally {
      setSeeding(false);
    }
  };

  const handleArchive = async (project: Project) => {
    try {
      await api.put(`/projects/${project.id}`, {
        name: project.name,
        description: project.description ?? '',
        isArchived: !project.isArchived,
      });
      fetchData();
    } catch {
      // Error handled by interceptor
    }
  };

  const handleDelete = async (project: Project) => {
    try {
      await api.delete(`/projects/${project.id}`);
      fetchData();
    } catch {
      // Error handled by interceptor
    }
  };

  const handleCreateProject = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newName.trim()) return;
    setCreating(true);
    setCreateError(null);
    try {
      const { data } = await api.post<Project>('/projects', {
        name: newName.trim(),
        description: newDesc.trim(),
      });
      setProjects((prev) => [...prev, data]);
      setShowCreate(false);
      setNewName('');
      setNewDesc('');
      try {
        await fetchData();
      } catch {
        // Refresh failed but project was created - keep it in state
      }
    } catch (err: unknown) {
      const res = (err as { response?: { data?: { message?: string }; status?: number } })?.response;
      if (res?.status === 401) {
        setCreateError('Session expired. Please log in again.');
      } else {
        setCreateError(res?.data?.message ?? 'Failed to create project. Please try again.');
      }
    } finally {
      setCreating(false);
    }
  };

  if (loading) {
    return <div style={styles.loading}>Loading...</div>;
  }

  // Chart data: per project - Todo, In Progress, Completed (stacked bars)
  const chartData =
    metrics?.projectStats?.map((p) => ({
      name: p.projectName,
      Todo: p.todo,
      'In Progress': p.inProgress,
      Completed: p.completed,
    })) ?? [];

  return (
    <div style={styles.container}>
      <h1 style={styles.h1}>Dashboard</h1>

      {/* Metrics widgets */}
      <div style={styles.widgets}>
        <div style={styles.widget}>
          <span style={styles.widgetLabel}>Projects</span>
          <span style={styles.widgetValue}>{metrics?.totalProjects ?? 0}</span>
        </div>
        <div style={styles.widget}>
          <span style={styles.widgetLabel}>Tasks Completed</span>
          <span style={styles.widgetValue}>{metrics?.tasksCompleted ?? 0}</span>
        </div>
        <div style={styles.widget}>
          <span style={styles.widgetLabel}>Tasks In Progress</span>
          <span style={styles.widgetValue}>{metrics?.tasksInProgress ?? 0}</span>
        </div>
        <div style={styles.widget}>
          <span style={styles.widgetLabel}>Productivity Score</span>
          <span style={styles.widgetValue}>
            {metrics?.productivityScore ?? '0%'}
          </span>
        </div>
        {weekly && (
          <div style={styles.widget}>
            <span style={styles.widgetLabel}>{weekly.periodLabel}</span>
            <span style={styles.widgetValue}>{weekly.completed} completed</span>
          </div>
        )}
        {monthly && (
          <div style={styles.widget}>
            <span style={styles.widgetLabel}>{monthly.periodLabel}</span>
            <span style={styles.widgetValue}>{monthly.completed} completed</span>
          </div>
        )}
        {streak && (
          <div style={styles.widget}>
            <span style={styles.widgetLabel}>Streak</span>
            <span style={styles.widgetValue}>{streak.currentStreakDays} days</span>
          </div>
        )}
        {goal && (
          <div style={{ ...styles.widget, ...(goal.goalMet ? { borderColor: '#4ade80' } : {}) }}>
            <span style={styles.widgetLabel}>Today&apos;s goal ({goal.targetPerDay})</span>
            <span style={styles.widgetValue}>{goal.completedToday}/{goal.targetPerDay}</span>
          </div>
        )}
      </div>

      {/* Chart - per project: Todo, In Progress, Completed */}
      {chartData.length > 0 && (
        <div style={styles.chartContainer}>
          <h2 style={styles.h2}>Task Status Overview</h2>
          <p style={styles.chartSubtitle}>
            Task breakdown by project (Todo, In Progress, Completed)
          </p>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#333" />
              <XAxis dataKey="name" stroke="#888" />
              <YAxis stroke="#888" />
              <Tooltip
                contentStyle={{ backgroundColor: '#1a1a2e', border: '1px solid #333' }}
              />
              <Legend />
              <Bar dataKey="Todo" stackId="a" fill="#94a3b8" radius={[0, 0, 0, 0]} />
              <Bar dataKey="In Progress" stackId="a" fill="#fbbf24" radius={[0, 0, 0, 0]} />
              <Bar dataKey="Completed" stackId="a" fill="#4ade80" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Projects list */}
      <div style={styles.projectsHeader}>
        <h2 style={styles.h2}>Your Projects</h2>
        <label style={styles.archiveLabel}>
          <input
            type="checkbox"
            checked={includeArchived}
            onChange={(e) => setIncludeArchived(e.target.checked)}
          />
          Show archived
        </label>
        <button
          onClick={handleSeedDemo}
          disabled={seeding}
          style={styles.seedButton}
          title="Create a demo project with 5 sample tasks"
        >
          {seeding ? 'Creating...' : 'Create Demo Project'}
        </button>
        <button
          onClick={() => { setShowCreate(!showCreate); setCreateError(null); }}
          style={styles.addButton}
        >
          {showCreate ? 'Cancel' : '+ New Project'}
        </button>
      </div>
      {showCreate && (
        <form onSubmit={handleCreateProject} style={styles.createForm}>
          {createError && (
            <p style={styles.errorMsg}>{createError}</p>
          )}
          <input
            type="text"
            placeholder="Project name"
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            required
            style={styles.input}
          />
          <input
            type="text"
            placeholder="Description (optional)"
            value={newDesc}
            onChange={(e) => setNewDesc(e.target.value)}
            style={styles.input}
          />
          <button type="submit" disabled={creating} style={styles.submitButton}>
            Create
          </button>
        </form>
      )}
      <div style={styles.projects}>
        {projects.map((p) => (
          <ProjectCard
            key={p.id}
            project={p}
            onArchive={handleArchive}
            onDelete={p.ownerId === userId ? handleDelete : undefined}
          />
        ))}
        {projects.length === 0 && (
          <p style={styles.empty}>No projects yet. Create one to get started!</p>
        )}
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: {
    padding: '2rem',
    maxWidth: '1200px',
    margin: '0 auto',
  },
  loading: {
    padding: '2rem',
    textAlign: 'center',
    color: 'var(--text-muted)',
  },
  h1: {
    margin: '0 0 1.5rem',
    color: 'var(--text)',
  },
  h2: {
    margin: '2rem 0 1rem',
    color: 'var(--text-muted)',
    fontSize: '1.25rem',
  },
  widgets: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
    gap: '1rem',
    marginBottom: '2rem',
  },
  widget: {
    padding: '1.25rem',
    backgroundColor: 'var(--bg-widget)',
    borderRadius: '8px',
    border: '1px solid var(--border)',
    display: 'flex',
    flexDirection: 'column',
    gap: '0.5rem',
  },
  widgetLabel: {
    fontSize: '0.85rem',
    color: 'var(--text-muted)',
  },
  widgetValue: {
    fontSize: '1.5rem',
    fontWeight: 'bold',
    color: 'var(--text)',
  },
  chartContainer: {
    padding: '1.5rem',
    backgroundColor: 'var(--bg-widget)',
    borderRadius: '8px',
    marginBottom: '2rem',
  },
  chartSubtitle: {
    margin: '0 0 1rem',
    fontSize: '0.9rem',
    color: 'var(--text-muted)',
  },
  projectsHeader: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    margin: '2rem 0 1rem',
  },
  archiveLabel: { display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-muted)', fontSize: '0.9rem' },
  seedButton: {
    padding: '0.5rem 1rem',
    backgroundColor: 'var(--accent-dim)',
    border: '1px solid var(--accent)',
    borderRadius: '4px',
    color: 'var(--text)',
    cursor: 'pointer',
  },
  addButton: {
    padding: '0.5rem 1rem',
    backgroundColor: 'var(--accent)',
    border: 'none',
    borderRadius: '4px',
    color: 'white',
    cursor: 'pointer',
  },
  createForm: {
    display: 'flex',
    flexDirection: 'column',
    gap: '0.5rem',
    marginBottom: '1rem',
  },
  errorMsg: {
    color: 'var(--accent)',
    margin: 0,
    fontSize: '0.9rem',
  },
  input: {
    padding: '0.5rem',
    borderRadius: '4px',
    border: '1px solid var(--border)',
    backgroundColor: 'var(--bg-input)',
    color: 'var(--text)',
    minWidth: '200px',
  },
  submitButton: {
    padding: '0.5rem 1rem',
    backgroundColor: '#4ade80',
    border: 'none',
    borderRadius: '4px',
    color: '#0f0f23',
    cursor: 'pointer',
  },
  projects: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))',
    gap: '1rem',
  },
  empty: {
    color: 'var(--text-muted)',
    gridColumn: '1 / -1',
    textAlign: 'center',
    padding: '2rem',
  },
};
