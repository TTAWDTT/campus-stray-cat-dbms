import axios from 'axios';
import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { Button, Card, Icon, Input, Modal, Select, Tag } from 'animal-island-ui';
import type { TableColumn } from 'animal-island-ui';
import { PageHeader } from '../../../shared/components/PageHeader';
import { StatusTag } from '../../../shared/components/StatusTag';
import { systemService } from '../../../services/system.service';
import type { BlacklistRecord, BlacklistWritePayload, SystemRole, SystemUser, UserStatus, UserWritePayload, VerifyStatus } from '../system.types';

type TabKey = 'users' | 'roles' | 'blacklist';
type ModalKind = 'user' | 'role' | 'blacklist' | null;

const roleNames: Record<string, string> = { ADMIN: '管理员', VOLUNTEER: '志愿者', USER: '普通用户', VET: '兽医' };
const userStatusLabels: Record<string, string> = { ACTIVE: '正常使用', DISABLED: '已停用' };
const verifyLabels: Record<string, string> = { VERIFIED: '已认证', UNVERIFIED: '未认证' };
const blacklistStatusLabels: Record<string, string> = { ACTIVE: '生效中', RELEASED: '已解除' };
const reasonLabels: Record<string, string> = { ABANDONMENT: '弃养行为', ANIMAL_ABUSE: '伤害动物', FALSE_INFORMATION: '虚假信息', OTHER: '其他原因' };
const tabs: { key: TabKey; label: string; note: string; icon: 'icon-design' | 'icon-chat' | 'icon-critterpedia' }[] = [
  { key: 'users', label: '用户管理', note: '账号、身份与启停状态', icon: 'icon-chat' },
  { key: 'roles', label: '角色权限', note: '角色与可访问范围', icon: 'icon-design' },
  { key: 'blacklist', label: '领养黑名单', note: '审核前风险信息', icon: 'icon-critterpedia' },
];

const blankUser = (): UserWritePayload => ({ roleID: '', username: '', password: '', realName: '', studentNo: '', phone: '', verifyStatus: 'UNVERIFIED', status: 'ACTIVE' });
const blankRole = (): SystemRole => ({ roleID: '', roleName: 'USER', description: '', permissionScope: '' });
const blankBlacklist = (): BlacklistWritePayload => ({ userId: '', reasonType: 'OTHER', reasonDetail: '', applicationId: '' });

const readError = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const response = error.response?.data;
    if (typeof response?.message === 'string') return response.message;
    if (typeof response === 'string') return response;
    if (error.response?.status === 401) return '登录状态已失效，请重新登录。';
    if (error.response?.status === 403) return '当前账号没有执行系统管理操作的权限。';
    if (error.response?.status === 409) return '数据已发生变化，请刷新后再试。';
  }
  return '系统管理服务暂时不可用，请稍后重试。';
};

