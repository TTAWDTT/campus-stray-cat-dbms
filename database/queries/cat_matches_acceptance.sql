SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
PROMPT ===== Cat matches acceptance test =====;

DECLARE
    object_count NUMBER;
    valid_count NUMBER;
    ordered_ids VARCHAR2(200);

    PROCEDURE expect_check_violation(p_match_id VARCHAR2,
                                     p_candidate_id VARCHAR2,
                                     p_score NUMBER,
                                     p_rank NUMBER,
                                     p_status VARCHAR2,
                                     p_error_code NUMBER,
                                     p_message VARCHAR2) IS
    BEGIN
        INSERT INTO CAT_MATCHRECORDS (
            MATCHID, SOURCEPHOTOID, CANDIDATECATID,
            SIMILARITYSCORE, RANKNO, CONFIRMSTATUS
        ) VALUES (
            p_match_id, 'test-match-source-photo', p_candidate_id,
            p_score, p_rank, p_status
        );
        RAISE_APPLICATION_ERROR(-20031, p_message);
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE <> p_error_code THEN
                RAISE;
            END IF;
            DBMS_OUTPUT.PUT_LINE('PASS: ' || p_message);
    END;

    PROCEDURE expect_unique_violation(p_match_id VARCHAR2,
                                      p_candidate_id VARCHAR2,
                                      p_rank NUMBER,
                                      p_message VARCHAR2) IS
    BEGIN
        INSERT INTO CAT_MATCHRECORDS (
            MATCHID, SOURCEPHOTOID, CANDIDATECATID,
            SIMILARITYSCORE, RANKNO, CONFIRMSTATUS
        ) VALUES (
            p_match_id, 'test-match-source-photo', p_candidate_id,
            50, p_rank, 'PENDING'
        );
        RAISE_APPLICATION_ERROR(-20032, p_message);
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            DBMS_OUTPUT.PUT_LINE('PASS: ' || p_message);
    END;
