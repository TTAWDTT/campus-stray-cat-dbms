PROMPT Preparing demo data for rescue 14/15/16...

-- 验收脚本使用的最小基础数据：角色、用户、区域、猫咪、健康记录。
DECLARE
BEGIN
    INSERT INTO SYS_ROLES (
        ROLEID,
        ROLENAME,
        DESCRIPTION,
        PERMISSIONSCOPE
    )
    SELECT 'role-demo-141516',
           'Rescue Demo Role',
           'Demo role for rescue module acceptance test',
           'RESCUE'
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM SYS_ROLES WHERE ROLEID = 'role-demo-141516'
    );

    INSERT INTO SYS_USERS (
        USERID,
        ROLEID,
        USERNAME,
        PASSWORDHASH,
        REALNAME,
        VERIFYSTATUS,
        STATUS
    )
    SELECT 'user-demo-reporter-141516',
           'role-demo-141516',
           'reporter_demo_141516',
           'demo-password-hash',
           'Demo Reporter',
           'VERIFIED',
           'ACTIVE'
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM SYS_USERS WHERE USERID = 'user-demo-reporter-141516'
    );

    INSERT INTO SYS_USERS (
        USERID,
        ROLEID,
        USERNAME,
        PASSWORDHASH,
        REALNAME,
        VERIFYSTATUS,
        STATUS
    )
    SELECT 'user-demo-handler-141516',
           'role-demo-141516',
           'handler_demo_141516',
           'demo-password-hash',
           'Demo Handler',
           'VERIFIED',
           'ACTIVE'
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM SYS_USERS WHERE USERID = 'user-demo-handler-141516'
    );

    INSERT INTO MAP_CAMPUSAREAS (
        AREAID,
        AREANAME,
        CAMPUSNAME,
        AREATYPE,
        RISKLEVEL
    )
    SELECT 'area-demo-141516',
           'Library East Gate',
           'Main Campus',
           'GATE',
           'MEDIUM'
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM MAP_CAMPUSAREAS WHERE AREAID = 'area-demo-141516'
    );

    INSERT INTO CAT_CATS (
        CATID,
        CATNAME,
        GENDER,
        COLORPATTERN,
        MAINAREAID,
        LIFESTATUS,
        ARCHIVESTATUS
    )
    SELECT 'cat-demo-141516',
           'Demo Cat',
           'UNKNOWN',
           'Orange white',
           'area-demo-141516',
           'ACTIVE',
           'NORMAL'
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM CAT_CATS WHERE CATID = 'cat-demo-141516'
    );

    INSERT INTO MED_HEALTHRECORDS (
        RECORDID,
        CATID,
        RECORDTYPE,
        HOSPITALNAME,
        DIAGNOSIS,
        RECORDDATE,
        NEXTDUEDATE
    )
    SELECT 'health-demo-141516',
           'cat-demo-141516',
           'VACCINE',
           'Demo Animal Hospital',
           'Routine vaccination',
           SYSDATE,
           SYSDATE + 30
    FROM DUAL
    WHERE NOT EXISTS (
        SELECT 1 FROM MED_HEALTHRECORDS WHERE RECORDID = 'health-demo-141516'
    );

    COMMIT;
END;
/
