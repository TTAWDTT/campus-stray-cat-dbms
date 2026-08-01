MERGE INTO SYS_ROLES target
USING (
    SELECT 'role-admin-a-group' AS ROLEID,
           'ADMIN' AS ROLENAME,
           'A组用户权限模块管理员' AS DESCRIPTION,
           'USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE' AS PERMISSIONSCOPE
    FROM DUAL
) source
ON (target.ROLEID = source.ROLEID)
WHEN MATCHED THEN UPDATE SET
    target.ROLENAME = source.ROLENAME,
    target.DESCRIPTION = source.DESCRIPTION,
    target.PERMISSIONSCOPE = source.PERMISSIONSCOPE
WHEN NOT MATCHED THEN INSERT
    (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
VALUES
    (source.ROLEID, source.ROLENAME, source.DESCRIPTION, source.PERMISSIONSCOPE);

MERGE INTO SYS_USERS target
USING (
    SELECT 'user-admin-a-group' AS USERID,
           'role-admin-a-group' AS ROLEID,
           'a_group_admin' AS USERNAME,
           'AQAAAAIAAYagAAAAEK2sAajzhA7dIlvCJAM656FKcRbzwy2Z1xwCA450N5PVrC2Evn53/GfU1TgnQIQ2Ig==' AS PASSWORDHASH,
           'A组管理员' AS REALNAME,
           'VERIFIED' AS VERIFYSTATUS,
           'ACTIVE' AS STATUS
    FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.VERIFYSTATUS, source.STATUS);


-- =====================================================
-- 1. 插入志愿者角色
-- =====================================================
MERGE INTO SYS_ROLES target
USING (
    SELECT 'role-volunteer-a-group' AS ROLEID,
           'VOLUNTEER' AS ROLENAME,
           'A组用户权限模块志愿者' AS DESCRIPTION,
           'CAT_MANAGE,ADOPTION_VIEW' AS PERMISSIONSCOPE
    FROM DUAL
) source
ON (target.ROLEID = source.ROLEID)
WHEN MATCHED THEN UPDATE SET
    target.ROLENAME = source.ROLENAME,
    target.DESCRIPTION = source.DESCRIPTION,
    target.PERMISSIONSCOPE = source.PERMISSIONSCOPE
WHEN NOT MATCHED THEN INSERT
    (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
VALUES
    (source.ROLEID, source.ROLENAME, source.DESCRIPTION, source.PERMISSIONSCOPE);

-- =====================================================
-- 2. 插入普通用户角色
-- =====================================================
MERGE INTO SYS_ROLES target
USING (
    SELECT 'role-user-a-group' AS ROLEID,
           'USER' AS ROLENAME,
           'A组用户权限模块普通用户' AS DESCRIPTION,
           'CAT_VIEW,ADOPTION_APPLY' AS PERMISSIONSCOPE
    FROM DUAL
) source
ON (target.ROLEID = source.ROLEID)
WHEN MATCHED THEN UPDATE SET
    target.ROLENAME = source.ROLENAME,
    target.DESCRIPTION = source.DESCRIPTION,
    target.PERMISSIONSCOPE = source.PERMISSIONSCOPE
WHEN NOT MATCHED THEN INSERT
    (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
VALUES
    (source.ROLEID, source.ROLENAME, source.DESCRIPTION, source.PERMISSIONSCOPE);

-- =====================================================
-- 3. 插入志愿者用户（用于测试角色分配）
-- =====================================================
MERGE INTO SYS_USERS target
USING (
    SELECT 'user-volunteer-a-group' AS USERID,
           'role-volunteer-a-group' AS ROLEID,
           'a_group_volunteer' AS USERNAME,
           'AQAAAAIAAYagAAAAEK2sAaJzhA7dI1vCJAM656FKcRbzwy2Z1xwCA45ON5PVrC2Evn53/GfU1TgNQI02Ig=' AS PASSWORDHASH,
           'A组志愿者' AS REALNAME,
           'VERIFIED' AS VERIFYSTATUS,
           'ACTIVE' AS STATUS
    FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.VERIFYSTATUS, source.STATUS);

-- =====================================================
-- 4. 插入普通用户（用于测试黑名单）
-- =====================================================
MERGE INTO SYS_USERS target
USING (
    SELECT 'user-common-a-group' AS USERID,
           'role-user-a-group' AS ROLEID,
           'a_group_user' AS USERNAME,
           'AQAAAAIAAYagAAAAEK2sAaJzhA7dI1vCJAM656FKcRbzwy2Z1xwCA45ON5PVrC2Evn53/GfU1TgNQI02Ig=' AS PASSWORDHASH,
           'A组普通用户' AS REALNAME,
           'VERIFIED' AS VERIFYSTATUS,
           'ACTIVE' AS STATUS
    FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.VERIFYSTATUS, source.STATUS);

-- =====================================================
-- 5. 插入一个被停用的用户（用于测试停用用户登录拦截）
-- =====================================================
MERGE INTO SYS_USERS target
USING (
    SELECT 'user-inactive-a-group' AS USERID,
           'role-user-a-group' AS ROLEID,
           'a_group_inactive' AS USERNAME,
           'AQAAAAIAAYagAAAAEK2sAaJzhA7dI1vCJAM656FKcRbzwy2Z1xwCA45ON5PVrC2Evn53/GfU1TgNQI02Ig=' AS PASSWORDHASH,
           'A组停用用户' AS REALNAME,
           'VERIFIED' AS VERIFYSTATUS,
           'INACTIVE' AS STATUS
    FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.VERIFYSTATUS, source.STATUS);

-- =====================================================
-- 6. 插入有效黑名单记录（用于测试黑名单功能）
-- =====================================================
MERGE INTO USER_BLACKLIST target
USING (
    SELECT 
        'bl-001-a-group' AS BLACKLISTID,
        'user-common-a-group' AS USERID,
        '违规领养' AS REASONTYPE,
        '多次领养后弃养猫咪，造成猫咪身心伤害' AS REASONDETAIL,
        NULL AS APPLICATIONID,
        'user-admin-a-group' AS CREATEDBY,
        SYSTIMESTAMP AS CREATEDAT,
        'Active' AS STATUS,
        NULL AS RELEASETIME,
        NULL AS RELEASEDBY
    FROM DUAL
) source
ON (target.BLACKLISTID = source.BLACKLISTID)
WHEN MATCHED THEN UPDATE SET
    target.USERID = source.USERID,
    target.REASONTYPE = source.REASONTYPE,
    target.REASONDETAIL = source.REASONDETAIL,
    target.APPLICATIONID = source.APPLICATIONID,
    target.CREATEDBY = source.CREATEDBY,
    target.CREATEDAT = source.CREATEDAT,
    target.STATUS = source.STATUS,
    target.RELEASETIME = source.RELEASETIME,
    target.RELEASEDBY = source.RELEASEDBY
WHEN NOT MATCHED THEN INSERT
    (BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, APPLICATIONID, 
     CREATEDBY, CREATEDAT, STATUS, RELEASETIME, RELEASEDBY)
VALUES
    (source.BLACKLISTID, source.USERID, source.REASONTYPE, source.REASONDETAIL, source.APPLICATIONID,
     source.CREATEDBY, source.CREATEDAT, source.STATUS, source.RELEASETIME, source.RELEASEDBY);

-- =====================================================
-- 7. 插入已解除的黑名单记录（用于演示历史保留）
-- =====================================================
MERGE INTO USER_BLACKLIST target
USING (
    SELECT 
        'bl-002-a-group' AS BLACKLISTID,
        'user-volunteer-a-group' AS USERID,
        '虚假信息' AS REASONTYPE,
        '提供虚假领养申请信息' AS REASONDETAIL,
        NULL AS APPLICATIONID,
        'user-admin-a-group' AS CREATEDBY,
        SYSTIMESTAMP - 30 AS CREATEDAT,
        'Released' AS STATUS,
        SYSTIMESTAMP - 15 AS RELEASETIME,
        'user-admin-a-group' AS RELEASEDBY
    FROM DUAL
) source
ON (target.BLACKLISTID = source.BLACKLISTID)
WHEN MATCHED THEN UPDATE SET
    target.USERID = source.USERID,
    target.REASONTYPE = source.REASONTYPE,
    target.REASONDETAIL = source.REASONDETAIL,
    target.APPLICATIONID = source.APPLICATIONID,
    target.CREATEDBY = source.CREATEDBY,
    target.CREATEDAT = source.CREATEDAT,
    target.STATUS = source.STATUS,
    target.RELEASETIME = source.RELEASETIME,
    target.RELEASEDBY = source.RELEASEDBY
WHEN NOT MATCHED THEN INSERT
    (BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, APPLICATIONID, 
     CREATEDBY, CREATEDAT, STATUS, RELEASETIME, RELEASEDBY)
VALUES
    (source.BLACKLISTID, source.USERID, source.REASONTYPE, source.REASONDETAIL, source.APPLICATIONID,
     source.CREATEDBY, source.CREATEDAT, source.STATUS, source.RELEASETIME, source.RELEASEDBY);

-- =====================================================
-- 8. 插入第二条有效黑名单记录（用于演示多个黑名单）
-- =====================================================
MERGE INTO USER_BLACKLIST target
USING (
    SELECT 
        'bl-003-a-group' AS BLACKLISTID,
        'user-inactive-a-group' AS USERID,
        '恶意行为' AS REASONTYPE,
        '在校园内恶意伤害流浪猫' AS REASONDETAIL,
        NULL AS APPLICATIONID,
        'user-admin-a-group' AS CREATEDBY,
        SYSTIMESTAMP - 5 AS CREATEDAT,
        'Active' AS STATUS,
        NULL AS RELEASETIME,
        NULL AS RELEASEDBY
    FROM DUAL
) source
ON (target.BLACKLISTID = source.BLACKLISTID)
WHEN MATCHED THEN UPDATE SET
    target.USERID = source.USERID,
    target.REASONTYPE = source.REASONTYPE,
    target.REASONDETAIL = source.REASONDETAIL,
    target.APPLICATIONID = source.APPLICATIONID,
    target.CREATEDBY = source.CREATEDBY,
    target.CREATEDAT = source.CREATEDAT,
    target.STATUS = source.STATUS,
    target.RELEASETIME = source.RELEASETIME,
    target.RELEASEDBY = source.RELEASEDBY
WHEN NOT MATCHED THEN INSERT
    (BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, APPLICATIONID, 
     CREATEDBY, CREATEDAT, STATUS, RELEASETIME, RELEASEDBY)
VALUES
    (source.BLACKLISTID, source.USERID, source.REASONTYPE, source.REASONDETAIL, source.APPLICATIONID,
     source.CREATEDBY, source.CREATEDAT, source.STATUS, source.RELEASETIME, source.RELEASEDBY);

-- =====================================================
-- 9. 验证数据插入结果
-- =====================================================
PROMPT ==========================================
PROMPT A组演示数据插入完成！验证结果：
PROMPT ==========================================

-- 查询角色
PROMPT --- 角色列表 ---
SELECT ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE 
FROM SYS_ROLES 
WHERE ROLEID LIKE 'role-%-a-group'
ORDER BY ROLENAME;

-- 查询用户
PROMPT --- 用户列表 ---
SELECT USERID, USERNAME, REALNAME, ROLEID, STATUS 
FROM SYS_USERS 
WHERE USERID LIKE 'user-%-a-group'
ORDER BY USERNAME;

-- 查询黑名单
PROMPT --- 黑名单列表 ---
SELECT BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, STATUS, CREATEDAT
FROM USER_BLACKLIST 
WHERE BLACKLISTID LIKE 'bl-%-a-group'
ORDER BY CREATEDAT DESC;

-- 查询有效黑名单视图
PROMPT --- 有效黑名单视图 ---
SELECT * FROM VW_ACTIVE_BLACKLIST_USERS;

