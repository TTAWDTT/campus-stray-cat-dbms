SET DEFINE OFF;
SET SERVEROUTPUT ON;

-- A组成员1高级 SQL：用户-角色视图与用户筛选索引。
-- 约定：后端 UserRepository 查询 VW_USER_ROLE_PROFILE（含 PASSWORDHASH，仅服务端使用，API 不回传）。
-- 状态契约：STATUS = ACTIVE | DISABLED；VERIFYSTATUS = VERIFIED | UNVERIFIED。

CREATE OR REPLACE PROCEDURE create_index_if_not_exists(p_sql IN VARCHAR2) AS
BEGIN
    EXECUTE IMMEDIATE p_sql;
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN
            RAISE;
        END IF;
END;
/

CREATE OR REPLACE VIEW VW_USER_ROLE_PROFILE AS
SELECT u.USERID,
       u.ROLEID,
       u.USERNAME,
       u.PASSWORDHASH,
       u.REALNAME,
       u.STUDENTNO,
       u.PHONE,
       u.VERIFYSTATUS,
       u.STATUS,
       r.ROLENAME,
       r.PERMISSIONSCOPE
FROM SYS_USERS u
JOIN SYS_ROLES r ON r.ROLEID = u.ROLEID;

-- 活跃用户简表：列表筛选演示（不含密码哈希）。
CREATE OR REPLACE VIEW VW_ACTIVE_USER_SUMMARIES AS
SELECT USERID,
       ROLEID,
       USERNAME,
       REALNAME,
       STUDENTNO,
       PHONE,
       VERIFYSTATUS,
       STATUS,
       ROLENAME,
       PERMISSIONSCOPE
FROM VW_USER_ROLE_PROFILE
WHERE STATUS = 'ACTIVE';

BEGIN
    create_index_if_not_exists(
        'CREATE INDEX IDX_SYS_USERS_STATUS_ROLEID ON SYS_USERS (STATUS, ROLEID)');
    create_index_if_not_exists(
        'CREATE INDEX IDX_SYS_USERS_USERNAME_UPPER ON SYS_USERS (UPPER(USERNAME))');
END;
/

PROMPT A-group member1 advanced SQL ready.
/

-- =====================================================
-- 以下为成员2：角色权限与用户黑名单模块
-- =====================================================

-- 1. 黑名单索引
BEGIN
    EXECUTE IMMEDIATE 'CREATE INDEX IDX_BLACKLIST_USER_STATUS ON USER_BLACKLIST (USERID, BLACKLISTSTATUS)';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -955 THEN RAISE; END IF;
END;
/

-- 2. 视图：有效黑名单用户
CREATE OR REPLACE VIEW VW_ACTIVE_BLACKLIST_USERS AS
SELECT
    b.BLACKLISTID,
    b.USERID,
    u.USERNAME,
    u.REALNAME,
    u.STUDENTNO,
    u.PHONE,
    b.REASONTYPE,
    b.REASONDETAIL,
    b.RELATEDAPPLICATIONID,
    b.CREATEUSERID,
    b.CREATETIME,
    b.BLACKLISTSTATUS
FROM USER_BLACKLIST b
INNER JOIN SYS_USERS u ON b.USERID = u.USERID
WHERE b.BLACKLISTSTATUS = 'Active'
  AND u.STATUS = 'ACTIVE';

-- 3. 分配角色存储过程
CREATE OR REPLACE PROCEDURE SP_ASSIGN_USER_ROLE (
    p_UserID     IN VARCHAR2,
    p_NewRoleID  IN VARCHAR2,
    p_OperatorID IN VARCHAR2,
    p_Result     OUT VARCHAR2
)
IS
    v_OldRoleID  VARCHAR2(36);
    v_UserExists NUMBER;
    v_RoleExists NUMBER;
