import { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { api } from '../services/api';

interface AuthContextType {
  isAuthenticated: boolean;
  userId: string | null;
  userName: string | null;
  login: (token: string, id: string, name?: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(() => !!localStorage.getItem('token'));
  const [userId, setUserId] = useState<string | null>(() => localStorage.getItem('userId'));
  const [userName, setUserName] = useState<string | null>(() => localStorage.getItem('userName'));

  useEffect(() => {
    const token = localStorage.getItem('token');
    const id = localStorage.getItem('userId');
    const name = localStorage.getItem('userName');
    setIsAuthenticated(!!token);
    setUserId(id);
    setUserName(name);

    if (token) {
      api.get<{ userId: string; userName: string }>('auth/me')
        .then(({ data }) => {
          const n = data?.userName ?? (data as { UserName?: string })?.UserName;
          if (n) {
            localStorage.setItem('userName', n);
            setUserName(n);
          }
        })
        .catch(() => {});
    }
  }, []);

  const login = useCallback((token: string, id: string, name?: string) => {
    localStorage.setItem('token', token);
    localStorage.setItem('userId', id);
    if (name) localStorage.setItem('userName', name);
    setIsAuthenticated(true);
    setUserId(id);
    setUserName(name ?? null);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    localStorage.removeItem('userName');
    setIsAuthenticated(false);
    setUserId(null);
    setUserName(null);
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, userId, userName, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
