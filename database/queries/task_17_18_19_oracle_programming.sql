SET DEFINE OFF;
PROMPT ===== Task 17/18/19 Oracle programming layer =====;

-- 任务 17/18/19 的目标：
-- 1) 领养申请：提交申请、审核申请、记录回访；
-- 2) 领养回访：查看和记录回访结论；
-- 3) 志愿者管理：注册志愿者、排班、打卡、记录积分。

-- 先定义一个小工具过程：如果索引不存在，就创建它，避免脚本重复执行时报错。
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

-- 任务 17/18/19 需要的辅助索引，方便按状态和时间查询。
BEGIN
    create_index_if_not_exists('CREATE INDEX IDX_ADOPT_APPS_STATUS_TIME ON ADOPT_APPLICATIONS(CURRENTSTATUS, APPLYTIME)');
    create_index_if_not_exists('CREATE INDEX IDX_ADOPT_VISITS_TIME_STATUS ON ADOPT_VISITS(VISITTIME, PASSFLAG)');
    create_index_if_not_exists('CREATE INDEX IDX_VOL_SHIFT_STATUS_TIME ON VOL_SHIFTS(SHIFTSTATUS, PLANSTARTTIME)');
END;
/

-- 任务 17：领养申请视图，快速查看“待审核”申请。
CREATE OR REPLACE VIEW VW_PENDING_ADOPTION_APPS AS
SELECT
    a.APPLICATIONID,
    a.CATID,
    c.CATNAME,
    a.APPLICANTUSERID,
    u.USERNAME AS APPLICANTNAME,
    a.APPLYTIME,
    a.CURRENTSTATUS,
    a.REVIEWERUSERID,
    a.AGREEMENTNO,
    a.CONFIRMTIME
FROM ADOPT_APPLICATIONS a
LEFT JOIN CAT_CATS c ON c.CATID = a.CATID
LEFT JOIN SYS_USERS u ON u.USERID = a.APPLICANTUSERID
WHERE a.CURRENTSTATUS = 'PENDING';
/

-- 任务 18：回访视图，集中查看回访记录和结果。
CREATE OR REPLACE VIEW VW_ADOPTION_VISIT_SUMMARY AS
SELECT
    v.VISITID,
    v.APPLICATIONID,
    a.CATID,
    v.VISITTYPE,
    v.VISITTIME,
    v.VISITORUSERID,
    v.CONCLUSION,
    v.PASSFLAG,
    a.CURRENTSTATUS
FROM ADOPT_VISITS v
LEFT JOIN ADOPT_APPLICATIONS a ON a.APPLICATIONID = v.APPLICATIONID;
/

-- 任务 19：志愿者活动视图，查看志愿者状态、排班和服务情况。
CREATE OR REPLACE VIEW VW_VOLUNTEER_ACTIVITY AS
SELECT
    vol.VOLUNTEERID,
    vol.USERID,
    u.USERNAME,
    vol.ACTIVESTATUS,
    vol.CREDITLEVEL,
    vol.SERVICESCORE,
    s.SHIFTID,
    s.SHIFTSTATUS,
    s.PLANSTARTTIME,
    s.PLANENDTIME
FROM VOL_VOLUNTEERS vol
LEFT JOIN SYS_USERS u ON u.USERID = vol.USERID
LEFT JOIN VOL_SHIFTS s ON s.VOLUNTEERID = vol.VOLUNTEERID;
/

-- 任务 17：领养流程包，负责提交申请、审核申请、记录回访。
CREATE OR REPLACE PACKAGE PKG_ADOPTION_WORKFLOW AS
    PROCEDURE submit_application(p_cat_id IN VARCHAR2, p_applicant_user_id IN VARCHAR2, p_status IN VARCHAR2 DEFAULT 'PENDING');
    PROCEDURE review_application(p_application_id IN VARCHAR2, p_reviewer_user_id IN VARCHAR2, p_status IN VARCHAR2, p_agreement_no IN VARCHAR2 DEFAULT NULL, p_confirm_time IN DATE DEFAULT NULL);
    PROCEDURE create_visit(p_application_id IN VARCHAR2, p_visit_type IN VARCHAR2, p_visit_time IN DATE DEFAULT SYSDATE, p_visitor_user_id IN VARCHAR2, p_conclusion IN VARCHAR2 DEFAULT NULL, p_passflag IN NUMBER DEFAULT 0);
END PKG_ADOPTION_WORKFLOW;
/

