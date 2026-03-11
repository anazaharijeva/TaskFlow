import { createContext, useContext, useEffect, useLayoutEffect, useState } from 'react';

export type ThemeMode = 'dark' | 'light';
export type AccentColor = 'coral' | 'blue' | 'green' | 'purple';

interface ThemeContextType {
  mode: ThemeMode;
  accent: AccentColor;
  setMode: (m: ThemeMode) => void;
  setAccent: (a: AccentColor) => void;
}

const ThemeContext = createContext<ThemeContextType | null>(null);

const STORAGE_KEY = 'taskflow-theme';

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [mode, setModeState] = useState<ThemeMode>(() => {
    const s = localStorage.getItem(STORAGE_KEY);
    if (s) {
      try {
        const { mode: m } = JSON.parse(s);
        if (m === 'light' || m === 'dark') return m;
      } catch {}
    }
    return window.matchMedia?.('(prefers-color-scheme: light)').matches ? 'light' : 'dark';
  });
  const [accent, setAccentState] = useState<AccentColor>(() => {
    const s = localStorage.getItem(STORAGE_KEY);
    if (s) {
      try {
        const { accent: a } = JSON.parse(s);
        if (['coral', 'blue', 'green', 'purple'].includes(a)) return a;
      } catch {}
    }
    return 'coral';
  });

  useLayoutEffect(() => {
    document.documentElement.setAttribute('data-theme', mode);
    document.documentElement.setAttribute('data-accent', accent);
  }, [mode, accent]);
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify({ mode, accent }));
  }, [mode, accent]);

  const setMode = (m: ThemeMode) => setModeState(m);
  const setAccent = (a: AccentColor) => setAccentState(a);

  return (
    <ThemeContext.Provider value={{ mode, accent, setMode, setAccent }}>
      {children}
    </ThemeContext.Provider>
  );
}

export function useTheme() {
  const ctx = useContext(ThemeContext);
  if (!ctx) throw new Error('useTheme must be used within ThemeProvider');
  return ctx;
}