BEGIN
    DELETE FROM CAT_MATCHRECORDS
    WHERE MATCHID LIKE 'test-match-%'
       OR SOURCEPHOTOID = 'test-match-source-photo';
    DELETE FROM CAT_PHOTOS
    WHERE PHOTOID LIKE 'test-match-%';
    DELETE FROM CAT_CATS
    WHERE CATID LIKE 'test-match-%';
    DELETE FROM SYS_USERS
    WHERE USERID = 'test-match-user';
    DELETE FROM SYS_ROLES
    WHERE ROLEID = 'test-match-role';
    COMMIT;

    SELECT COUNT(*) INTO object_count
    FROM USER_TABLES
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS';
    IF object_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20030, 'CAT_MATCHRECORDS table is missing.');
    END IF;

    SELECT COUNT(*) INTO object_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME IN (
          'CK_CAT_MATCH_SCORE',
          'CK_CAT_MATCH_RANK',
          'CK_CAT_MATCH_STATUS',
          'UQ_CAT_MATCH_SOURCE_CANDIDATE',
          'UQ_CAT_MATCH_SOURCE_RANK'
      );
    IF object_count <> 5 THEN
        RAISE_APPLICATION_ERROR(-20033, 'One or more CAT_MATCHRECORDS constraints are missing.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: all CAT_MATCHRECORDS constraints exist.');

    INSERT INTO SYS_ROLES (ROLEID, ROLENAME)
    VALUES ('test-match-role', 'Match Acceptance Role');

    INSERT INTO SYS_USERS (USERID, ROLEID, USERNAME, PASSWORDHASH, STATUS)
    VALUES ('test-match-user', 'test-match-role', 'match_acceptance_user', 'TEST_ONLY', 'ACTIVE');

    INSERT INTO CAT_CATS (CATID, CATNAME, MAINAREAID, LIFESTATUS, ARCHIVESTATUS)
    VALUES ('test-match-source-cat', 'Match Source Cat', NULL, 'ON_CAMPUS', 'PUBLISHED');
    INSERT INTO CAT_CATS (CATID, CATNAME, MAINAREAID, LIFESTATUS, ARCHIVESTATUS)
    VALUES ('test-match-candidate-1', 'Match Candidate One', NULL, 'ON_CAMPUS', 'PUBLISHED');
    INSERT INTO CAT_CATS (CATID, CATNAME, MAINAREAID, LIFESTATUS, ARCHIVESTATUS)
    VALUES ('test-match-candidate-2', 'Match Candidate Two', NULL, 'ON_CAMPUS', 'PUBLISHED');

    INSERT INTO CAT_PHOTOS (
        PHOTOID, CATID, PHOTOURL, UPLOADUSERID, UPLOADTIME, ISPRIMARY
    ) VALUES (
        'test-match-source-photo', 'test-match-source-cat',
        '/uploads/cats/test-match-source-cat/test-match-source-photo.jpg',
        'test-match-user', SYSDATE, 0
    );
    INSERT INTO CAT_PHOTOS (
        PHOTOID, CATID, PHOTOURL, UPLOADUSERID, UPLOADTIME, ISPRIMARY
    ) VALUES (
        'test-match-candidate-photo', 'test-match-candidate-1',
        '/uploads/cats/test-match-candidate-1/test-match-candidate-photo.jpg',
        'test-match-user', SYSDATE, 1
    );

    INSERT INTO CAT_MATCHRECORDS (
        MATCHID, SOURCEPHOTOID, CANDIDATECATID,
        SIMILARITYSCORE, RANKNO, CONFIRMSTATUS, CONFIRMUSERID
    ) VALUES (
        'test-match-record-1', 'test-match-source-photo', 'test-match-candidate-1',
        91.50, 1, NULL, NULL
    );
    INSERT INTO CAT_MATCHRECORDS (
        MATCHID, SOURCEPHOTOID, CANDIDATECATID,
        SIMILARITYSCORE, RANKNO, CONFIRMSTATUS, CONFIRMUSERID
    ) VALUES (
        'test-match-record-2', 'test-match-source-photo', 'test-match-candidate-2',
        82.25, 2, 'PENDING', NULL
    );

    SELECT COUNT(*) INTO valid_count
    FROM CAT_MATCHRECORDS
    WHERE SOURCEPHOTOID = 'test-match-source-photo';
    IF valid_count <> 2 THEN
        RAISE_APPLICATION_ERROR(-20034, 'Valid match records were not inserted.');
    END IF;

    SELECT LISTAGG(MATCHID, ',') WITHIN GROUP (ORDER BY RANKNO)
    INTO ordered_ids
    FROM CAT_MATCHRECORDS
    WHERE SOURCEPHOTOID = 'test-match-source-photo';
    IF ordered_ids <> 'test-match-record-1,test-match-record-2' THEN
        RAISE_APPLICATION_ERROR(-20035, 'Match ordering by rank is not stable.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: valid rows and rank ordering.');

    SELECT COUNT(*) INTO valid_count
    FROM CAT_MATCHRECORDS
    WHERE MATCHID = 'test-match-record-1'
      AND (CASE
               WHEN CONFIRMSTATUS IS NULL OR TRIM(CONFIRMSTATUS) IS NULL THEN 'PENDING'
               ELSE UPPER(TRIM(CONFIRMSTATUS))
           END) = 'PENDING';
    IF valid_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20036, 'NULL confirmation status was not treated as PENDING.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: NULL confirmation status is PENDING.');

    UPDATE CAT_MATCHRECORDS
    SET CONFIRMSTATUS = 'CONFIRMED',
        CONFIRMUSERID = 'test-match-user'
    WHERE MATCHID = 'test-match-record-1';
    SELECT COUNT(*) INTO valid_count
    FROM CAT_MATCHRECORDS
    WHERE MATCHID = 'test-match-record-1'
      AND CONFIRMSTATUS = 'CONFIRMED'
      AND CONFIRMUSERID = 'test-match-user';
    IF valid_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20037, 'Confirmation status or user was not persisted.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: confirmation status and user persisted.');

    expect_check_violation('test-match-invalid-score', 'test-match-candidate-2',
                           100.01, 3, 'PENDING', -2290,
                           'similarity score constraint');
    expect_check_violation('test-match-invalid-rank', 'test-match-candidate-2',
                           50, 0, 'PENDING', -2290,
                           'rank constraint');
    expect_check_violation('test-match-invalid-status', 'test-match-candidate-2',
                           50, 3, 'UNKNOWN', -2290,
                           'confirmation status constraint');
    expect_unique_violation('test-match-duplicate-candidate', 'test-match-candidate-1',
                            3, 'source/candidate uniqueness constraint');
    expect_unique_violation('test-match-duplicate-rank', 'test-match-candidate-2',
                            1, 'source/rank uniqueness constraint');

    DELETE FROM CAT_MATCHRECORDS
    WHERE MATCHID LIKE 'test-match-%'
       OR SOURCEPHOTOID = 'test-match-source-photo';
    DELETE FROM CAT_PHOTOS
    WHERE PHOTOID LIKE 'test-match-%';
    DELETE FROM CAT_CATS
    WHERE CATID LIKE 'test-match-%';
    DELETE FROM SYS_USERS
    WHERE USERID = 'test-match-user';
    DELETE FROM SYS_ROLES
    WHERE ROLEID = 'test-match-role';
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('PASS: acceptance data cleaned.');
END;
/

PROMPT ===== Cat matches acceptance test complete =====;
