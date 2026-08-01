SELECT USERID, USERNAME, STATUS
FROM SYS_USERS
ORDER BY USERNAME;

SELECT *
FROM VW_USER_ROLE_PROFILE
WHERE UPPER(USERNAME) LIKE '%ADMIN%';

UPDATE SYS_USERS
SET STATUS = 'DISABLED'
WHERE USERID = :USER_ID;

UPDATE SYS_USERS
SET STATUS = 'ACTIVE'
WHERE USERID = :USER_ID;


SET SERVEROUTPUT ON;

-- =====================================================
-- 测试1: 查询所有角色
-- =====================================================
PROMPT === 测试1: 查询所有角色 ===
SELECT ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE, ISACTIVE, CREATEDAT
FROM SYS_ROLES
ORDER BY CREATEDAT DESC;

-- =====================================================
-- 测试2: 查询A组测试用户
-- =====================================================
PROMPT === 测试2: 查询A组测试用户 ===
SELECT USERID, USERNAME, REALNAME, ROLEID, STATUS
FROM SYS_USERS
WHERE USERID LIKE 'user-%-a-group'
   OR USERNAME LIKE 'a_group_%'
ORDER BY USERNAME;

-- =====================================================
-- 测试3: 查询用户及其角色信息（联合视图）
-- =====================================================
PROMPT === 测试3: 查询用户及其角色信息 ===
SELECT USERID, USERNAME, REALNAME, ROLENAME, PERMISSIONSCOPE, STATUS
FROM VW_USER_ROLE_PROFILE
WHERE USERID LIKE 'user-%-a-group'
ORDER BY USERNAME;

-- =====================================================
-- 测试4: 测试分配角色存储过程
-- =====================================================
PROMPT === 测试4: 测试 SP_ASSIGN_USER_ROLE ===
-- 先查看当前用户的角色
PROMPT --- 分配前: 查看 user-common-a-group 当前角色 ---
SELECT USERID, USERNAME, ROLEID FROM SYS_USERS WHERE USERID = 'user-common-a-group';

DECLARE
    v_Result VARCHAR2(500);
    v_UserID VARCHAR2(36) := 'user-common-a-group';
    v_NewRoleID VARCHAR2(36) := 'role-volunteer-a-group';
    v_OperatorID VARCHAR2(36) := 'user-admin-a-group';
