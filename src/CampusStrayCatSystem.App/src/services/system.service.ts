import { http } from './http';
import type { BlacklistRecord, BlacklistWritePayload, PagedResult, SystemRole, SystemUser, UserStatus, UserWritePayload, VerifyStatus } from '../features/system/system.types';

type ApiRecord = Record<string, unknown>;

const value = <T>(data: ApiRecord, camel: string, pascal: string): T | null =>
  (data[camel] ?? data[pascal] ?? null) as T | null;

const text = (data: ApiRecord, camel: string, pascal: string) => value<string>(data, camel, pascal) || '';

const toUser = (data: ApiRecord): SystemUser => ({
  userID: text(data, 'userID', 'UserID'),
  roleID: text(data, 'roleID', 'RoleID'),
  username: text(data, 'username', 'Username'),
  realName: value<string>(data, 'realName', 'RealName'),
  studentNo: value<string>(data, 'studentNo', 'StudentNo'),
  phone: value<string>(data, 'phone', 'Phone'),
  verifyStatus: value<VerifyStatus>(data, 'verifyStatus', 'VerifyStatus'),
  status: value<UserStatus>(data, 'status', 'Status'),
  roleName: value<string>(data, 'roleName', 'RoleName'),
  permissionScope: value<string>(data, 'permissionScope', 'PermissionScope'),
});

const toRole = (data: ApiRecord): SystemRole => ({
  roleID: text(data, 'roleID', 'RoleID'),
  roleName: text(data, 'roleName', 'RoleName'),
  description: text(data, 'description', 'Description'),
  permissionScope: text(data, 'permissionScope', 'PermissionScope'),
});

const toBlacklist = (data: ApiRecord): BlacklistRecord => ({
  blacklistId: text(data, 'blacklistId', 'BlacklistId'),
  userId: text(data, 'userId', 'UserId'),
  userName: value<string>(data, 'userName', 'UserName'),
  reasonType: text(data, 'reasonType', 'ReasonType'),
  reasonDetail: text(data, 'reasonDetail', 'ReasonDetail'),
  applicationId: value<string>(data, 'applicationId', 'ApplicationId'),
  createdBy: value<string>(data, 'createdBy', 'CreatedBy'),
  createdByName: value<string>(data, 'createdByName', 'CreatedByName'),
  createdAt: value<string>(data, 'createdAt', 'CreatedAt'),
  status: text(data, 'status', 'Status'),
  releaseTime: value<string>(data, 'releaseTime', 'ReleaseTime'),
  releasedBy: value<string>(data, 'releasedBy', 'ReleasedBy'),
  releasedByName: value<string>(data, 'releasedByName', 'ReleasedByName'),
});

export const systemService = {
  async users(filters: { username?: string; status?: string; roleId?: string } = {}) {
    const { data } = await http.get<ApiRecord[]>('/users', { params: filters });
    return data.map(toUser);
  },

  async createUser(payload: UserWritePayload) {
    const { data } = await http.post<ApiRecord>('/users', payload);
    return toUser(data);
  },

  async updateUser(userID: string, payload: UserWritePayload) {
    await http.put(`/users/${encodeURIComponent(userID)}`, payload);
  },

  async updateUserStatus(userID: string, status: UserStatus) {
    await http.patch(`/users/${encodeURIComponent(userID)}/status`, { status });
  },

  async roles() {
    const { data } = await http.get<ApiRecord[]>('/Roles');
    return data.map(toRole);
  },

  async createRole(payload: SystemRole) {
    const { data } = await http.post<ApiRecord>('/Roles', payload);
    return toRole(data);
  },

  async updateRole(roleID: string, payload: SystemRole) {
    await http.put(`/Roles/${encodeURIComponent(roleID)}`, payload);
  },

  async deleteRole(roleID: string) {
    await http.delete(`/Roles/${encodeURIComponent(roleID)}`);
  },

  async assignRole(userId: string, roleId: string) {
    await http.post('/Roles/assign', { userId, roleId });
  },

  async blacklist(filters: { userId?: string; status?: string; keyword?: string; page?: number; pageSize?: number } = {}) {
    const { data } = await http.get<ApiRecord>('/blacklist', { params: filters });
    const items = value<ApiRecord[]>(data, 'items', 'Items') || [];
    return {
      items: items.map(toBlacklist),
      totalCount: value<number>(data, 'totalCount', 'TotalCount') || 0,
      page: value<number>(data, 'page', 'Page') || 1,
      pageSize: value<number>(data, 'pageSize', 'PageSize') || 20,
      totalPages: value<number>(data, 'totalPages', 'TotalPages') || 1,
    } as PagedResult<BlacklistRecord>;
  },

  async addBlacklist(payload: BlacklistWritePayload) {
    await http.post('/blacklist', payload);
  },

  async releaseBlacklist(blacklistId: string) {
    await http.patch(`/blacklist/${encodeURIComponent(blacklistId)}/release`, {});
  },
};
