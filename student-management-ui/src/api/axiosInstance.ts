import axios from 'axios';

function getOrCreateSessionId(): string {
  let sessionId = localStorage.getItem('sessionId');
  if (!sessionId) {
    sessionId = crypto.randomUUID();
    localStorage.setItem('sessionId', sessionId);
  }
  return sessionId;
}

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

axiosInstance.interceptors.request.use((config) => {
  const sessionId = getOrCreateSessionId();
  config.headers['X-Session-Id'] = sessionId;
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
