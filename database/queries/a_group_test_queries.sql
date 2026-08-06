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

-- =====================================================
-- 以下为成员2测试
-- =====================================================

-- 10) 查看角色
SELECT ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE
FROM SYS_ROLES
WHERE ROLEID LIKE 'role-%-a-group'
ORDER BY ROLENAME;

-- 11) 查看用户
SELECT USERID, USERNAME, REALNAME, ROLEID, STATUS
FROM SYS_USERS
WHERE USERID LIKE 'user-%-a-group'
ORDER BY USERNAME;

-- 12) 用户+角色联合查询
SELECT u.USERNAME, u.REALNAME, r.ROLENAME, r.PERMISSIONSCOPE, u.STATUS
FROM SYS_USERS u
JOIN SYS_ROLES r ON u.ROLEID = r.ROLEID
WHERE u.USERID LIKE 'user-%-a-group'
ORDER BY u.USERNAME;

-- 13) 测试分配角色

SELECT USERID, USERNAME, ROLEID FROM SYS_USERS WHERE USERID = 'user-normal-a-group';

DECLARE
    v_Result VARCHAR2(500);
BEGIN
    SP_ASSIGN_USER_ROLE(
        p_UserID => 'user-normal-a-group',
        p_NewRoleID => 'role-volunteer-a-group',
        p_OperatorID => 'user-admin-a-group',
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_ASSIGN_USER_ROLE 执行结果: ' || v_Result);
END;
/

SELECT USERID, USERNAME, ROLEID FROM SYS_USERS WHERE USERID = 'user-normal-a-group';

-- 14) 测试加入黑名单

-- 加入前查看
SELECT BLACKLISTID, USERID, REASONTYPE, BLACKLISTSTATUS 
FROM USER_BLACKLIST WHERE USERID = 'user-volunteer-a-group';

DECLARE
    v_Result VARCHAR2(500);
BEGIN
    SP_ADD_USER_BLACKLIST(
        p_UserID => 'user-volunteer-a-group',
        p_ReasonType => '测试拉黑',
        p_ReasonDetail => '测试存储过程功能-新加入黑名单',
        p_ApplicationID => NULL,
        p_CreatedBy => 'user-admin-a-group',
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_ADD_USER_BLACKLIST 执行结果: ' || v_Result);
END;
/

-- 加入后验证
SELECT BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, BLACKLISTSTATUS, CREATETIME
FROM USER_BLACKLIST
WHERE USERID = 'user-volunteer-a-group'
ORDER BY CREATETIME DESC;

-- 15) 测试重复拉黑（预期失败）

DECLARE
    v_Result VARCHAR2(500);
BEGIN
    SP_ADD_USER_BLACKLIST(
        p_UserID => 'user-normal-a-group',
        p_ReasonType => '重复拉黑测试',
        p_ReasonDetail => '这个应该失败，因为用户已在黑名单中',
        p_ApplicationID => NULL,
        p_CreatedBy => 'user-admin-a-group',
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('重复拉黑结果（预期失败）: ' || v_Result);
END;
/

-- 16) 查询有效黑名单视图

SELECT BLACKLISTID, USERID, USERNAME, REASONTYPE, BLACKLISTSTATUS
FROM VW_ACTIVE_BLACKLIST_USERS
ORDER BY CREATETIME DESC;

-- 17) 测试解除黑名单

DECLARE
    v_Result VARCHAR2(500);
    v_BlacklistID VARCHAR2(36);
BEGIN
    SELECT BLACKLISTID INTO v_BlacklistID 
    FROM USER_BLACKLIST 
    WHERE BLACKLISTSTATUS = 'ACTIVE' AND ROWNUM = 1;
    
    SP_RELEASE_USER_BLACKLIST(
        p_BlacklistID => v_BlacklistID,
        p_ReleasedBy => 'user-admin-a-group',
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_RELEASE_USER_BLACKLIST 执行结果: ' || v_Result);
    DBMS_OUTPUT.PUT_LINE('解除的黑名单ID: ' || v_BlacklistID);
END;
/

-- 验证解除
SELECT BLACKLISTID, USERID, BLACKLISTSTATUS, RELEASETIME
FROM USER_BLACKLIST
WHERE BLACKLISTSTATUS = 'RELEASED'
ORDER BY RELEASETIME DESC;

-- 18) 查询用户黑名单状态
SELECT 
    USERID,
    CASE 
        WHEN EXISTS (SELECT 1 FROM USER_BLACKLIST WHERE USERID = 'user-normal-a-group' AND BLACKLISTSTATUS = 'ACTIVE')
        THEN '在黑名单中'
        ELSE '不在黑名单中'
    END AS BLACKLIST_STATUS
FROM DUAL;

SELECT 
    USERID,
    CASE 
        WHEN EXISTS (SELECT 1 FROM USER_BLACKLIST WHERE USERID = 'user-admin-a-group' AND BLACKLISTSTATUS = 'ACTIVE')
        THEN '在黑名单中'
        ELSE '不在黑名单中'
    END AS BLACKLIST_STATUS
FROM DUAL;

-- 19) 综合验证

SELECT 
    u.USERNAME,
    u.REALNAME,
    r.ROLENAME,
    r.PERMISSIONSCOPE,
    u.STATUS AS USER_STATUS,
    CASE 
        WHEN b.BLACKLISTSTATUS = 'ACTIVE' THEN '黑名单有效'
        WHEN b.BLACKLISTSTATUS = 'RELEASED' THEN '黑名单已解除'
        ELSE '不在黑名单'
    END AS BLACKLIST_STATUS
FROM SYS_USERS u
LEFT JOIN SYS_ROLES r ON u.ROLEID = r.ROLEID
LEFT JOIN USER_BLACKLIST b ON u.USERID = b.USERID AND b.BLACKLISTSTATUS = 'ACTIVE'
WHERE u.USERID LIKE 'user-%-a-group'
ORDER BY u.USERNAME;
