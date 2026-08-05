export interface AuthUser {
  userId: string;
  username: string;
  realName?: string | null;
  roleId: string;
  roleName?: string | null;
  permissionScope?: string | null;
  studentNo?: string | null;
  phone?: string | null;
  verifyStatus?: string | null;
  status?: string | null;
}

export interface LoginResponse extends AuthUser {
  token: string;
  expiresAtUtc: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}
