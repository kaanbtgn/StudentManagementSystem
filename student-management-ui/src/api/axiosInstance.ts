import axios from 'axios';
import { getCurrentSessionId } from '@/store/sessionStore';

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

axiosInstance.interceptors.request.use((config) => {
  const sessionId = getCurrentSessionId();
  if (sessionId) {
    config.headers['X-Session-Id'] = sessionId;
  }
  return config;
});

axiosInstance.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      window.location.href = '/login';
      return Promise.reject(error);
    }
    if (error.response?.status >= 500) {
      const message = error.response?.data?.message ?? 'Sunucu hatası oluştu.';
      console.error('[API Error]', message);
    }
    return Promise.reject(error);
  }
);

export default axiosInstance;
