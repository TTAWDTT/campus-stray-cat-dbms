SET DEFINE OFF;

-- 统一的索引创建入口，避免脚本重复执行时因为索引已存在而失败。
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

-- 任务 17-19 的公共辅助索引，主要覆盖状态和时间维度的查询。
BEGIN
    create_index_if_not_exists('CREATE INDEX IDX_ADOPT_APPS_STATUS_TIME ON ADOPT_APPLICATIONS(CURRENTSTATUS, APPLYTIME)');
    create_index_if_not_exists('CREATE INDEX IDX_ADOPT_VISITS_TIME_STATUS ON ADOPT_VISITS(VISITTIME, PASSFLAG)');
    create_index_if_not_exists('CREATE INDEX IDX_VOL_SHIFT_STATUS_TIME ON VOL_SHIFTS(SHIFTSTATUS, PLANSTARTTIME)');
END;
/

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

-- 任务 18：领养回访汇总视图，集中返回回访记录和申请状态。
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

-- 任务 19：志愿者活动视图，聚合志愿者、排班和服务状态。
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

CREATE OR REPLACE PACKAGE PKG_ADOPTION_WORKFLOW AS
    PROCEDURE submit_application(p_cat_id IN VARCHAR2, p_applicant_user_id IN VARCHAR2, p_status IN VARCHAR2 DEFAULT 'PENDING');
    PROCEDURE review_application(p_application_id IN VARCHAR2, p_reviewer_user_id IN VARCHAR2, p_status IN VARCHAR2, p_agreement_no IN VARCHAR2 DEFAULT NULL, p_confirm_time IN DATE DEFAULT NULL);
    PROCEDURE create_visit(p_application_id IN VARCHAR2, p_visit_type IN VARCHAR2, p_visitor_user_id IN VARCHAR2, p_visit_time IN DATE DEFAULT SYSDATE, p_conclusion IN VARCHAR2 DEFAULT NULL, p_passflag IN NUMBER DEFAULT 0);
END PKG_ADOPTION_WORKFLOW;
/

CREATE OR REPLACE PACKAGE BODY PKG_ADOPTION_WORKFLOW AS
    PROCEDURE submit_application(p_cat_id IN VARCHAR2, p_applicant_user_id IN VARCHAR2, p_status IN VARCHAR2 DEFAULT 'PENDING') IS
    BEGIN
        INSERT INTO ADOPT_APPLICATIONS (APPLICATIONID, CATID, APPLICANTUSERID, APPLYTIME, CURRENTSTATUS, REVIEWERUSERID, AGREEMENTNO, CONFIRMTIME)
        VALUES ('APP-' || DBMS_RANDOM.STRING('X', 8), p_cat_id, p_applicant_user_id, SYSDATE, 'PENDING', NULL, NULL, NULL);
    END submit_application;

    PROCEDURE review_application(p_application_id IN VARCHAR2, p_reviewer_user_id IN VARCHAR2, p_status IN VARCHAR2, p_agreement_no IN VARCHAR2 DEFAULT NULL, p_confirm_time IN DATE DEFAULT NULL) IS
        v_applicant_user_id VARCHAR2(36);
        v_current_status VARCHAR2(30);
        v_blacklisted NUMBER := 0;
    BEGIN
        IF p_status IS NULL OR p_status NOT IN ('APPROVED', 'REJECTED') THEN
            raise_application_error(-20031, '审核状态只能是 APPROVED 或 REJECTED');
        END IF;

        -- 检查申请人是否在黑名单中（状态为 ACTIVE 表示仍在黑名单）
        SELECT APPLICANTUSERID, CURRENTSTATUS
        INTO v_applicant_user_id, v_current_status
        FROM ADOPT_APPLICATIONS
        WHERE APPLICATIONID = p_application_id;

        IF v_current_status <> 'PENDING' THEN
            raise_application_error(-20032, '只有 PENDING 状态的申请可以审核');
        END IF;

        SELECT COUNT(1) INTO v_blacklisted FROM USER_BLACKLIST ub WHERE ub.USERID = v_applicant_user_id AND UPPER(ub.BLACKLISTSTATUS) = 'ACTIVE';

        IF v_blacklisted > 0 THEN
            -- 若在黑名单，强制设置为 REJECTED 并记录审核人/时间，避免通过
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

    PROCEDURE create_visit(p_application_id IN VARCHAR2, p_visit_type IN VARCHAR2, p_visitor_user_id IN VARCHAR2, p_visit_time IN DATE DEFAULT SYSDATE, p_conclusion IN VARCHAR2 DEFAULT NULL, p_passflag IN NUMBER DEFAULT 0) IS
        v_current_status VARCHAR2(30);
    BEGIN
        IF p_passflag IS NULL OR p_passflag NOT IN (0, 1) THEN
            raise_application_error(-20033, '回访通过标记只能是 0 或 1');
        END IF;

        SELECT CURRENTSTATUS INTO v_current_status
        FROM ADOPT_APPLICATIONS
        WHERE APPLICATIONID = p_application_id;

        IF v_current_status <> 'APPROVED' THEN
            raise_application_error(-20034, '只有已通过的领养申请可以回访');
        END IF;

        INSERT INTO ADOPT_VISITS (VISITID, APPLICATIONID, VISITTYPE, VISITTIME, VISITORUSERID, CONCLUSION, PASSFLAG)
        VALUES ('VIS-' || DBMS_RANDOM.STRING('X', 8), p_application_id, p_visit_type, p_visit_time, p_visitor_user_id, p_conclusion, p_passflag);
    END create_visit;
