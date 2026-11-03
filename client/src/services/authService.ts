import { apiClient, tokenStorage } from './apiClient';
import type { AuthResponse, LoginRequest, RegisterRequest, UserProfile } from '../types';

export const authService = {
  // Đăng nhập
  async login(data: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await apiClient.post<AuthResponse>('/auth/login', data);
      tokenStorage.setAuth(response.data);
      return response.data;
    } catch (error) {
      // Demo mock fallback if Backend is offline
      if ((error as { code?: string })?.code === 'ERR_NETWORK' || !window.navigator.onLine) {
        console.warn('[Demo Fallback] Backend chưa khởi chạy, kích hoạt chế độ demo!');
        const mockAuth: AuthResponse = {
          token: 'demo-jwt-token',
          refreshToken: 'demo-refresh-token',
          expiresAt: new Date(Date.now() + 86400000).toISOString(),
          user: {
            userId: 'demo-user-1',
            fullName: data.username.startsWith('09') ? 'Nguyễn Minh Tân' : data.username,
            phoneNumber: data.username,
            role: 'Tenant',
          },
        };
        tokenStorage.setAuth(mockAuth);
        return mockAuth;
      }
      throw error;
    }
  },

  // Đăng ký
  async register(data: RegisterRequest): Promise<AuthResponse> {
    try {
      const response = await apiClient.post<AuthResponse>('/auth/register', data);
      tokenStorage.setAuth(response.data);
      return response.data;
    } catch (error) {
      if ((error as { code?: string })?.code === 'ERR_NETWORK') {
        const mockAuth: AuthResponse = {
          token: 'demo-jwt-token',
          refreshToken: 'demo-refresh-token',
          expiresAt: new Date(Date.now() + 86400000).toISOString(),
          user: {
            userId: 'demo-user-new',
            fullName: data.fullName,
            phoneNumber: data.phoneNumber,
            role: data.role,
          },
        };
        tokenStorage.setAuth(mockAuth);
        return mockAuth;
      }
      throw error;
    }
  },

  // Đăng xuất
  async logout(): Promise<void> {
    const refreshToken = tokenStorage.getRefreshToken();
    try {
      if (refreshToken) {
        await apiClient.post('/auth/logout', { refreshToken });
      }
    } finally {
      tokenStorage.clearAuth();
    }
  },

  // Lấy thông tin tài khoản
  async getProfile(): Promise<UserProfile> {
    const response = await apiClient.get<UserProfile>('/users/profile');
    return response.data;
  },
};