BEGIN
    SP_ASSIGN_USER_ROLE(
        p_UserID => v_UserID,
        p_NewRoleID => v_NewRoleID,
        p_OperatorID => v_OperatorID,
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_ASSIGN_USER_ROLE 执行结果: ' || v_Result);
END;
/

-- 验证角色是否更新成功
PROMPT --- 分配后: 查看 user-common-a-group 角色 ---
SELECT USERID, USERNAME, ROLEID FROM SYS_USERS WHERE USERID = 'user-common-a-group';

-- =====================================================
-- 测试5: 测试加入黑名单存储过程
-- =====================================================
PROMPT === 测试5: 测试 SP_ADD_USER_BLACKLIST ===
-- 先查看当前黑名单记录
PROMPT --- 加入前: 查看当前黑名单 ---
SELECT BLACKLISTID, USERID, REASONTYPE, STATUS FROM USER_BLACKLIST WHERE BLACKLISTID LIKE 'bl-%-a-group';

DECLARE
    v_Result VARCHAR2(500);
    v_UserID VARCHAR2(36) := 'user-volunteer-a-group';
    v_ReasonType VARCHAR2(50) := '测试拉黑';
    v_ReasonDetail VARCHAR2(500) := '测试存储过程功能-新加入黑名单';
    v_ApplicationID VARCHAR2(36) := NULL;
    v_CreatedBy VARCHAR2(36) := 'user-admin-a-group';
BEGIN
    SP_ADD_USER_BLACKLIST(
        p_UserID => v_UserID,
        p_ReasonType => v_ReasonType,
        p_ReasonDetail => v_ReasonDetail,
        p_ApplicationID => v_ApplicationID,
        p_CreatedBy => v_CreatedBy,
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_ADD_USER_BLACKLIST 执行结果: ' || v_Result);
END;
/

-- 验证黑名单是否插入成功
PROMPT --- 加入后: 查看黑名单 ---
SELECT BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, STATUS, CREATEDAT
FROM USER_BLACKLIST
WHERE USERID = 'user-volunteer-a-group'
ORDER BY CREATEDAT DESC;

-- =====================================================
-- 测试6: 测试重复拉黑（应该报错）
-- =====================================================
PROMPT === 测试6: 测试重复拉黑（预期失败） ===
DECLARE
    v_Result VARCHAR2(500);
    v_UserID VARCHAR2(36) := 'user-common-a-group';
    v_ReasonType VARCHAR2(50) := '重复拉黑测试';
    v_ReasonDetail VARCHAR2(500) := '这个应该失败，因为用户已在黑名单中';
    v_ApplicationID VARCHAR2(36) := NULL;
    v_CreatedBy VARCHAR2(36) := 'user-admin-a-group';
BEGIN
    SP_ADD_USER_BLACKLIST(
        p_UserID => v_UserID,
        p_ReasonType => v_ReasonType,
        p_ReasonDetail => v_ReasonDetail,
        p_ApplicationID => v_ApplicationID,
        p_CreatedBy => v_CreatedBy,
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('重复拉黑结果（预期失败）: ' || v_Result);
END;
/

-- =====================================================
-- 测试7: 查询有效黑名单视图
-- =====================================================
PROMPT === 测试7: 查询 VW_ACTIVE_BLACKLIST_USERS ===
SELECT BLACKLISTID, USERID, USERNAME, REALNAME, REASONTYPE, REASONDETAIL, STATUS
FROM VW_ACTIVE_BLACKLIST_USERS
ORDER BY CREATEDAT DESC;

-- =====================================================
-- 测试8: 测试解除黑名单存储过程
-- =====================================================
PROMPT === 测试8: 测试 SP_RELEASE_USER_BLACKLIST ===
-- 先查询一个有效的黑名单记录ID
PROMPT --- 解除前: 查看有效黑名单 ---
SELECT BLACKLISTID, USERID, STATUS FROM USER_BLACKLIST WHERE STATUS = 'Active' AND ROWNUM = 1;

DECLARE
    v_Result VARCHAR2(500);
    v_BlacklistID VARCHAR2(36);
    v_ReleasedBy VARCHAR2(36) := 'user-admin-a-group';
BEGIN
    -- 获取第一条有效黑名单记录
    SELECT BLACKLISTID INTO v_BlacklistID 
    FROM USER_BLACKLIST 
    WHERE STATUS = 'Active' 
    AND ROWNUM = 1;
    
    SP_RELEASE_USER_BLACKLIST(
        p_BlacklistID => v_BlacklistID,
        p_ReleasedBy => v_ReleasedBy,
        p_Result => v_Result
    );
    DBMS_OUTPUT.PUT_LINE('SP_RELEASE_USER_BLACKLIST 执行结果: ' || v_Result);
    DBMS_OUTPUT.PUT_LINE('解除的黑名单ID: ' || v_BlacklistID);
END;
/

-- 验证解除是否成功
PROMPT --- 解除后: 查看已解除记录 ---
SELECT BLACKLISTID, USERID, STATUS, RELEASETIME, RELEASEDBY
FROM USER_BLACKLIST
WHERE STATUS = 'Released'
ORDER BY RELEASETIME DESC;

-- =====================================================
-- 测试9: 查询用户黑名单状态（供领养模块调用）
-- =====================================================
PROMPT === 测试9: 查询用户黑名单状态 ===
-- 查询在黑名单中的用户
PROMPT --- user-common-a-group 的黑名单状态 ---
SELECT 
    USERID,
    CASE 
        WHEN EXISTS (SELECT 1 FROM USER_BLACKLIST WHERE USERID = 'user-common-a-group' AND STATUS = 'Active')
        THEN '在黑名单中'
        ELSE '不在黑名单中'
    END AS BLACKLIST_STATUS,
    (SELECT REASONTYPE FROM USER_BLACKLIST WHERE USERID = 'user-common-a-group' AND STATUS = 'Active' AND ROWNUM = 1) AS REASONTYPE
FROM DUAL;

-- 查询不在黑名单中的用户
PROMPT --- user-admin-a-group 的黑名单状态 ---
SELECT 
    USERID,
    CASE 
        WHEN EXISTS (SELECT 1 FROM USER_BLACKLIST WHERE USERID = 'user-admin-a-group' AND STATUS = 'Active')
        THEN '在黑名单中'
        ELSE '不在黑名单中'
    END AS BLACKLIST_STATUS
FROM DUAL;

-- =====================================================
-- 测试10: 查询审计日志
-- =====================================================
PROMPT === 测试10: 查询审计日志 ===
SELECT AUDITID, TABLENAME, RECORDID, ACTIONTYPE, OPERATORID, OPERATIONTIME
FROM LOG_AUDITTRAILS
WHERE TABLENAME IN ('SYS_USERS', 'USER_BLACKLIST')
  AND OPERATORID LIKE 'user-%-a-group'
ORDER BY OPERATIONTIME DESC
FETCH FIRST 10 ROWS ONLY;

-- =====================================================
-- 测试11: 统计信息汇总
-- =====================================================
PROMPT === 测试11: A组数据统计汇总 ===
SELECT '角色总数' AS STATISTIC, COUNT(1) AS VALUE FROM SYS_ROLES WHERE ROLEID LIKE 'role-%-a-group'
UNION ALL
SELECT '用户总数' AS STATISTIC, COUNT(1) AS VALUE FROM SYS_USERS WHERE USERID LIKE 'user-%-a-group'
UNION ALL
SELECT '启用用户数' AS STATISTIC, COUNT(1) AS VALUE FROM SYS_USERS WHERE USERID LIKE 'user-%-a-group' AND STATUS = 'Active'
UNION ALL
SELECT '停用用户数' AS STATISTIC, COUNT(1) AS VALUE FROM SYS_USERS WHERE USERID LIKE 'user-%-a-group' AND STATUS = 'Inactive'
UNION ALL
SELECT '有效黑名单数' AS STATISTIC, COUNT(1) AS VALUE FROM USER_BLACKLIST WHERE BLACKLISTID LIKE 'bl-%-a-group' AND STATUS = 'Active'
UNION ALL
SELECT '已解除黑名单数' AS STATISTIC, COUNT(1) AS VALUE FROM USER_BLACKLIST WHERE BLACKLISTID LIKE 'bl-%-a-group' AND STATUS = 'Released'
UNION ALL
SELECT '审计日志数' AS STATISTIC, COUNT(1) AS VALUE FROM LOG_AUDITTRAILS WHERE OPERATORID LIKE 'user-%-a-group';

-- =====================================================
-- 测试12: 综合验证 - 用户权限检查
-- =====================================================
PROMPT === 测试12: 综合验证 - 用户权限检查 ===
SELECT 
    u.USERNAME,
    u.REALNAME,
    r.ROLENAME,
    r.PERMISSIONSCOPE,
    u.STATUS AS USER_STATUS,
    CASE 
        WHEN b.STATUS = 'Active' THEN '黑名单有效'
        WHEN b.STATUS = 'Released' THEN '黑名单已解除'
        ELSE '不在黑名单'
    END AS BLACKLIST_STATUS
FROM SYS_USERS u
LEFT JOIN SYS_ROLES r ON u.ROLEID = r.ROLEID
LEFT JOIN USER_BLACKLIST b ON u.USERID = b.USERID AND b.STATUS = 'Active'
WHERE u.USERID LIKE 'user-%-a-group'
ORDER BY u.USERNAME;

