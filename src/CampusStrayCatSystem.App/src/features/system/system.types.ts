export type UserStatus = 'ACTIVE' | 'DISABLED';
export type VerifyStatus = 'VERIFIED' | 'UNVERIFIED';

export interface SystemUser {
  userID: string;
  roleID: string;
  username: string;
  realName: string | null;
  studentNo: string | null;
  phone: string | null;
  verifyStatus: VerifyStatus | null;
  status: UserStatus | null;
  roleName: string | null;
  permissionScope: string | null;
}

export interface SystemRole {
  roleID: string;
  roleName: string;
  description: string;
  permissionScope: string;
}

export interface BlacklistRecord {
  blacklistId: string;
  userId: string;
  userName: string | null;
  reasonType: string;
  reasonDetail: string;
  applicationId: string | null;
  createdBy: string | null;
  createdByName: string | null;
  createdAt: string | null;
  status: 'ACTIVE' | 'RELEASED' | string;
  releaseTime: string | null;
  releasedBy: string | null;
  releasedByName: string | null;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface UserWritePayload {
  roleID: string;
  username?: string;
  password?: string;
  realName?: string;
  studentNo?: string;
  phone?: string;
  verifyStatus?: VerifyStatus;
  status?: UserStatus;
}

export interface BlacklistWritePayload {
  userId: string;
  reasonType: string;
  reasonDetail: string;
  applicationId?: string;
}
