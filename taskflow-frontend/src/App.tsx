import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import { ThemeProvider } from './contexts/ThemeContext';
import { Navbar } from './components/Navbar';
import { LoginPage } from './pages/LoginPage';
import { Dashboard } from './pages/Dashboard';
import { ProjectPage } from './pages/ProjectPage';

/**
 * Protected route - redirects to login if not authenticated.
 */
function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return <>{children}</>;
}

/**
 * If already logged in, redirect to dashboard instead of showing login page.
 */
function LoginRedirect({ children }: { children: React.ReactNode }) {
  const { isAuthenticated } = useAuth();
  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }
  return <>{children}</>;
}

/**
 * App - Root component with routing.
 * /login - Login/Register
 * /dashboard - Dashboard (protected)
 * /projects/:id - Project detail (protected)
 */
function App() {
  return (
    <ThemeProvider>
    <AuthProvider>
    <BrowserRouter>
      <Navbar />
      <main style={styles.main}>
        <Routes>
          <Route path="/login" element={
            <LoginRedirect>
              <LoginPage />
            </LoginRedirect>
          } />
          <Route
            path="/"
            element={<Navigate to="/dashboard" replace />}
          />
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />
          <Route
            path="/projects/:id"
            element={
              <ProtectedRoute>
                <ProjectPage />
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Routes>
      </main>
    </BrowserRouter>
    </AuthProvider>
    </ThemeProvider>
  );
}

const styles: Record<string, React.CSSProperties> = {
  main: {
    minHeight: 'calc(100vh - 60px)',
    backgroundColor: 'var(--bg-main)',
  },
};

export default App;
