CREATE OR REPLACE VIEW VW_USER_ROLE_PROFILE AS
SELECT u.USERID,
       u.ROLEID,
       u.USERNAME,
       u.REALNAME,
       u.STUDENTNO,
       u.PHONE,
       u.VERIFYSTATUS,
       u.STATUS,
       r.ROLENAME,
       r.PERMISSIONSCOPE
FROM SYS_USERS u
JOIN SYS_ROLES r ON r.ROLEID = u.ROLEID;

CREATE INDEX IDX_SYS_USERS_STATUS_ROLEID ON SYS_USERS (STATUS, ROLEID);
CREATE INDEX IDX_SYS_USERS_USERNAME_UPPER ON SYS_USERS (UPPER(USERNAME));


-- =====================================================
-- 1. 黑名单索引: 优化查询性能
-- =====================================================
CREATE INDEX IDX_BLACKLIST_USER_STATUS ON USER_BLACKLIST (UserID, Status);

-- =====================================================
-- 2. 视图: 有效黑名单用户（供领养审核模块使用）
-- =====================================================
CREATE OR REPLACE VIEW VW_ACTIVE_BLACKLIST_USERS AS
SELECT 
    b.BlacklistID,
    b.UserID,
    u.Username,
    u.RealName,
    u.StudentNo,
    u.Phone,
    b.ReasonType,
    b.ReasonDetail,
    b.ApplicationID,
    b.CreatedBy,
    b.CreatedAt,
    b.Status
FROM USER_BLACKLIST b
INNER JOIN SYS_USERS u ON b.UserID = u.UserID
WHERE b.Status = 'Active'
  AND u.Status = 'Active'
ORDER BY b.CreatedAt DESC;

-- =====================================================
-- 3. 存储过程: 分配角色（含审计日志）
-- =====================================================
CREATE OR REPLACE PROCEDURE SP_ASSIGN_USER_ROLE (
    p_UserID      IN  VARCHAR2,
    p_NewRoleID   IN  VARCHAR2,
    p_OperatorID  IN  VARCHAR2,
    p_Result      OUT VARCHAR2
)
IS
    v_OldRoleID   SYS_USERS.RoleID%TYPE;
    v_OldRoleName SYS_ROLES.RoleName%TYPE;
    v_NewRoleName SYS_ROLES.RoleName%TYPE;
    v_UserExists  NUMBER;
    v_RoleExists  NUMBER;
BEGIN
    -- 开启事务保存点
    SAVEPOINT SP_ASSIGN_USER_ROLE_START;

    -- 检查用户是否存在
    SELECT COUNT(1) INTO v_UserExists FROM SYS_USERS WHERE UserID = p_UserID;
    IF v_UserExists = 0 THEN
        p_Result := '用户不存在';
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        RETURN;
    END IF;

    -- 检查新角色是否存在且启用
    SELECT COUNT(1) INTO v_RoleExists FROM SYS_ROLES WHERE RoleID = p_NewRoleID AND IsActive = '1';
    IF v_RoleExists = 0 THEN
        p_Result := '角色不存在或已停用';
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        RETURN;
    END IF;

    -- 查询用户当前角色
    SELECT RoleID INTO v_OldRoleID FROM SYS_USERS WHERE UserID = p_UserID;

    -- 如果新旧角色相同，直接返回成功
    IF v_OldRoleID = p_NewRoleID THEN
        p_Result := '';
        RETURN;
    END IF;

    -- 获取角色名称（用于审计日志）
    SELECT RoleName INTO v_OldRoleName FROM SYS_ROLES WHERE RoleID = v_OldRoleID;
    SELECT RoleName INTO v_NewRoleName FROM SYS_ROLES WHERE RoleID = p_NewRoleID;

    -- 更新用户角色
    UPDATE SYS_USERS 
    SET RoleID = p_NewRoleID 
    WHERE UserID = p_UserID;

    -- 写入审计日志
    INSERT INTO LOG_AUDITTRAILS (
        AuditID,
        TableName,
        RecordID,
        ActionType,
        OldValue,
        NewValue,
        OperatorID,
        OperationTime
    ) VALUES (
        SYS_GUID(),
        'SYS_USERS',
        p_UserID,
        'UPDATE_ROLE',
        '{"RoleID":"' || v_OldRoleID || '","RoleName":"' || v_OldRoleName || '"}',
        '{"RoleID":"' || p_NewRoleID || '","RoleName":"' || v_NewRoleName || '"}',
        p_OperatorID,
        SYSTIMESTAMP
    );

    -- 提交事务
    COMMIT;
    p_Result := '';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK TO SP_ASSIGN_USER_ROLE_START;
        p_Result := '分配角色失败: ' || SQLERRM;
END SP_ASSIGN_USER_ROLE;
/

-- =====================================================
-- 4. 存储过程: 加入黑名单（防重复 + 审计）
-- =====================================================
CREATE OR REPLACE PROCEDURE SP_ADD_USER_BLACKLIST (
    p_UserID        IN  VARCHAR2,
    p_ReasonType    IN  VARCHAR2,
    p_ReasonDetail  IN  VARCHAR2,
    p_ApplicationID IN  VARCHAR2 DEFAULT NULL,
    p_CreatedBy     IN  VARCHAR2,
    p_Result        OUT VARCHAR2
)
IS
    v_ActiveCount   NUMBER;
    v_BlacklistID   VARCHAR2(36);
    v_UserName      SYS_USERS.Username%TYPE;
