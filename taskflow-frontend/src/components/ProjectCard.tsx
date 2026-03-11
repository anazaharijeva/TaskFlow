import { Link } from 'react-router-dom';
import type { Project } from '../types';

/**
 * ProjectCard - Displays a project summary with link to project page.
 */
interface ProjectCardProps {
  project: Project;
  onArchive?: (project: Project) => void;
  onDelete?: (project: Project) => void;
}

export function ProjectCard({ project, onArchive, onDelete }: ProjectCardProps) {
  const handleArchive = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    onArchive?.(project);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (window.confirm(`Delete project "${project.name}"? This cannot be undone.`)) {
      onDelete?.(project);
    }
  };

  return (
    <div style={styles.wrapper}>
      <Link to={`/projects/${project.id}`} style={styles.link}>
        <div style={styles.card}>
          <h3 style={styles.name}>{project.name}</h3>
          <p style={styles.description}>
            {project.description || 'No description'}
          </p>
          <span style={styles.badge}>{project.taskCount} tasks</span>
          {project.isArchived && (
            <span style={styles.archivedBadge}>Archived</span>
          )}
        </div>
      </Link>
      <div style={styles.actions}>
        {onArchive && (
          <button
            type="button"
            onClick={handleArchive}
            style={styles.archiveBtn}
            title={project.isArchived ? 'Unarchive' : 'Archive'}
          >
            {project.isArchived ? 'Unarchive' : 'Archive'}
          </button>
        )}
        {onDelete && (
          <button
            type="button"
            onClick={handleDelete}
            style={styles.deleteBtn}
            title="Delete project"
          >
            Delete
          </button>
        )}
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  wrapper: { position: 'relative' },
  link: {
    textDecoration: 'none',
    color: 'inherit',
  },
  card: {
    padding: '1.25rem',
    backgroundColor: 'var(--bg-widget)',
    borderRadius: '8px',
    border: '1px solid var(--border)',
    transition: 'transform 0.2s, box-shadow 0.2s',
  },
  name: {
    margin: 0,
    fontSize: '1.25rem',
    color: 'var(--text)',
  },
  description: {
    margin: '0.5rem 0',
    fontSize: '0.9rem',
    color: 'var(--text-muted)',
  },
  badge: {
    display: 'inline-block',
    marginTop: '0.5rem',
    marginRight: '0.5rem',
    padding: '0.25rem 0.5rem',
    backgroundColor: 'var(--border)',
    borderRadius: '4px',
    fontSize: '0.8rem',
    color: 'var(--text)',
  },
  actions: {
    position: 'absolute',
    top: '0.5rem',
    right: '0.5rem',
    display: 'flex',
    gap: '0.5rem',
  },
  archiveBtn: {
    padding: '0.25rem 0.5rem',
    fontSize: '0.75rem',
    backgroundColor: 'transparent',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    color: 'var(--text-muted)',
    cursor: 'pointer',
  },
  deleteBtn: {
    padding: '0.25rem 0.5rem',
    fontSize: '0.75rem',
    backgroundColor: 'transparent',
    border: '1px solid #dc2626',
    borderRadius: '4px',
    color: '#ef4444',
    cursor: 'pointer',
  },
  archivedBadge: {
    display: 'inline-block',
    marginTop: '0.5rem',
    padding: '0.25rem 0.5rem',
    backgroundColor: '#64748b',
    borderRadius: '4px',
    fontSize: '0.8rem',
    color: '#94a3b8',
  },
};
