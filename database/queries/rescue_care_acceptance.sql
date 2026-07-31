SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;

PROMPT ===== Rescue care database acceptance =====;

-- 该脚本验证数据库编程层本身，不模拟 HTTP 请求。
-- 验收结束会 ROLLBACK，重复执行不会持续写入演示数据。

CREATE OR REPLACE PROCEDURE assert_condition(
    p_condition IN BOOLEAN,
    p_message IN VARCHAR2
) AS
BEGIN
    IF NOT p_condition THEN
        RAISE_APPLICATION_ERROR(-20999, p_message);
    END IF;
END;
/

DECLARE
    v_count NUMBER;
    v_reminder_id MED_REMINDERS.REMINDERID%TYPE;
    v_report_id EMERGENCY_REPORTS.REPORTID%TYPE;
    v_sighting_id CAT_SIGHTINGS.SIGHTINGID%TYPE;
    v_alert_id CAT_MISSINGALERTS.ALERTID%TYPE;
BEGIN
    SAVEPOINT rescue_care_acceptance_start;

    INSERT INTO SYS_ROLES (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
    VALUES ('role-rescue-demo', 'Rescue Demo Role', 'Demo role for rescue care acceptance', 'RESCUE');

    INSERT INTO SYS_USERS (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
    VALUES ('user-rescue-reporter', 'role-rescue-demo', 'rescue_reporter_demo', 'demo-password-hash', 'Demo Reporter', 'VERIFIED', 'ACTIVE');

    INSERT INTO SYS_USERS (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
    VALUES ('user-rescue-handler', 'role-rescue-demo', 'rescue_handler_demo', 'demo-password-hash', 'Demo Handler', 'VERIFIED', 'ACTIVE');

    INSERT INTO MAP_CAMPUSAREAS (AREAID, AREANAME, CAMPUSNAME, AREATYPE, RISKLEVEL)
    VALUES ('area-rescue-demo', 'Library East Gate', 'Main Campus', 'GATE', 'MEDIUM');

    INSERT INTO CAT_CATS (CATID, CATNAME, GENDER, COLORPATTERN, MAINAREAID, LIFESTATUS, ARCHIVESTATUS)
    VALUES ('cat-rescue-demo', 'Rescue Demo Cat', 'UNKNOWN', 'Orange white', 'area-rescue-demo', 'ACTIVE', 'NORMAL');

    INSERT INTO MED_HEALTHRECORDS (
        RECORDID,
        CATID,
        RECORDTYPE,
        HOSPITALNAME,
        DIAGNOSIS,
        RECORDDATE,
        NEXTDUEDATE
    ) VALUES (
        'health-rescue-vaccination',
        'cat-rescue-demo',
        'VACCINATION',
        'Demo Animal Hospital',
        'Routine vaccination',
        SYSDATE,
        SYSDATE + 30
    );

    SELECT COUNT(*)
    INTO v_count
    FROM MED_REMINDERS
    WHERE RECORDID = 'health-rescue-vaccination'
      AND REMINDERTYPE = 'VACCINATION'
      AND SENDSTATUS = 'PENDING';

    assert_condition(v_count = 1, 'Health trigger should create one VACCINATION reminder');

    PKG_RESCUE_CARE.CREATE_REMINDER(
        P_RECORDID       => NULL,
        P_CATID          => 'cat-rescue-demo',
        P_REMINDERTYPE   => 'DEWORMING',
        P_RECEIVERUSERID => 'user-rescue-handler',
        P_REMINDERTIME   => SYSDATE + 60,
        O_REMINDERID     => v_reminder_id
    );

    assert_condition(v_reminder_id IS NOT NULL, 'CREATE_REMINDER should return reminder id');

    PKG_RESCUE_CARE.MARK_REMINDER_SENT(v_reminder_id);
    PKG_RESCUE_CARE.COMPLETE_REMINDER(v_reminder_id);

    SELECT COUNT(*)
    INTO v_count
    FROM VW_MED_REMINDERS
    WHERE REMINDERID = v_reminder_id
      AND REMINDERTYPE = 'DEWORMING'
      AND SENDSTATUS = 'COMPLETED';

    assert_condition(v_count = 1, 'Reminder should be completed and visible in VW_MED_REMINDERS');

    PKG_RESCUE_CARE.SUBMIT_EMERGENCY_REPORT(
        P_REPORTERUSERID => 'user-rescue-reporter',
        P_AREAID         => 'area-rescue-demo',
        P_ANIMALTYPE     => 'CAT',
        P_PHOTOURL       => 'https://example.com/emergency-cat.jpg',
        P_LONGITUDE      => 121.215,
        P_LATITUDE       => 31.289,
        P_URGENCYLEVEL   => 'HIGH',
        O_REPORTID       => v_report_id
    );

    assert_condition(v_report_id IS NOT NULL, 'SUBMIT_EMERGENCY_REPORT should return report id');

    PKG_RESCUE_CARE.ASSIGN_EMERGENCY_REPORT(v_report_id, 'user-rescue-handler');
    PKG_RESCUE_CARE.UPDATE_EMERGENCY_STATUS(v_report_id, 'RESOLVED', 'Volunteer sent the cat to the clinic.');

    SELECT COUNT(*)
    INTO v_count
    FROM VW_EMERGENCY_REPORTS
    WHERE REPORTID = v_report_id
      AND PROCESSSTATUS = 'RESOLVED'
      AND HANDLERUSERID = 'user-rescue-handler';

    assert_condition(v_count = 1, 'Emergency report should be resolved and visible in VW_EMERGENCY_REPORTS');

    PKG_RESCUE_CARE.CREATE_SIGHTING(
        P_CATID        => 'cat-rescue-demo',
        P_USERID       => 'user-rescue-reporter',
        P_AREAID       => 'area-rescue-demo',
        P_LONGITUDE    => 121.215,
        P_LATITUDE     => 31.289,
        P_PHOTOURL     => 'https://example.com/sighting-cat.jpg',
        P_SIGHTINGTIME => SYSDATE - 8,
        P_REMARK       => 'Last seen near Library East Gate',
        O_SIGHTINGID   => v_sighting_id
    );

    assert_condition(v_sighting_id IS NOT NULL, 'CREATE_SIGHTING should return sighting id');

    PKG_RESCUE_CARE.CREATE_MISSING_ALERT(
        P_CATID            => 'cat-rescue-demo',
        P_LASTSIGHTINGID   => v_sighting_id,
        P_LASTSIGHTINGTIME => SYSDATE - 8,
        P_THRESHOLDDAYS    => 7,
        P_HANDLERUSERID    => 'user-rescue-handler',
        P_REMARK           => 'Demo missing alert',
        O_ALERTID          => v_alert_id
    );

    assert_condition(v_alert_id IS NOT NULL, 'CREATE_MISSING_ALERT should return alert id');

    PKG_RESCUE_CARE.UPDATE_MISSING_STATUS(v_alert_id, 'FOUND', 'user-rescue-handler', 'Cat found near Library East Gate');

    SELECT COUNT(*)
    INTO v_count
    FROM VW_MISSING_ALERTS
    WHERE ALERTID = v_alert_id
      AND ALERTSTATUS = 'FOUND'
      AND HANDLERUSERID = 'user-rescue-handler'
      AND CLOSETIME IS NOT NULL;

    assert_condition(v_count = 1, 'Missing alert should be found and visible in VW_MISSING_ALERTS');

    DBMS_OUTPUT.PUT_LINE('Requirement 14 passed: reminders create, update, and view query.');
    DBMS_OUTPUT.PUT_LINE('Requirement 15 passed: emergency report submit, assign, update, and view query.');
    DBMS_OUTPUT.PUT_LINE('Requirement 16 passed: sighting, missing alert create, update, and view query.');

    ROLLBACK TO rescue_care_acceptance_start;
END;
/

DROP PROCEDURE assert_condition;

PROMPT Rescue care database acceptance completed.