BEGIN
    -- 开启事务保存点
    SAVEPOINT SP_ADD_USER_BLACKLIST_START;

    -- 检查用户是否存在
    SELECT COUNT(1), Username 
    INTO v_ActiveCount, v_UserName
    FROM SYS_USERS 
    WHERE UserID = p_UserID;
    
    IF v_ActiveCount = 0 THEN
        p_Result := '用户不存在';
        ROLLBACK TO SP_ADD_USER_BLACKLIST_START;
        RETURN;
    END IF;

    -- 检查用户是否已有有效黑名单记录
    SELECT COUNT(1) INTO v_ActiveCount
    FROM USER_BLACKLIST
    WHERE UserID = p_UserID AND Status = 'Active';

    IF v_ActiveCount > 0 THEN
        p_Result := '该用户已在黑名单中，请勿重复拉黑';
        ROLLBACK TO SP_ADD_USER_BLACKLIST_START;
        RETURN;
    END IF;

    -- 生成黑名单记录ID
    v_BlacklistID := SYS_GUID();

    -- 插入黑名单记录
    INSERT INTO USER_BLACKLIST (
        BlacklistID,
        UserID,
        ReasonType,
        ReasonDetail,
        ApplicationID,
        CreatedBy,
        CreatedAt,
        Status
    ) VALUES (
        v_BlacklistID,
        p_UserID,
        p_ReasonType,
        p_ReasonDetail,
        p_ApplicationID,
        p_CreatedBy,
        SYSTIMESTAMP,
        'Active'
    );

    -- 写入审计日志
    INSERT INTO LOG_AUDITTRAILS (
        AuditID,
        TableName,
        RecordID,
        ActionType,
        OldValue,
        NewValue,
        OperatorID,
        OperationTime
    ) VALUES (
        SYS_GUID(),
        'USER_BLACKLIST',
        v_BlacklistID,
        'INSERT',
        NULL,
        '{"UserID":"' || p_UserID || '","UserName":"' || v_UserName || '","ReasonType":"' || p_ReasonType || '","ReasonDetail":"' || p_ReasonDetail || '"}',
        p_CreatedBy,
        SYSTIMESTAMP
    );

    -- 提交事务
    COMMIT;
    p_Result := '';

EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK TO SP_ADD_USER_BLACKLIST_START;
        p_Result := '加入黑名单失败: ' || SQLERRM;
END SP_ADD_USER_BLACKLIST;
/

-- =====================================================
-- 5. 存储过程: 解除黑名单（保留历史 + 审计）
-- =====================================================
CREATE OR REPLACE PROCEDURE SP_RELEASE_USER_BLACKLIST (
    p_BlacklistID  IN  VARCHAR2,
    p_ReleasedBy   IN  VARCHAR2,
    p_Result       OUT VARCHAR2
)
IS
    v_Status       USER_BLACKLIST.Status%TYPE;
    v_UserID       USER_BLACKLIST.UserID%TYPE;
    v_UserName     SYS_USERS.Username%TYPE;
BEGIN
    -- 开启事务保存点
    SAVEPOINT SP_RELEASE_USER_BLACKLIST_START;

    -- 检查黑名单记录是否存在，并获取状态和用户ID
    SELECT Status, UserID INTO v_Status, v_UserID
    FROM USER_BLACKLIST
    WHERE BlacklistID = p_BlacklistID;

    -- 检查是否已解除
    IF v_Status = 'Released' THEN
        p_Result := '该黑名单记录已被解除';
        ROLLBACK TO SP_RELEASE_USER_BLACKLIST_START;
        RETURN;
    END IF;

    -- 获取用户姓名
    SELECT Username INTO v_UserName
    FROM SYS_USERS
    WHERE UserID = v_UserID;

    -- 更新黑名单记录（解除）
    UPDATE USER_BLACKLIST
    SET Status = 'Released',
        ReleaseTime = SYSTIMESTAMP,
        ReleasedBy = p_ReleasedBy
    WHERE BlacklistID = p_BlacklistID;

    -- 写入审计日志
    INSERT INTO LOG_AUDITTRAILS (
        AuditID,
        TableName,
        RecordID,
        ActionType,
        OldValue,
        NewValue,
        OperatorID,
        OperationTime
    ) VALUES (
        SYS_GUID(),
        'USER_BLACKLIST',
        p_BlacklistID,
        'RELEASE',
        '{"Status":"Active","UserID":"' || v_UserID || '","UserName":"' || v_UserName || '"}',
        '{"Status":"Released","ReleasedBy":"' || p_ReleasedBy || '","ReleaseTime":"' || TO_CHAR(SYSTIMESTAMP, 'YYYY-MM-DD HH24:MI:SS') || '"}',
        p_ReleasedBy,
        SYSTIMESTAMP
    );

    -- 提交事务
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
