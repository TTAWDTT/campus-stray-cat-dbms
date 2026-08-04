SET DEFINE OFF;

-- A组成员1演示数据（用户登录 / 用户信息管理）
-- 状态契约（API 与库一致，仅使用英文）：
--   SYS_USERS.STATUS: ACTIVE | DISABLED
--   SYS_USERS.VERIFYSTATUS: VERIFIED | UNVERIFIED
--   SYS_ROLES.ROLENAME: ADMIN | VOLUNTEER | USER
-- 演示登录密码（四类账号相同）：Passw0rd!
-- PASSWORDHASH 为 ASP.NET Core Identity PasswordHasher V3（PBKDF2-HMAC-SHA512）格式。

MERGE INTO SYS_ROLES target
USING (
    SELECT 'role-admin-a-group' AS ROLEID,
           'ADMIN' AS ROLENAME,
           '系统管理员' AS DESCRIPTION,
           'USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE' AS PERMISSIONSCOPE
    FROM DUAL
    UNION ALL
    SELECT 'role-volunteer-a-group',
           'VOLUNTEER',
           '校园志愿者',
           'CAT_VIEW,SIGHTING_WRITE,SHIFT_CHECKIN'
    FROM DUAL
    UNION ALL
    SELECT 'role-user-a-group',
           'USER',
           '普通用户',
           'CAT_VIEW,ADOPT_APPLY'
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
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==' AS PASSWORDHASH,
           'A组管理员' AS REALNAME,
           'A20260001' AS STUDENTNO,
           '13800000001' AS PHONE,
           'VERIFIED' AS VERIFYSTATUS,
           'ACTIVE' AS STATUS
    FROM DUAL
    UNION ALL
    SELECT 'user-volunteer-a-group',
           'role-volunteer-a-group',
           'a_group_volunteer',
           'AQAAAAIAAYagAAAAEFWxQWKCxqXfgM+7c9Ayi8qoW1C1OGzqu/kO19VHFPMdcmf8jAcmeINy1p+5J1dyHQ==',
           'A组志愿者',
           'A20260002',
           '13800000002',
           'VERIFIED',
           'ACTIVE'
    FROM DUAL
    UNION ALL
    SELECT 'user-normal-a-group',
           'role-user-a-group',
           'a_group_user',
           'AQAAAAIAAYagAAAAEE4L1+YIfNuVyS5gjoVjFUjLRF+28wvRH7dcuymlNXliJ3nl11B35NohPBOw/yPQuQ==',
           'A组普通用户',
           'A20260003',
           '13800000003',
           'UNVERIFIED',
           'ACTIVE'
    FROM DUAL
    UNION ALL
    SELECT 'user-disabled-a-group',
           'role-user-a-group',
           'a_group_disabled',
           'AQAAAAIAAYagAAAAEE0qxO1PFTvWdv36axASUjjuvX3ztsRdbJ4f2M+sGdBEjvl132UtcKxFSs+fBWqrJQ==',
           'A组停用用户',
           'A20260004',
           '13800000004',
           'VERIFIED',
           'DISABLED'
    FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.STUDENTNO = source.STUDENTNO,
    target.PHONE = source.PHONE,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, STUDENTNO, PHONE, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.STUDENTNO, source.PHONE, source.VERIFYSTATUS, source.STATUS);

PROMPT A-group member1 demo users ready. Login password: Passw0rd!
/

-- =====================================================
-- 成员2：黑名单演示数据
-- =====================================================
MERGE INTO USER_BLACKLIST target
USING (
    SELECT 'bl-001-a-group' AS BLACKLISTID, 'user-normal-a-group' AS USERID, '违规领养' AS REASONTYPE, '多次领养后弃养猫咪，造成猫咪身心伤害' AS REASONDETAIL, NULL AS RELATEDAPPLICATIONID, 'user-admin-a-group' AS CREATEUSERID, SYSDATE AS CREATETIME, 'Active' AS BLACKLISTSTATUS, NULL AS RELEASETIME, NULL AS RELEASEDBY FROM DUAL
    UNION ALL
    SELECT 'bl-002-a-group', 'user-disabled-a-group', '恶意行为', '在校园内恶意伤害流浪猫', NULL, 'user-admin-a-group', SYSDATE-5, 'Active', NULL, NULL FROM DUAL
    UNION ALL
    SELECT 'bl-003-a-group', 'user-volunteer-a-group', '虚假信息', '提供虚假领养申请信息', NULL, 'user-admin-a-group', SYSDATE-30, 'Released', SYSDATE-15, 'user-admin-a-group' FROM DUAL
) source
ON (target.BLACKLISTID = source.BLACKLISTID)
WHEN MATCHED THEN UPDATE SET
    target.USERID = source.USERID,
    target.REASONTYPE = source.REASONTYPE,
    target.REASONDETAIL = source.REASONDETAIL,
    target.RELATEDAPPLICATIONID = source.RELATEDAPPLICATIONID,
    target.CREATEUSERID = source.CREATEUSERID,
    target.CREATETIME = source.CREATETIME,
    target.BLACKLISTSTATUS = source.BLACKLISTSTATUS,
    target.RELEASETIME = source.RELEASETIME,
    target.RELEASEDBY = source.RELEASEDBY
WHEN NOT MATCHED THEN INSERT (BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, RELATEDAPPLICATIONID, CREATEUSERID, CREATETIME, BLACKLISTSTATUS, RELEASETIME, RELEASEDBY)
VALUES (source.BLACKLISTID, source.USERID, source.REASONTYPE, source.REASONDETAIL, source.RELATEDAPPLICATIONID, source.CREATEUSERID, source.CREATETIME, source.BLACKLISTSTATUS, source.RELEASETIME, source.RELEASEDBY);

PROMPT A-group member2 blacklist demo data ready.