CREATE OR REPLACE PACKAGE BODY PKG_ADOPTION_WORKFLOW AS
    -- 提交领养申请，默认状态为待审核。
    PROCEDURE submit_application(p_cat_id IN VARCHAR2, p_applicant_user_id IN VARCHAR2, p_status IN VARCHAR2 DEFAULT 'PENDING') IS
    BEGIN
        INSERT INTO ADOPT_APPLICATIONS (APPLICATIONID, CATID, APPLICANTUSERID, APPLYTIME, CURRENTSTATUS, REVIEWERUSERID, AGREEMENTNO, CONFIRMTIME)
        VALUES ('APP-' || DBMS_RANDOM.STRING('X', 8), p_cat_id, p_applicant_user_id, SYSDATE, p_status, NULL, NULL, NULL);
    END submit_application;

    -- 审核领养申请，更新申请状态、审核人和协议编号，若申请人处于黑名单则强制拒绝。
    PROCEDURE review_application(p_application_id IN VARCHAR2, p_reviewer_user_id IN VARCHAR2, p_status IN VARCHAR2, p_agreement_no IN VARCHAR2 DEFAULT NULL, p_confirm_time IN DATE DEFAULT NULL) IS
        v_applicant_user_id VARCHAR2(36);
        v_blacklisted NUMBER := 0;
    BEGIN
        -- 先查申请人并判断是否在黑名单中
        SELECT APPLICANTUSERID INTO v_applicant_user_id FROM ADOPT_APPLICATIONS WHERE APPLICATIONID = p_application_id;
        SELECT COUNT(1) INTO v_blacklisted FROM USER_BLACKLIST ub WHERE ub.USERID = v_applicant_user_id AND ub.BLACKLISTSTATUS = 'ACTIVE';

        IF v_blacklisted > 0 THEN
            UPDATE ADOPT_APPLICATIONS
            SET CURRENTSTATUS = 'REJECTED',
                REVIEWERUSERID = p_reviewer_user_id,
                AGREEMENTNO = NULL,
                CONFIRMTIME = NVL(p_confirm_time, SYSDATE)
            WHERE APPLICATIONID = p_application_id;
        ELSE
            UPDATE ADOPT_APPLICATIONS
            SET CURRENTSTATUS = p_status,
                REVIEWERUSERID = p_reviewer_user_id,
                AGREEMENTNO = p_agreement_no,
                CONFIRMTIME = NVL(p_confirm_time, SYSDATE)
            WHERE APPLICATIONID = p_application_id;
        END IF;
    END review_application;

    -- 为某个申请增加一次回访记录。
    PROCEDURE create_visit(p_application_id IN VARCHAR2, p_visit_type IN VARCHAR2, p_visit_time IN DATE DEFAULT SYSDATE, p_visitor_user_id IN VARCHAR2, p_conclusion IN VARCHAR2 DEFAULT NULL, p_passflag IN NUMBER DEFAULT 0) IS
    BEGIN
        INSERT INTO ADOPT_VISITS (VISITID, APPLICATIONID, VISITTYPE, VISITTIME, VISITORUSERID, CONCLUSION, PASSFLAG)
        VALUES ('VIS-' || DBMS_RANDOM.STRING('X', 8), p_application_id, p_visit_type, p_visit_time, p_visitor_user_id, p_conclusion, p_passflag);
    END create_visit;
END PKG_ADOPTION_WORKFLOW;
/

-- 任务 18/19：志愿者管理包，负责注册志愿者、排班、打卡和积分记录。
CREATE OR REPLACE PACKAGE PKG_VOLUNTEER_MGMT AS
    PROCEDURE register_volunteer(p_user_id IN VARCHAR2, p_join_date IN DATE DEFAULT SYSDATE, p_service_score IN NUMBER DEFAULT 0, p_credit_level IN VARCHAR2 DEFAULT 'L1', p_active_status IN VARCHAR2 DEFAULT 'ACTIVE', p_graduation_year IN VARCHAR2 DEFAULT NULL);
    PROCEDURE create_shift(p_volunteer_id IN VARCHAR2, p_point_id IN VARCHAR2, p_backup_volunteer_id IN VARCHAR2 DEFAULT NULL, p_plan_start_time IN DATE, p_plan_end_time IN DATE, p_shift_status IN VARCHAR2 DEFAULT 'PLANNED');
    PROCEDURE check_in_shift(p_shift_id IN VARCHAR2, p_checkin_time IN DATE DEFAULT SYSDATE, p_longitude IN NUMBER DEFAULT NULL, p_latitude IN NUMBER DEFAULT NULL, p_photo_url IN VARCHAR2 DEFAULT NULL, p_distance_meters IN NUMBER DEFAULT NULL, p_checkin_status IN VARCHAR2 DEFAULT 'CHECKED_IN');
    PROCEDURE add_credit_log(p_volunteer_id IN VARCHAR2, p_source_type IN VARCHAR2, p_source_id IN VARCHAR2, p_score_change IN NUMBER, p_credit_level_after IN VARCHAR2, p_create_time IN DATE DEFAULT SYSDATE, p_remark IN VARCHAR2 DEFAULT NULL);
END PKG_VOLUNTEER_MGMT;
/