END PKG_ADOPTION_WORKFLOW;
/

CREATE OR REPLACE PACKAGE PKG_VOLUNTEER_MGMT AS
    PROCEDURE register_volunteer(p_user_id IN VARCHAR2, p_join_date IN DATE DEFAULT SYSDATE, p_service_score IN NUMBER DEFAULT 0, p_credit_level IN VARCHAR2 DEFAULT 'L1', p_active_status IN VARCHAR2 DEFAULT 'ACTIVE', p_graduation_year IN VARCHAR2 DEFAULT NULL);
    PROCEDURE create_shift(p_volunteer_id IN VARCHAR2, p_point_id IN VARCHAR2, p_plan_start_time IN DATE, p_plan_end_time IN DATE, p_backup_volunteer_id IN VARCHAR2 DEFAULT NULL, p_shift_status IN VARCHAR2 DEFAULT 'PLANNED');
    PROCEDURE check_in_shift(p_shift_id IN VARCHAR2, p_operator_user_id IN VARCHAR2, p_checkin_time IN DATE DEFAULT SYSDATE, p_longitude IN NUMBER DEFAULT NULL, p_latitude IN NUMBER DEFAULT NULL, p_photo_url IN VARCHAR2 DEFAULT NULL, p_distance_meters IN NUMBER DEFAULT NULL, p_checkin_status IN VARCHAR2 DEFAULT 'CHECKED_IN');
    PROCEDURE add_credit_log(p_volunteer_id IN VARCHAR2, p_source_type IN VARCHAR2, p_source_id IN VARCHAR2, p_score_change IN NUMBER, p_credit_level_after IN VARCHAR2, p_create_time IN DATE DEFAULT SYSDATE, p_remark IN VARCHAR2 DEFAULT NULL);
END PKG_VOLUNTEER_MGMT;
/