BEGIN
    SAVEPOINT SP_ASSIGN_USER_ROLE_START;

    SELECT COUNT(1) INTO v_UserExists FROM SYS_USERS WHERE USERID = p_UserID;
    IF v_UserExists = 0 THEN
        p_Result := '用户不存在';
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        RETURN;
    END IF;

    SELECT COUNT(1) INTO v_RoleExists FROM SYS_ROLES WHERE ROLEID = p_NewRoleID;
    IF v_RoleExists = 0 THEN
        p_Result := '角色不存在';
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        RETURN;
    END IF;

    SELECT ROLEID INTO v_OldRoleID FROM SYS_USERS WHERE USERID = p_UserID;
    IF v_OldRoleID = p_NewRoleID THEN
        p_Result := '';
        RETURN;
    END IF;

    UPDATE SYS_USERS SET ROLEID = p_NewRoleID WHERE USERID = p_UserID;

    COMMIT;
    p_Result := '';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        p_Result := '分配角色失败: ' || SQLERRM;
END SP_ASSIGN_USER_ROLE;
/

-- 4. 加入黑名单存储过程
CREATE OR REPLACE PROCEDURE SP_ADD_USER_BLACKLIST (
    p_UserID IN VARCHAR2,
    p_ReasonType IN VARCHAR2,
    p_ReasonDetail IN VARCHAR2,
    p_CreatedBy IN VARCHAR2,
    p_Result OUT VARCHAR2,
    p_ApplicationID IN VARCHAR2 DEFAULT NULL
)
IS
    v_ActiveCount NUMBER;
    v_BlacklistID VARCHAR2(36);
BEGIN
    SELECT COUNT(1) INTO v_ActiveCount FROM SYS_USERS WHERE USERID = p_UserID;
    IF v_ActiveCount = 0 THEN
        p_Result := '用户不存在';
        RETURN;
    END IF;

    SELECT COUNT(1) INTO v_ActiveCount
    FROM USER_BLACKLIST
    WHERE USERID = p_UserID AND BLACKLISTSTATUS = 'Active';

    IF v_ActiveCount > 0 THEN
        p_Result := '该用户已在黑名单中，请勿重复拉黑';
        RETURN;
    END IF;

    v_BlacklistID := SYS_GUID();
    INSERT INTO USER_BLACKLIST (
        BLACKLISTID, USERID, REASONTYPE, REASONDETAIL, RELATEDAPPLICATIONID,
        CREATEUSERID, CREATETIME, BLACKLISTSTATUS
    ) VALUES (
        v_BlacklistID, p_UserID, p_ReasonType, p_ReasonDetail, p_ApplicationID,
        p_CreatedBy, SYSDATE, 'Active'
    );

    p_Result := '';
    COMMIT;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_Result := '加入黑名单失败: ' || SQLERRM;
END SP_ADD_USER_BLACKLIST;
/

-- 5. 解除黑名单存储过程
CREATE OR REPLACE PROCEDURE SP_RELEASE_USER_BLACKLIST (
    p_BlacklistID  IN  VARCHAR2,
    p_ReleasedBy   IN  VARCHAR2,
    p_Result       OUT VARCHAR2
)
IS
    v_Status       VARCHAR2(20);
BEGIN
    SAVEPOINT SP_RELEASE_USER_BLACKLIST_START;

    SELECT BLACKLISTSTATUS INTO v_Status
    FROM USER_BLACKLIST WHERE BLACKLISTID = p_BlacklistID;

    IF v_Status = 'Released' THEN
        p_Result := '该黑名单记录已被解除';
        ROLLBACK TO SP_RELEASE_USER_BLACKLIST_START;
        RETURN;
    END IF;

    UPDATE USER_BLACKLIST
    SET BLACKLISTSTATUS = 'Released',
        RELEASETIME = SYSTIMESTAMP,
        RELEASEDBY = p_ReleasedBy
    WHERE BLACKLISTID = p_BlacklistID;

    COMMIT;
    p_Result := '';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK TO SP_RELEASE_USER_BLACKLIST_START;
        p_Result := '黑名单记录不存在';
    WHEN OTHERS THEN
        ROLLBACK TO SP_RELEASE_USER_BLACKLIST_START;
        p_Result := '解除黑名单失败: ' || SQLERRM;
END SP_RELEASE_USER_BLACKLIST;
/

PROMPT A-group member2 advanced SQL ready.
