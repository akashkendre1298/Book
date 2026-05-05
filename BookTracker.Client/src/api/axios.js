import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5128/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Required for HttpOnly cookies
});

// Request interceptor removed: Tokens are now handled automatically via Cookies

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !window.location.pathname.includes('/login')) {
      localStorage.removeItem('user'); // User profile might still be in localStorage for UI, but token is in cookie
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export const getAssetUrl = (path) => {
  if (!path) return null;
  if (path.startsWith('http')) return path;
  const baseUrl = (import.meta.env.VITE_API_URL || 'http://localhost:5128/api/v1').split('/api/v1')[0];
  return `${baseUrl}${path}`;
};

export default api;