CREATE OR REPLACE PACKAGE BODY PKG_VOLUNTEER_MGMT AS
    PROCEDURE register_volunteer(p_user_id IN VARCHAR2, p_join_date IN DATE DEFAULT SYSDATE, p_service_score IN NUMBER DEFAULT 0, p_credit_level IN VARCHAR2 DEFAULT 'L1', p_active_status IN VARCHAR2 DEFAULT 'ACTIVE', p_graduation_year IN VARCHAR2 DEFAULT NULL) IS
        v_existing NUMBER;
    BEGIN
        SELECT COUNT(1) INTO v_existing FROM VOL_VOLUNTEERS WHERE USERID = p_user_id;
        IF v_existing > 0 THEN
            raise_application_error(-20043, '该用户已经注册为志愿者');
        END IF;

        BEGIN
            INSERT INTO VOL_VOLUNTEERS (VOLUNTEERID, USERID, JOINDATE, SERVICESCORE, CREDITLEVEL, ACTIVESTATUS, GRADUATIONYEAR)
            VALUES ('VOL-' || DBMS_RANDOM.STRING('X', 8), p_user_id, p_join_date, p_service_score, p_credit_level, p_active_status, p_graduation_year);
        EXCEPTION
            WHEN DUP_VAL_ON_INDEX THEN
                raise_application_error(-20043, '该用户已经注册为志愿者');
        END;
    END register_volunteer;

    PROCEDURE create_shift(p_volunteer_id IN VARCHAR2, p_point_id IN VARCHAR2, p_plan_start_time IN DATE, p_plan_end_time IN DATE, p_backup_volunteer_id IN VARCHAR2 DEFAULT NULL, p_shift_status IN VARCHAR2 DEFAULT 'PLANNED') IS
        v_count NUMBER;
    BEGIN
        IF p_plan_start_time IS NULL OR p_plan_end_time IS NULL OR p_plan_end_time <= p_plan_start_time THEN
            raise_application_error(-20044, '排班时间不能为空且结束时间必须晚于开始时间');
        END IF;

        SELECT COUNT(1) INTO v_count
        FROM VOL_VOLUNTEERS
        WHERE VOLUNTEERID = p_volunteer_id AND ACTIVESTATUS = 'ACTIVE';
        IF v_count = 0 THEN
            raise_application_error(-20045, '志愿者不存在或已停用');
        END IF;

        SELECT COUNT(1) INTO v_count FROM MAP_SERVICEPOINTS WHERE POINTID = p_point_id;
        IF v_count = 0 THEN
            raise_application_error(-20046, '投喂点不存在');
        END IF;

        IF p_backup_volunteer_id IS NOT NULL THEN
            SELECT COUNT(1) INTO v_count
            FROM VOL_VOLUNTEERS
            WHERE VOLUNTEERID = p_backup_volunteer_id AND ACTIVESTATUS = 'ACTIVE';
            IF v_count = 0 OR p_backup_volunteer_id = p_volunteer_id THEN
                raise_application_error(-20047, '备用志愿者不存在、已停用或不能与负责人相同');
            END IF;
        END IF;

        INSERT INTO VOL_SHIFTS (SHIFTID, VOLUNTEERID, POINTID, BACKUPVOLUNTEERID, PLANSTARTTIME, PLANENDTIME, SHIFTSTATUS)
        VALUES ('SHIFT-' || DBMS_RANDOM.STRING('X', 8), p_volunteer_id, p_point_id, p_backup_volunteer_id, p_plan_start_time, p_plan_end_time, p_shift_status);
    END create_shift;

    PROCEDURE check_in_shift(p_shift_id IN VARCHAR2, p_operator_user_id IN VARCHAR2, p_checkin_time IN DATE DEFAULT SYSDATE, p_longitude IN NUMBER DEFAULT NULL, p_latitude IN NUMBER DEFAULT NULL, p_photo_url IN VARCHAR2 DEFAULT NULL, p_distance_meters IN NUMBER DEFAULT NULL, p_checkin_status IN VARCHAR2 DEFAULT 'CHECKED_IN') IS
        v_volunteer_id VARCHAR2(36);
        v_credit_level VARCHAR2(20);
        v_checkin_id VARCHAR2(36);
        v_existing NUMBER := 0;
    BEGIN
        BEGIN
            SELECT v.VOLUNTEERID, v.CREDITLEVEL
            INTO v_volunteer_id, v_credit_level
            FROM VOL_SHIFTS s
            INNER JOIN VOL_VOLUNTEERS v ON v.VOLUNTEERID = s.VOLUNTEERID
            WHERE s.SHIFTID = p_shift_id
              AND v.USERID = p_operator_user_id
              AND v.ACTIVESTATUS = 'ACTIVE';
        EXCEPTION
            WHEN NO_DATA_FOUND THEN
                raise_application_error(-20041, '只能为本人有效排班签到');
        END;

        SELECT COUNT(1) INTO v_existing
        FROM VOL_CHECKINS
        WHERE SHIFTID = p_shift_id
          AND UPPER(CHECKINSTATUS) IN ('CHECKED_IN', 'LATE');

        IF v_existing > 0 THEN
            raise_application_error(-20042, '该排班已经签到，不能重复签到');
        END IF;

        v_checkin_id := 'CHK-' || DBMS_RANDOM.STRING('X', 8);
        BEGIN
            INSERT INTO VOL_CHECKINS (CHECKINID, SHIFTID, CHECKINTIME, LONGITUDE, LATITUDE, PHOTOURL, DISTANCEMETERS, CHECKINSTATUS)
            VALUES (v_checkin_id, p_shift_id, p_checkin_time, p_longitude, p_latitude, p_photo_url, p_distance_meters, p_checkin_status);
        EXCEPTION
            WHEN DUP_VAL_ON_INDEX THEN
                raise_application_error(-20042, '该排班已经签到，不能重复签到');
        END;

        UPDATE VOL_SHIFTS
        SET SHIFTSTATUS = 'COMPLETED'
        WHERE SHIFTID = p_shift_id;

        IF p_checkin_status = 'CHECKED_IN' THEN
            UPDATE VOL_VOLUNTEERS
            SET SERVICESCORE = NVL(SERVICESCORE, 0) + 1
            WHERE VOLUNTEERID = v_volunteer_id;

            INSERT INTO VOL_CREDITLOGS (CREDITLOGID, VOLUNTEERID, SOURCETYPE, SOURCEID, SCORECHANGE, CREDITLEVELAFTER, CREATETIME, REMARK)
            VALUES ('CRED-' || DBMS_RANDOM.STRING('X', 8), v_volunteer_id, 'CHECKIN', v_checkin_id, 1, NVL(v_credit_level, 'L1'), p_checkin_time, '排班签到自动增加服务积分');
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
