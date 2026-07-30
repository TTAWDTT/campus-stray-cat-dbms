SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== Preparing demo data for rescue 14/15/16 =====;

DECLARE
    V_COUNT NUMBER;
BEGIN
    SELECT COUNT(*) INTO V_COUNT FROM SYS_ROLES WHERE ROLEID = 'role-demo-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO SYS_ROLES (
            ROLEID,
            ROLENAME,
            DESCRIPTION,
            PERMISSIONSCOPE
        ) VALUES (
            'role-demo-141516',
            'DEMO_ROLE',
            'Demo role for rescue 14/15/16 acceptance test',
            'RESCUE'
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM SYS_USERS WHERE USERID = 'user-demo-reporter-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO SYS_USERS (
            USERID,
            ROLEID,
            USERNAME,
            PASSWORDHASH,
            REALNAME,
            VERIFYSTATUS,
            STATUS
        ) VALUES (
            'user-demo-reporter-141516',
            'role-demo-141516',
            'demo_reporter_141516',
            'demo_hash',
            'Demo Reporter',
            'VERIFIED',
            'ACTIVE'
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM SYS_USERS WHERE USERID = 'user-demo-handler-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO SYS_USERS (
            USERID,
            ROLEID,
            USERNAME,
            PASSWORDHASH,
            REALNAME,
            VERIFYSTATUS,
            STATUS
        ) VALUES (
            'user-demo-handler-141516',
            'role-demo-141516',
            'demo_handler_141516',
            'demo_hash',
            'Demo Handler',
            'VERIFIED',
            'ACTIVE'
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM MAP_CAMPUSAREAS WHERE AREAID = 'area-demo-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO MAP_CAMPUSAREAS (
            AREAID,
            AREANAME,
            CAMPUSNAME,
            AREATYPE,
            RISKLEVEL
        ) VALUES (
            'area-demo-141516',
            'Library East Gate',
            'Main Campus',
            'BUILDING_GATE',
            'MEDIUM'
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM CAT_CATS WHERE CATID = 'cat-demo-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO CAT_CATS (
            CATID,
            CATNAME,
            GENDER,
            COLORPATTERN,
            STERILIZEDFLAG,
            EARTIPFLAG,
            MAINAREAID,
            LIFESTATUS,
            ARCHIVESTATUS
        ) VALUES (
            'cat-demo-141516',
            'Orange Cat',
            'UNKNOWN',
            'Orange and White',
            0,
            0,
            'area-demo-141516',
            'ACTIVE',
            'NORMAL'
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM MED_HEALTHRECORDS WHERE RECORDID = 'health-demo-auto-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO MED_HEALTHRECORDS (
            RECORDID,
            CATID,
            RECORDTYPE,
            HOSPITALNAME,
            DIAGNOSIS,
            RECORDDATE,
            NEXTDUEDATE
        ) VALUES (
            'health-demo-auto-141516',
            'cat-demo-141516',
            'VACCINE',
            'Campus Pet Clinic',
            'Vaccine given, follow-up in 30 days',
            SYSDATE,
            SYSDATE + 30
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM MED_HEALTHRECORDS WHERE RECORDID = 'health-demo-manual-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO MED_HEALTHRECORDS (
            RECORDID,
            CATID,
            RECORDTYPE,
            HOSPITALNAME,
            DIAGNOSIS,
            RECORDDATE,
            NEXTDUEDATE
        ) VALUES (
            'health-demo-manual-141516',
            'cat-demo-141516',
            'DEWORM',
            'Campus Pet Clinic',
            'Deworming completed, reminder will be created manually',
            SYSDATE,
            NULL
        );
    END IF;

    SELECT COUNT(*) INTO V_COUNT FROM CAT_SIGHTINGS WHERE SIGHTINGID = 'sighting-demo-141516';
    IF V_COUNT = 0 THEN
        INSERT INTO CAT_SIGHTINGS (
            SIGHTINGID,
            CATID,
            USERID,
            AREAID,
            LONGITUDE,
            LATITUDE,
            SIGHTINGTIME,
            REMARK
        ) VALUES (
            'sighting-demo-141516',
            'cat-demo-141516',
            'user-demo-reporter-141516',
            'area-demo-141516',
            120.12345678,
            30.12345678,
            SYSDATE - 8,
            'Last seen near the Library East Gate'
        );
    END IF;

    COMMIT;

    PKG_RESCUE_141516.AUTO_CREATE_REMINDERS_FROM_HEALTH;
END;
/

PROMPT ===== Requirement 14: reminders =====;

DECLARE
    V_AUTO_REMINDERID   MED_REMINDERS.REMINDERID%TYPE;
BEGIN
    SELECT MIN(REMINDERID)
    INTO V_AUTO_REMINDERID
    FROM MED_REMINDERS
    WHERE RECORDID = 'health-demo-auto-141516';

    DBMS_OUTPUT.PUT_LINE('Auto reminder: ' || V_AUTO_REMINDERID);
END;
/

SELECT
    REMINDERID,
    REMINDERTYPE,
    SENDSTATUS,
    REMINDERTIME
