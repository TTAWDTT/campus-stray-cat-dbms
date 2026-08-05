import axios from 'axios';
import { create } from 'zustand';
import { authService } from '../services/auth.service';
import { TOKEN_KEY } from '../services/http';
import type { AuthUser, LoginRequest } from '../types/auth';

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  ready: boolean;
  loading: boolean;
  error: string | null;
  restore: () => Promise<void>;
  login: (payload: LoginRequest) => Promise<void>;
  logout: () => Promise<void>;
}

const readMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const message = error.response?.data?.message;
    if (typeof message === 'string') return message;
    if (error.response?.status === 403) return '当前账号已停用，无法登录。';
    if (error.response?.status === 401) return '用户名或密码错误。';
  }
  return '暂时无法连接校园猫岛服务，请稍后再试。';
};

const clearSession = () => {
  localStorage.removeItem(TOKEN_KEY);
  return { token: null, user: null };
};

export const useAuthStore = create<AuthState>((set, get) => ({
  token: localStorage.getItem(TOKEN_KEY),
  user: null,
  ready: false,
  loading: false,
  error: null,

  restore: async () => {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) {
      set({ ready: true });
      return;
    }

    try {
      const user = await authService.me();
      set({ token, user, ready: true, error: null });
    } catch {
      set({ ...clearSession(), ready: true });
    }
  },

  login: async (payload) => {
    set({ loading: true, error: null });
    try {
      const response = await authService.login(payload);
      localStorage.setItem(TOKEN_KEY, response.token);
      set({ token: response.token, user: response, ready: true, loading: false, error: null });
    } catch (error) {
      set({ loading: false, error: readMessage(error) });
      throw error;
    }
  },

  logout: async () => {
    try {
      if (get().token) await authService.logout();
    } finally {
      set({ ...clearSession(), ready: true, loading: false, error: null });
    }
  },
}));
