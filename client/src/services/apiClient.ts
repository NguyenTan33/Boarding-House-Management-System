import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import type { ApiErrorResponse, AuthResponse } from '../types';

const BASE_URL = import.meta.env.VITE_API_URL || '/api';

export const apiClient = axios.create({
  baseURL: BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

// Storage keys
const TOKEN_KEY = 'greenhouse_token';
const REFRESH_TOKEN_KEY = 'greenhouse_refresh_token';
const USER_KEY = 'greenhouse_user';

export const tokenStorage = {
  getToken: () => localStorage.getItem(TOKEN_KEY),
  getRefreshToken: () => localStorage.getItem(REFRESH_TOKEN_KEY),
  getUser: () => {
    const raw = localStorage.getItem(USER_KEY);
    try {
      return raw ? JSON.parse(raw) : null;
    } catch {
      return null;
    }
  },
  setAuth: (data: AuthResponse) => {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(data.user));
  },
  clearAuth: () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },
};

// Request Interceptor: Tự động đính kèm Bearer Token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = tokenStorage.getToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response Interceptor: Xử lý 401 (Refresh Token) & 429 (Rate Limit)
let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value?: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

const processQueue = (error: unknown, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiErrorResponse>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };

    // Bắt lỗi 429: Rate Limit
    if (error.response?.status === 429) {
      const retryAfter = error.response.data?.retryAfterSeconds || 60;
      console.warn(`[API] Rate limit reached. Vui lòng thử lại sau ${retryAfter} giây.`);
      window.dispatchEvent(
        new CustomEvent('api:rate-limited', { detail: { retryAfterSeconds: retryAfter } })
      );
      return Promise.reject(error);
    }

    // Bắt lỗi 401: Unauthorized -> Cố gắng Refresh Token
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`;
            }
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const currentToken = tokenStorage.getToken();
      const currentRefreshToken = tokenStorage.getRefreshToken();

      if (!currentRefreshToken) {
        tokenStorage.clearAuth();
        window.dispatchEvent(new CustomEvent('api:unauthorized'));
        isRefreshing = false;
        return Promise.reject(error);
      }

      try {
        const response = await axios.post<AuthResponse>(`${BASE_URL}/auth/refresh`, {
          token: currentToken,
          refreshToken: currentRefreshToken,
        });

        const newAuth = response.data;
        tokenStorage.setAuth(newAuth);

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newAuth.token}`;
        }

        processQueue(null, newAuth.token);
        return apiClient(originalRequest);
      } catch (refreshErr) {
        processQueue(refreshErr, null);
        tokenStorage.clearAuth();
        window.dispatchEvent(new CustomEvent('api:unauthorized'));
        return Promise.reject(refreshErr);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);
