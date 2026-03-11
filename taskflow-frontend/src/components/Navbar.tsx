import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { useTheme, type AccentColor } from '../contexts/ThemeContext';

/**
 * Navigation bar - shows app title, theme toggle, and logout when authenticated.
 */
export function Navbar() {
  const { isAuthenticated, userName, logout } = useAuth();
  const { mode, accent, setMode, setAccent } = useTheme();
  const navigate = useNavigate();
  const [showTheme, setShowTheme] = useState(false);

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav style={styles.nav}>
      <Link to="/dashboard" style={styles.logo}>
        TaskFlow
      </Link>
      {isAuthenticated && (
        <div style={styles.right}>
          <span style={styles.userName}>{userName || '...'}</span>
          <Link to="/dashboard" style={styles.link}>Dashboard</Link>
          <div style={styles.themeWrapper}>
            <button
              type="button"
              onClick={() => setShowTheme(!showTheme)}
              style={styles.themeBtn}
              title="Theme"
              aria-label="Theme settings"
            >
              {mode === 'dark' ? '🌙' : '☀️'}
            </button>
            {showTheme && (
              <div style={styles.themeDropdown}>
                <div style={styles.themeRow}>
                  <span style={styles.themeLabel}>Mode:</span>
                  <button
                    type="button"
                    onClick={() => { setMode('dark'); setShowTheme(false); }}
                    style={{ ...styles.themeOpt, ...(mode === 'dark' ? styles.themeOptActive : {}) }}
                  >
                    Dark
                  </button>
                  <button
                    type="button"
                    onClick={() => { setMode('light'); setShowTheme(false); }}
                    style={{ ...styles.themeOpt, ...(mode === 'light' ? styles.themeOptActive : {}) }}
                  >
                    Light
                  </button>
                </div>
                <div style={styles.themeRow}>
                  <span style={styles.themeLabel}>Color:</span>
                  {(['coral', 'blue', 'green', 'purple'] as AccentColor[]).map((c) => (
                    <button
                      key={c}
                      type="button"
                      onClick={() => { setAccent(c); setShowTheme(false); }}
                      style={{
                        ...styles.themeOpt,
                        ...(accent === c ? styles.themeOptActive : {}),
                      }}
                    >
                      {c}
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
          <button onClick={handleLogout} style={styles.button}>
            Logout
          </button>
        </div>
      )}
    </nav>
  );
}

const styles: Record<string, React.CSSProperties> = {
  nav: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: '1rem 2rem',
    backgroundColor: 'var(--bg-card)',
    borderBottom: '1px solid var(--border)',
    color: 'var(--text)',
  },
  logo: {
    fontSize: '1.5rem',
    fontWeight: 'bold',
    color: 'var(--text)',
    textDecoration: 'none',
  },
  right: {
    display: 'flex',
    gap: '1rem',
    alignItems: 'center',
  },
  userName: {
    fontSize: '0.95rem',
    fontWeight: 500,
    color: 'var(--text)',
    marginRight: '0.25rem',
  },
  link: {
    color: 'var(--text)',
    textDecoration: 'none',
  },
  themeWrapper: { position: 'relative' },
  themeBtn: {
    padding: '0.4rem 0.6rem',
    backgroundColor: 'transparent',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    cursor: 'pointer',
    fontSize: '1.25rem',
  },
  themeDropdown: {
    position: 'absolute',
    top: '100%',
    right: 0,
    marginTop: '0.5rem',
    padding: '0.75rem',
    backgroundColor: 'var(--bg-card)',
    border: '1px solid var(--border)',
    borderRadius: '8px',
    boxShadow: '0 4px 12px rgba(0,0,0,0.2)',
    zIndex: 100,
  },
  themeRow: { display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' },
  themeLabel: { fontSize: '0.85rem', color: 'var(--text-muted)', minWidth: '50px' },
  themeOpt: {
    padding: '0.3rem 0.6rem',
    fontSize: '0.85rem',
    backgroundColor: 'transparent',
    border: '1px solid var(--border)',
    borderRadius: '4px',
    color: 'var(--text)',
    cursor: 'pointer',
  },
  themeOptActive: { backgroundColor: 'var(--accent-dim)', borderColor: 'var(--accent)' },
  button: {
    padding: '0.5rem 1rem',
    backgroundColor: 'var(--accent)',
    border: 'none',
    borderRadius: '4px',
    color: 'white',
    cursor: 'pointer',
  },
};
