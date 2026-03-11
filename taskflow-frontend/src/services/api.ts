import axios from 'axios';

/**
 * Axios instance configured for the TaskFlow API.
 * baseURL points to the backend - Vite proxy forwards /api to localhost:5000 when in dev.
 */
// In dev, use relative URL so Vite proxy forwards to backend
export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Interceptor: Attach JWT token from localStorage to every request.
 * Token is stored after login/register.
 */
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

/**
 * Interceptor: On 401 Unauthorized, clear token and redirect to login.
 * Skip for auth endpoints (login/register) - they return 401 for invalid credentials.
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const url = error.config?.url ?? '';
      const isAuthEndpoint = url.includes('/auth/login') || url.includes('/auth/register');
      const hasToken = !!localStorage.getItem('token');
      if (!isAuthEndpoint && hasToken) {
        localStorage.removeItem('token');
        localStorage.removeItem('userId');
        localStorage.removeItem('userName');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);