FROM V_MED_PENDING_REMINDERS
WHERE RECORDID = 'health-demo-auto-141516';

DECLARE
    V_AUTO_REMINDERID   MED_REMINDERS.REMINDERID%TYPE;
    V_MANUAL_REMINDERID MED_REMINDERS.REMINDERID%TYPE;
BEGIN
    SELECT MIN(REMINDERID)
    INTO V_AUTO_REMINDERID
    FROM MED_REMINDERS
    WHERE RECORDID = 'health-demo-auto-141516';

    PKG_RESCUE_141516.MARK_REMINDER_SENT(V_AUTO_REMINDERID);
    PKG_RESCUE_141516.COMPLETE_REMINDER(V_AUTO_REMINDERID);

    PKG_RESCUE_141516.CREATE_REMINDER(
        P_RECORDID       => 'health-demo-manual-141516',
        P_CATID          => 'cat-demo-141516',
        P_REMINDERTYPE   => 'DEWORM',
        P_RECEIVERUSERID => 'user-demo-handler-141516',
        P_REMINDERTIME   => SYSDATE + 60,
        O_REMINDERID     => V_MANUAL_REMINDERID
    );

    DBMS_OUTPUT.PUT_LINE('Manual reminder: ' || V_MANUAL_REMINDERID);

    PKG_RESCUE_141516.MARK_REMINDER_SENT(V_MANUAL_REMINDERID);
    PKG_RESCUE_141516.COMPLETE_REMINDER(V_MANUAL_REMINDERID);
END;
/

SELECT
    REMINDERID,
    CATID,
    REMINDERTYPE,
    RECEIVERUSERID,
    REMINDERTIME,
    SENDSTATUS
FROM MED_REMINDERS
WHERE RECORDID IN ('health-demo-auto-141516', 'health-demo-manual-141516')
ORDER BY REMINDERTIME DESC;

PROMPT ===== Requirement 15: emergency reports =====;

DECLARE
    V_REPORTID EMERGENCY_REPORTS.REPORTID%TYPE;
BEGIN
    PKG_RESCUE_141516.SUBMIT_EMERGENCY_REPORT(
        P_REPORTERUSERID => 'user-demo-reporter-141516',
        P_AREAID         => 'area-demo-141516',
        P_ANIMALTYPE     => 'CAT',
        P_PHOTOURL       => 'https://example.com/demo-cat-injured.jpg',
        P_LONGITUDE      => 120.12345678,
        P_LATITUDE       => 30.12345678,
        P_URGENCYLEVEL   => 'HIGH',
        O_REPORTID       => V_REPORTID
    );

    DBMS_OUTPUT.PUT_LINE('Created emergency report: ' || V_REPORTID);

    PKG_RESCUE_141516.ASSIGN_EMERGENCY_REPORT(
        P_REPORTID      => V_REPORTID,
        P_HANDLERUSERID => 'user-demo-handler-141516'
    );

    PKG_RESCUE_141516.UPDATE_EMERGENCY_STATUS(
        P_REPORTID      => V_REPORTID,
        P_PROCESSSTATUS => 'RESOLVED',
        P_PROCESSRESULT => '志愿者已到场，猫咪已送至合作医院处理'
    );
END;
/

SELECT
    REPORTID,
    REPORTERUSERID,
    AREAID,
    ANIMALTYPE,
    URGENCYLEVEL,
    PROCESSSTATUS,
    HANDLERUSERID,
    PROCESSRESULT
FROM EMERGENCY_REPORTS
WHERE REPORTERUSERID = 'user-demo-reporter-141516'
ORDER BY REPORTTIME DESC;

PROMPT ===== Requirement 16: missing alerts =====;

DECLARE
    V_ALERTID CAT_MISSINGALERTS.ALERTID%TYPE;
BEGIN
    PKG_RESCUE_141516.CREATE_MISSING_ALERT(
        P_CATID            => 'cat-demo-141516',
        P_LASTSIGHTINGID   => 'sighting-demo-141516',
        P_LASTSIGHTINGTIME => SYSDATE - 8,
        P_THRESHOLDDAYS    => 7,
        P_HANDLERUSERID    => 'user-demo-handler-141516',
        P_REMARK           => 'No sightings for 7 days, alert created',
        O_ALERTID          => V_ALERTID
    );

    DBMS_OUTPUT.PUT_LINE('Created missing alert: ' || V_ALERTID);

    PKG_RESCUE_141516.UPDATE_MISSING_STATUS(
        P_ALERTID       => V_ALERTID,
        P_ALERTSTATUS   => 'FOUND',
        P_HANDLERUSERID => 'user-demo-handler-141516',
        P_REMARK        => 'Cat found again near the Library East Gate'
    );
END;
/

SELECT
    ALERTID,
    CATID,
    LASTSIGHTINGID,
    LASTSIGHTINGTIME,
    THRESHOLDDAYS,
    ALERTSTATUS,
    HANDLERUSERID,
    CLOSETIME,
    REMARK
FROM CAT_MISSINGALERTS
WHERE CATID = 'cat-demo-141516'
ORDER BY ALERTTIME DESC;

PROMPT ===== Acceptance test complete =====;