const formatTime = (value: string | null) => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '—' : date.toLocaleString('zh-CN', { year: 'numeric', month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' });
};

function SeriousTable({
  columns,
  dataSource,
  rowKey,
  loading,
  emptyText,
}: {
  columns: TableColumn[];
  dataSource: Record<string, unknown>[];
  rowKey: string;
  loading?: boolean;
  emptyText: string;
}) {
  return <div className="system-native-table-wrap">
    <span className="mobile-table-scroll-hint" aria-hidden="true">左右滑动查看完整信息</span>
    <div className="system-native-table" role="table" aria-busy={loading}>
      <div className="system-native-row system-native-head" role="row">
        {columns.map((column, index) => <div key={`${String(column.title)}-${index}`} role="columnheader" style={{ width: column.width ? `${column.width}px` : undefined, textAlign: column.align || 'left' }}>{column.title}</div>)}
      </div>
      {loading ? <div className="system-native-empty">正在加载数据…</div> : dataSource.length === 0 ? <div className="system-native-empty">{emptyText}</div> : dataSource.map((row, rowIndex) => <div className="system-native-row" role="row" key={String(row[rowKey] ?? rowIndex)}>
        {columns.map((column, index) => {
          const value = column.dataIndex ? row[column.dataIndex] : undefined;
          const cell = column.render ? column.render(value, row, rowIndex) : value as ReactNode;
          return <div key={`${String(column.title)}-${index}`} role="cell" style={{ width: column.width ? `${column.width}px` : undefined, textAlign: column.align || 'left' }}>{cell ?? '—'}</div>;
        })}
      </div>)}
    </div>
  </div>;
}

export function SystemPage() {
  const [activeTab, setActiveTab] = useState<TabKey>('users');
  const [users, setUsers] = useState<SystemUser[]>([]);
  const [roles, setRoles] = useState<SystemRole[]>([]);
  const [blacklist, setBlacklist] = useState<BlacklistRecord[]>([]);
  const [blacklistTotal, setBlacklistTotal] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [modal, setModal] = useState<ModalKind>(null);
  const [saving, setSaving] = useState(false);
  const [editingUser, setEditingUser] = useState<SystemUser | null>(null);
  const [editingRole, setEditingRole] = useState<SystemRole | null>(null);
  const [userForm, setUserForm] = useState<UserWritePayload>(blankUser);
  const [roleForm, setRoleForm] = useState<SystemRole>(blankRole);
  const [blacklistForm, setBlacklistForm] = useState<BlacklistWritePayload>(blankBlacklist);
  const [userSearch, setUserSearch] = useState('');
  const [userStatus, setUserStatus] = useState('');
  const [userRole, setUserRole] = useState('');
  const [blacklistSearch, setBlacklistSearch] = useState('');
  const [blacklistStatus, setBlacklistStatus] = useState('ACTIVE');

  const loadData = async () => {
    setLoading(true);
    setError('');
    try {
      const [nextUsers, nextRoles, nextBlacklist] = await Promise.all([
        systemService.users(),
        systemService.roles(),
        systemService.blacklist({ status: blacklistStatus || undefined, keyword: blacklistSearch.trim() || undefined, pageSize: 100 }),
      ]);
      setUsers(nextUsers);
      setRoles(nextRoles);
      setBlacklist(nextBlacklist.items);
      setBlacklistTotal(nextBlacklist.totalCount);
    } catch (loadError) {
      setError(readError(loadError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void loadData(); }, []);

  const reloadBlacklist = async () => {
    try {
      const result = await systemService.blacklist({ status: blacklistStatus || undefined, keyword: blacklistSearch.trim() || undefined, pageSize: 100 });
      setBlacklist(result.items);
      setBlacklistTotal(result.totalCount);
    } catch (loadError) {
      setError(readError(loadError));
    }
  };

  useEffect(() => {
    if (!loading) void reloadBlacklist();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [blacklistStatus]);

  const filteredUsers = useMemo(() => {
    const keyword = userSearch.trim().toLowerCase();
    return users.filter((user) => (!keyword || [user.username, user.realName, user.studentNo, user.phone].some((item) => item?.toLowerCase().includes(keyword)))
      && (!userStatus || user.status === userStatus) && (!userRole || user.roleID === userRole));
  }, [users, userSearch, userStatus, userRole]);

  const openNewUser = () => { setEditingUser(null); setUserForm(blankUser()); setModal('user'); };
  const openEditUser = (user: SystemUser) => {
    setEditingUser(user);
    setUserForm({ roleID: user.roleID, realName: user.realName || '', studentNo: user.studentNo || '', phone: user.phone || '', verifyStatus: user.verifyStatus || 'UNVERIFIED', status: user.status || 'ACTIVE' });
    setModal('user');
  };
  const availableRoleNames = Object.keys(roleNames).filter((roleName) => !roles.some((role) => role.roleName === roleName));
  const openNewRole = () => {
    if (!availableRoleNames.length) {
      setError('四种系统角色均已存在。如需调整权限，请编辑现有角色。');
      return;
    }
    setEditingRole(null);
    setRoleForm({ ...blankRole(), roleName: availableRoleNames[0] });
    setModal('role');
  };
  const openEditRole = (role: SystemRole) => { setEditingRole(role); setRoleForm({ ...role }); setModal('role'); };
  const openBlacklist = () => { setBlacklistForm(blankBlacklist()); setModal('blacklist'); };
  const closeModal = () => { if (!saving) setModal(null); };

  const submitUser = async () => {
    if (!userForm.roleID || (!editingUser && (!userForm.username?.trim() || !userForm.password))) {
      setError('请填写用户名、初始密码并选择角色。');
      return;
    }
    setSaving(true);
    try {
      if (editingUser) {
        await systemService.updateUser(editingUser.userID, userForm);
        setNotice(`“${editingUser.username}”的资料已更新。`);
      } else {
        await systemService.createUser({ ...userForm, username: userForm.username?.trim(), password: userForm.password });
        setNotice(`已创建账号“${userForm.username?.trim()}”。`);
      }
      setModal(null);
      await loadData();
    } catch (saveError) {
      setError(readError(saveError));
    } finally { setSaving(false); }
  };

  const toggleStatus = async (user: SystemUser) => {
    const next: UserStatus = user.status === 'DISABLED' ? 'ACTIVE' : 'DISABLED';
    const action = next === 'ACTIVE' ? '启用' : '停用';
    if (!window.confirm(`确定${action}账号“${user.username}”吗？`)) return;
    try {
      await systemService.updateUserStatus(user.userID, next);
      setNotice(`账号“${user.username}”已${action}。`);
      await loadData();
    } catch (updateError) { setError(readError(updateError)); }
  };

  const submitRole = async () => {
    if (!roleForm.roleName.trim() || !roleForm.description.trim()) {
      setError('请填写角色编码和角色说明。');
      return;
    }
    setSaving(true);
    try {
      const payload = { roleName: roleForm.roleName.trim(), description: roleForm.description.trim(), permissionScope: roleForm.permissionScope.trim() };
      if (editingRole) {
        await systemService.updateRole(editingRole.roleID, { ...payload, roleID: editingRole.roleID });
        setNotice(`角色“${roleNames[payload.roleName] || payload.roleName}”已更新。`);
      } else {
        await systemService.createRole(payload);
        setNotice(`已创建角色“${roleNames[payload.roleName] || payload.roleName}”。`);
      }
      setModal(null);
      await loadData();
    } catch (saveError) { setError(readError(saveError)); }
    finally { setSaving(false); }
  };

  const deleteRole = async (role: SystemRole) => {
    if (!window.confirm(`确定删除角色“${roleNames[role.roleName] || role.roleName}”吗？仍被用户使用的角色无法删除。`)) return;
    try {
      await systemService.deleteRole(role.roleID);
      setNotice(`角色“${roleNames[role.roleName] || role.roleName}”已删除。`);
      await loadData();
    } catch (deleteError) { setError(readError(deleteError)); }
  };

  const submitBlacklist = async () => {
    if (!blacklistForm.userId || !blacklistForm.reasonDetail.trim()) {
      setError('请选择用户并填写具体原因。');
      return;
    }
    setSaving(true);
    try {
      await systemService.addBlacklist({ ...blacklistForm, reasonDetail: blacklistForm.reasonDetail.trim(), applicationId: blacklistForm.applicationId?.trim() || undefined });
      const user = users.find((item) => item.userID === blacklistForm.userId);
      setNotice(`已将“${user?.username || '该用户'}”加入黑名单。`);
      setModal(null);
      await loadData();
    } catch (saveError) { setError(readError(saveError)); }
    finally { setSaving(false); }
  };

  const releaseBlacklist = async (record: BlacklistRecord) => {
    if (!window.confirm(`确定解除“${record.userName || record.userId}”的黑名单状态吗？`)) return;
    try {
      await systemService.releaseBlacklist(record.blacklistId);
      setNotice(`已解除“${record.userName || record.userId}”的黑名单。`);
      await loadData();
    } catch (releaseError) { setError(readError(releaseError)); }
  };

  const rowUser = (row: Record<string, unknown>) => row as unknown as SystemUser;
  const rowRole = (row: Record<string, unknown>) => row as unknown as SystemRole;
  const rowBlacklist = (row: Record<string, unknown>) => row as unknown as BlacklistRecord;
  const userColumns: TableColumn[] = [
    { title: '用户', width: 160, render: (_value, raw) => { const user = rowUser(raw); return <span className="system-user-cell"><strong>{user.username}</strong><small>{user.realName || '未填写姓名'}</small></span>; } },
    { title: '角色', width: 116, render: (_value, raw) => { const user = rowUser(raw); return <Tag color="app-teal" variant="soft" size="small">{roleNames[user.roleName || ''] || user.roleName || '未分配'}</Tag>; } },
    { title: '认证', width: 94, render: (_value, raw) => { const user = rowUser(raw); return <StatusTag value={user.verifyStatus || 'UNVERIFIED'} label={verifyLabels[user.verifyStatus || 'UNVERIFIED']} />; } },
    { title: '账号状态', width: 106, render: (_value, raw) => { const user = rowUser(raw); return <StatusTag value={user.status || 'ACTIVE'} label={userStatusLabels[user.status || 'ACTIVE']} />; } },
    { title: '学号 / 工号', width: 138, render: (_value, raw) => rowUser(raw).studentNo || '—' },
    { title: '操作', width: 150, align: 'right', render: (_value, raw) => { const user = rowUser(raw); return <div className="system-table-actions"><Button type="text" size="small" onClick={() => openEditUser(user)}>编辑</Button><Button type="text" size="small" onClick={() => void toggleStatus(user)}>{user.status === 'DISABLED' ? '启用' : '停用'}</Button></div>; } },
  ];
  const roleColumns: TableColumn[] = [
    { title: '角色', width: 140, render: (_value, raw) => { const role = rowRole(raw); return <span className="system-role-name"><strong>{roleNames[role.roleName] || role.roleName}</strong><small>{role.roleName}</small></span>; } },
    { title: '说明', width: 180, dataIndex: 'description', render: (value) => typeof value === 'string' && value || '—' },
    { title: '权限范围', width: 340, render: (_value, raw) => { const role = rowRole(raw); const scopes = role.permissionScope.split(',').map((item) => item.trim()).filter(Boolean); return <div className="system-permission-tags">{scopes.length ? scopes.map((scope) => <Tag key={scope} color="app-yellow" variant="soft" size="small">{scope}</Tag>) : <span>未配置</span>}</div>; } },
    { title: '操作', width: 130, align: 'right', render: (_value, raw) => { const role = rowRole(raw); return <div className="system-table-actions"><Button type="text" size="small" onClick={() => openEditRole(role)}>编辑</Button><Button type="text" size="small" onClick={() => void deleteRole(role)}>删除</Button></div>; } },
  ];
  const blacklistColumns: TableColumn[] = [
    { title: '用户', width: 140, render: (_value, raw) => { const record = rowBlacklist(raw); return <span className="system-user-cell"><strong>{record.userName || record.userId}</strong><small>{record.userId}</small></span>; } },
    { title: '原因', width: 132, render: (_value, raw) => { const record = rowBlacklist(raw); return <span className="system-reason-cell"><strong>{reasonLabels[record.reasonType] || record.reasonType}</strong><small>{record.reasonDetail}</small></span>; } },
    { title: '加入时间', width: 138, render: (_value, raw) => formatTime(rowBlacklist(raw).createdAt) },
    { title: '状态', width: 96, render: (_value, raw) => { const record = rowBlacklist(raw); return <StatusTag value={record.status === 'ACTIVE' ? 'DISABLED' : 'ACTIVE'} label={blacklistStatusLabels[record.status] || record.status} />; } },
    { title: '解除信息', width: 168, render: (_value, raw) => { const record = rowBlacklist(raw); return record.status === 'RELEASED' ? <span className="system-release-info">{formatTime(record.releaseTime)}<small>{record.releasedByName || record.releasedBy || ''}</small></span> : '—'; } },
    { title: '操作', width: 100, align: 'right', render: (_value, raw) => { const record = rowBlacklist(raw); return record.status === 'ACTIVE' ? <Button type="text" size="small" onClick={() => void releaseBlacklist(record)}>解除</Button> : <span className="system-empty-action">—</span>; } },
  ];

  const activeBlacklistUsers = users.filter((user) => !blacklist.some((record) => record.userId === user.userID && record.status === 'ACTIVE'));
  const overview = [
    { label: '可用账号', value: users.filter((user) => user.status === 'ACTIVE').length, note: `共 ${users.length} 个用户`, tone: 'app-green' as const },
    { label: '角色类型', value: roles.length, note: '可按角色控制菜单与操作', tone: 'app-teal' as const },
    { label: '生效黑名单', value: blacklist.filter((record) => record.status === 'ACTIVE').length, note: '领养审核时应优先核验', tone: 'app-yellow' as const },
  ];

  return <section className="feature-page system-page">
    <PageHeader kicker="SYSTEM MANAGEMENT" title="系统管理" icon="icon-design" />
    <div className="system-overview-row">
      {overview.map((item) => <Card key={item.label} color={item.tone} className="system-overview-card"><strong>{item.value}</strong><span><b>{item.label}</b><small>{item.note}</small></span></Card>)}
    </div>
    <Card className="system-workspace-card">
      <div className="system-tabs" role="tablist" aria-label="系统管理模块">
        {tabs.map((tab) => <button key={tab.key} type="button" role="tab" aria-selected={activeTab === tab.key} className={activeTab === tab.key ? 'active' : ''} onClick={() => setActiveTab(tab.key)}><Icon name={tab.icon} size={17} /><span><strong>{tab.label}</strong><small>{tab.note}</small></span></button>)}
      </div>
      {notice && <div className="system-notice" role="status"><Icon name="icon-diy" size={16} /><span>{notice}</span><Button type="text" size="small" onClick={() => setNotice('')}>知道了</Button></div>}
      {error && <div className="system-alert" role="alert"><span>{error}</span><Button type="text" size="small" onClick={() => setError('')}>关闭</Button></div>}
      {activeTab === 'users' && <div className="system-panel"><div className="system-panel-heading"><div><h2>用户管理</h2></div><Button type="primary" onClick={openNewUser}><Icon name="icon-diy" size={16} />新增用户</Button></div><div className="system-filter-grid"><Input value={userSearch} onChange={(event) => setUserSearch(event.target.value)} allowClear placeholder="搜索账号、姓名或学号" prefix={<Icon name="icon-chat" size={15} />} /><Select value={userRole} onChange={setUserRole} options={[{ key: '', label: '全部角色' }, ...roles.map((role) => ({ key: role.roleID, label: roleNames[role.roleName] || role.roleName }))]} /><Select value={userStatus} onChange={setUserStatus} options={[{ key: '', label: '全部账号状态' }, { key: 'ACTIVE', label: '正常使用' }, { key: 'DISABLED', label: '已停用' }]} /><Button type="default" onClick={() => { setUserSearch(''); setUserRole(''); setUserStatus(''); }}>重置</Button></div><SeriousTable columns={userColumns} dataSource={filteredUsers as unknown as Record<string, unknown>[]} rowKey="userID" loading={loading} emptyText="暂无用户数据" /></div>}
      {activeTab === 'roles' && <div className="system-panel"><div className="system-panel-heading"><div><h2>角色与权限</h2><p>角色编码使用系统约定的英文值，中文仅用于页面显示与说明。</p></div><Button type="primary" onClick={openNewRole} disabled={!availableRoleNames.length}><Icon name="icon-diy" size={16} />新增角色</Button></div><SeriousTable columns={roleColumns} dataSource={roles as unknown as Record<string, unknown>[]} rowKey="roleID" loading={loading} emptyText="暂无角色数据" /></div>}
      {activeTab === 'blacklist' && <div className="system-panel"><div className="system-panel-heading"><div><h2>领养黑名单</h2><p>记录不适合领养或存在违规行为的用户，供审核流程核验。</p></div><Button type="primary" onClick={openBlacklist}><Icon name="icon-diy" size={16} />加入黑名单</Button></div><div className="system-filter-grid system-blacklist-filters"><Input value={blacklistSearch} onChange={(event) => setBlacklistSearch(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') void reloadBlacklist(); }} allowClear placeholder="搜索用户、原因或关联申请" prefix={<Icon name="icon-critterpedia" size={15} />} /><Select value={blacklistStatus} onChange={setBlacklistStatus} options={[{ key: '', label: '全部状态' }, { key: 'ACTIVE', label: '生效中' }, { key: 'RELEASED', label: '已解除' }]} /><Button type="default" onClick={() => void reloadBlacklist()}>查询</Button></div><div className="system-table-meta">当前共 {blacklistTotal} 条记录</div><SeriousTable columns={blacklistColumns} dataSource={blacklist as unknown as Record<string, unknown>[]} rowKey="blacklistId" loading={loading} emptyText="暂无黑名单数据" /></div>}
    </Card>
    <Modal open={modal === 'user'} title={editingUser ? `编辑用户 · ${editingUser.username}` : '新增系统用户'} width={650} typewriter={false} onClose={closeModal} footer={<div className="system-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" loading={saving} onClick={() => void submitUser()}>{editingUser ? '保存修改' : '创建用户'}</Button></div>}><div className="system-form"><div className="system-form-intro"><Icon name="icon-chat" size={21} /><span><strong>{editingUser ? '维护账号资料与访问身份' : '为校园猫岛创建一位新成员'}</strong><small>账号状态和角色会在下次请求时由服务端再次校验。</small></span></div><div className="system-form-grid">{!editingUser && <label><span>用户名 *</span><Input value={userForm.username || ''} onChange={(event) => setUserForm((form) => ({ ...form, username: event.target.value }))} placeholder="例如：campus_volunteer" /></label>}<label><span>角色 *</span><Select value={userForm.roleID} onChange={(value) => setUserForm((form) => ({ ...form, roleID: value }))} options={[{ key: '', label: '请选择角色' }, ...roles.map((role) => ({ key: role.roleID, label: roleNames[role.roleName] || role.roleName }))]} /></label>{!editingUser && <label><span>初始密码 *</span><Input type="password" value={userForm.password || ''} onChange={(event) => setUserForm((form) => ({ ...form, password: event.target.value }))} placeholder="至少 6 位" /></label>}<label><span>姓名</span><Input value={userForm.realName || ''} onChange={(event) => setUserForm((form) => ({ ...form, realName: event.target.value }))} placeholder="真实姓名或称呼" /></label><label><span>学号 / 工号</span><Input value={userForm.studentNo || ''} onChange={(event) => setUserForm((form) => ({ ...form, studentNo: event.target.value }))} placeholder="选填" /></label><label><span>联系电话</span><Input value={userForm.phone || ''} onChange={(event) => setUserForm((form) => ({ ...form, phone: event.target.value }))} placeholder="选填" /></label><label><span>认证状态</span><Select value={userForm.verifyStatus || 'UNVERIFIED'} onChange={(value) => setUserForm((form) => ({ ...form, verifyStatus: value as VerifyStatus }))} options={[{ key: 'UNVERIFIED', label: '未认证' }, { key: 'VERIFIED', label: '已认证' }]} /></label><label><span>账号状态</span><Select value={userForm.status || 'ACTIVE'} onChange={(value) => setUserForm((form) => ({ ...form, status: value as UserStatus }))} options={[{ key: 'ACTIVE', label: '正常使用' }, { key: 'DISABLED', label: '已停用' }]} /></label></div></div></Modal>
    <Modal open={modal === 'role'} title={editingRole ? `编辑角色 · ${roleNames[editingRole.roleName] || editingRole.roleName}` : '新增角色'} width={620} typewriter={false} onClose={closeModal} footer={<div className="system-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" loading={saving} onClick={() => void submitRole()}>{editingRole ? '保存角色' : '创建角色'}</Button></div>}><div className="system-form"><div className="system-form-intro"><Icon name="icon-design" size={21} /><span><strong>角色决定系统的访问能力</strong><small>{editingRole ? '角色 ID 仅用于识别，不能修改。' : '角色 ID 由服务端自动生成。'} 权限用英文代码、半角逗号分隔，例如 USER_MANAGE,ROLE_MANAGE。</small></span></div><div className="system-form-grid">{editingRole && <label><span>角色 ID</span><Input value={roleForm.roleID} disabled /></label>}<label><span>角色编码 *</span><Select value={roleForm.roleName} onChange={(value) => setRoleForm((form) => ({ ...form, roleName: value }))} options={(editingRole ? Object.keys(roleNames) : availableRoleNames).map((key) => ({ key, label: `${roleNames[key]}（${key}）` }))} /></label><label className="system-form-wide"><span>角色说明 *</span><Input value={roleForm.description} onChange={(event) => setRoleForm((form) => ({ ...form, description: event.target.value }))} placeholder="例如：负责基础医疗和健康记录维护" /></label><label className="system-form-wide"><span>权限范围</span><textarea value={roleForm.permissionScope} onChange={(event) => setRoleForm((form) => ({ ...form, permissionScope: event.target.value }))} placeholder="例如：CAT_VIEW,MEDICAL_WRITE" /></label></div></div></Modal>
    <Modal open={modal === 'blacklist'} title="加入领养黑名单" width={620} typewriter={false} onClose={closeModal} footer={<div className="system-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" loading={saving} onClick={() => void submitBlacklist()}>确认加入</Button></div>}><div className="system-form"><div className="system-form-intro"><Icon name="icon-critterpedia" size={21} /><span><strong>保留对领养审核有价值的风险记录</strong><small>已在有效黑名单中的用户不会重复出现在候选项内。</small></span></div><div className="system-form-grid"><label className="system-form-wide"><span>用户 *</span><Select value={blacklistForm.userId} onChange={(value) => setBlacklistForm((form) => ({ ...form, userId: value }))} options={[{ key: '', label: '请选择需要处理的用户' }, ...activeBlacklistUsers.map((user) => ({ key: user.userID, label: `${user.username}${user.realName ? ` · ${user.realName}` : ''}` }))]} /></label><label><span>原因类型 *</span><Select value={blacklistForm.reasonType} onChange={(value) => setBlacklistForm((form) => ({ ...form, reasonType: value }))} options={Object.entries(reasonLabels).map(([key, label]) => ({ key, label }))} /></label><label><span>关联申请 ID</span><Input value={blacklistForm.applicationId || ''} onChange={(event) => setBlacklistForm((form) => ({ ...form, applicationId: event.target.value }))} placeholder="选填" /></label><label className="system-form-wide"><span>具体原因 *</span><textarea value={blacklistForm.reasonDetail} onChange={(event) => setBlacklistForm((form) => ({ ...form, reasonDetail: event.target.value }))} placeholder="请记录可供领养审核参考的事实与原因" /></label></div></div></Modal>
  </section>;
}
