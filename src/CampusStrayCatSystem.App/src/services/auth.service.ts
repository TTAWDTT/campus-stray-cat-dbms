import { http } from './http';
import type { AuthUser, LoginRequest, LoginResponse } from '../types/auth';

type ApiUser = Record<string, unknown>;

const value = <T>(data: ApiUser, camel: string, pascal: string): T | undefined =>
  (data[camel] ?? data[pascal]) as T | undefined;

const toUser = (data: ApiUser): AuthUser => ({
  userId: value<string>(data, 'userId', 'UserID') || '',
  username: value<string>(data, 'username', 'Username') || '',
  realName: value<string | null>(data, 'realName', 'RealName'),
  roleId: value<string>(data, 'roleId', 'RoleID') || '',
  roleName: value<string | null>(data, 'roleName', 'RoleName'),
  permissionScope: value<string | null>(data, 'permissionScope', 'PermissionScope'),
  studentNo: value<string | null>(data, 'studentNo', 'StudentNo'),
  phone: value<string | null>(data, 'phone', 'Phone'),
  verifyStatus: value<string | null>(data, 'verifyStatus', 'VerifyStatus'),
  status: value<string | null>(data, 'status', 'Status'),
});

export const authService = {
  async login(payload: LoginRequest) {
    const { data } = await http.post<Record<string, unknown>>('/auth/login', payload);
    return {
      ...toUser(data),
      token: value<string>(data, 'token', 'Token') || '',
      expiresAtUtc: value<string>(data, 'expiresAtUtc', 'ExpiresAtUtc') || '',
    } as LoginResponse;
  },

  async me() {
    const { data } = await http.get<ApiUser>('/auth/me');
    return toUser(data);
  },

  async logout() {
    await http.post('/auth/logout');
  },
};