CREATE OR REPLACE PACKAGE BODY PKG_VOLUNTEER_MGMT AS
    -- 注册志愿者，记录基本信息和等级。
    PROCEDURE register_volunteer(p_user_id IN VARCHAR2, p_join_date IN DATE DEFAULT SYSDATE, p_service_score IN NUMBER DEFAULT 0, p_credit_level IN VARCHAR2 DEFAULT 'L1', p_active_status IN VARCHAR2 DEFAULT 'ACTIVE', p_graduation_year IN VARCHAR2 DEFAULT NULL) IS
    BEGIN
        INSERT INTO VOL_VOLUNTEERS (VOLUNTEERID, USERID, JOINDATE, SERVICESCORE, CREDITLEVEL, ACTIVESTATUS, GRADUATIONYEAR)
        VALUES ('VOL-' || DBMS_RANDOM.STRING('X', 8), p_user_id, p_join_date, p_service_score, p_credit_level, p_active_status, p_graduation_year);
    END register_volunteer;

    -- 创建志愿者排班记录。
    PROCEDURE create_shift(p_volunteer_id IN VARCHAR2, p_point_id IN VARCHAR2, p_backup_volunteer_id IN VARCHAR2 DEFAULT NULL, p_plan_start_time IN DATE, p_plan_end_time IN DATE, p_shift_status IN VARCHAR2 DEFAULT 'PLANNED') IS
    BEGIN
        INSERT INTO VOL_SHIFTS (SHIFTID, VOLUNTEERID, POINTID, BACKUPVOLUNTEERID, PLANSTARTTIME, PLANENDTIME, SHIFTSTATUS)
        VALUES ('SHIFT-' || DBMS_RANDOM.STRING('X', 8), p_volunteer_id, p_point_id, p_backup_volunteer_id, p_plan_start_time, p_plan_end_time, p_shift_status);
    END create_shift;

    -- 志愿者打卡，记录签到位置和状态。若为正常签到，则自动产生积分记录并更新志愿者总积分。
    PROCEDURE check_in_shift(p_shift_id IN VARCHAR2, p_checkin_time IN DATE DEFAULT SYSDATE, p_longitude IN NUMBER DEFAULT NULL, p_latitude IN NUMBER DEFAULT NULL, p_photo_url IN VARCHAR2 DEFAULT NULL, p_distance_meters IN NUMBER DEFAULT NULL, p_checkin_status IN VARCHAR2 DEFAULT 'CHECKED_IN') IS
        v_volunteer_id VARCHAR2(36);
        v_score_change NUMBER := 0;
        v_credit_level VARCHAR2(20);
    BEGIN
        INSERT INTO VOL_CHECKINS (CHECKINID, SHIFTID, CHECKINTIME, LONGITUDE, LATITUDE, PHOTOURL, DISTANCEMETERS, CHECKINSTATUS)
        VALUES ('CHK-' || DBMS_RANDOM.STRING('X', 8), p_shift_id, p_checkin_time, p_longitude, p_latitude, p_photo_url, p_distance_meters, p_checkin_status);

        -- 如果为正常签到，则给予 1 分（可按需调整），并更新志愿者总积分。
        IF p_checkin_status = 'CHECKED_IN' THEN
            SELECT VOLUNTEERID INTO v_volunteer_id FROM VOL_SHIFTS WHERE SHIFTID = p_shift_id;
            SELECT NVL(CREDITLEVEL, 'L1') INTO v_credit_level FROM VOL_VOLUNTEERS WHERE VOLUNTEERID = v_volunteer_id;
            v_score_change := 1;

            -- 插入积分日志（复用包内 add_credit_log）
            add_credit_log(v_volunteer_id, 'CHECKIN', p_shift_id, v_score_change, v_credit_level, p_checkin_time, 'Auto credit for check-in');
        END IF;
    END check_in_shift;

    -- 为志愿者增加积分变更记录，并同步更新志愿者的累计服务分数。
    PROCEDURE add_credit_log(p_volunteer_id IN VARCHAR2, p_source_type IN VARCHAR2, p_source_id IN VARCHAR2, p_score_change IN NUMBER, p_credit_level_after IN VARCHAR2, p_create_time IN DATE DEFAULT SYSDATE, p_remark IN VARCHAR2 DEFAULT NULL) IS
    BEGIN
        INSERT INTO VOL_CREDITLOGS (CREDITLOGID, VOLUNTEERID, SOURCETYPE, SOURCEID, SCORECHANGE, CREDITLEVELAFTER, CREATETIME, REMARK)
        VALUES ('CRED-' || DBMS_RANDOM.STRING('X', 8), p_volunteer_id, p_source_type, p_source_id, p_score_change, p_credit_level_after, p_create_time, p_remark);

        -- 更新累计积分（若不存在则忽略）
        UPDATE VOL_VOLUNTEERS
        SET SERVICESCORE = NVL(SERVICESCORE, 0) + NVL(p_score_change, 0)
        WHERE VOLUNTEERID = p_volunteer_id;
    END add_credit_log;
END PKG_VOLUNTEER_MGMT;
/

PROMPT Task 17/18/19 Oracle programming layer completed. Please execute this script after create_tables.sql.
