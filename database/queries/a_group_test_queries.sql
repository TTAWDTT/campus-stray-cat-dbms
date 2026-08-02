SET DEFINE OFF;

-- A组成员1测试查询：与 Auth/Users 接口场景对应，可重复执行核对。
-- 建议先执行 a_group_demo_data.sql 与 a_group_advanced.sql。

-- 1) 登录关联：按用户名取用户+角色（对应 POST /api/auth/login）
SELECT USERID, USERNAME, STATUS, ROLEID, ROLENAME, PERMISSIONSCOPE
FROM VW_USER_ROLE_PROFILE
WHERE UPPER(USERNAME) = UPPER('a_group_admin');

-- 2) 停用账号应拒绝登录（对应停用用户登录失败）
SELECT USERID, USERNAME, STATUS
FROM VW_USER_ROLE_PROFILE
WHERE UPPER(USERNAME) = UPPER('a_group_disabled');

-- 3) 用户列表：按用户名模糊筛选（对应 GET /api/users?username=）
SELECT USERID, USERNAME, STATUS, ROLENAME
FROM VW_USER_ROLE_PROFILE
WHERE UPPER(USERNAME) LIKE '%' || UPPER('a_group') || '%'
ORDER BY USERNAME;

-- 4) 用户列表：按状态筛选（对应 GET /api/users?status=ACTIVE）
SELECT USERID, USERNAME, STATUS, ROLENAME
FROM VW_USER_ROLE_PROFILE
WHERE STATUS = 'ACTIVE'
ORDER BY USERNAME;

-- 5) 用户列表：按角色筛选（对应 GET /api/users?roleId=）
SELECT USERID, USERNAME, ROLEID, ROLENAME
FROM VW_USER_ROLE_PROFILE
WHERE ROLEID = 'role-user-a-group'
ORDER BY USERNAME;

-- 6) 用户名唯一性检查（对应 POST /api/users 冲突 409）
SELECT COUNT(1) AS USERNAME_COUNT
FROM SYS_USERS
WHERE UPPER(USERNAME) = UPPER('a_group_admin');

-- 7) RoleID 外键存在性（对应新增用户 RoleID 校验）
SELECT ROLEID, ROLENAME
FROM SYS_ROLES
WHERE ROLEID = 'role-volunteer-a-group';

-- 8) 启停状态更新演示（对应 PATCH /api/users/{id}/status）
-- 执行后请再查回状态；勿在共享库长期保留测试中间态。
UPDATE SYS_USERS
SET STATUS = 'DISABLED'
WHERE USERID = 'user-normal-a-group'
  AND STATUS = 'ACTIVE';

SELECT USERID, USERNAME, STATUS
FROM VW_USER_ROLE_PROFILE
WHERE USERID = 'user-normal-a-group';

UPDATE SYS_USERS
SET STATUS = 'ACTIVE'
WHERE USERID = 'user-normal-a-group'
  AND STATUS = 'DISABLED';

SELECT USERID, USERNAME, STATUS
FROM VW_USER_ROLE_PROFILE
WHERE USERID = 'user-normal-a-group';

-- 9) 活跃用户视图抽查
SELECT USERID, USERNAME, ROLENAME
FROM VW_ACTIVE_USER_SUMMARIES
ORDER BY USERNAME;
