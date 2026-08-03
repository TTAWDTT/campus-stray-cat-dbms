SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
PROMPT ===== Cat photos acceptance test =====;

DECLARE
    record_count NUMBER;
BEGIN
    DELETE FROM CAT_MATCHRECORDS
    WHERE SOURCEPHOTOID IN ('test-photo-one', 'test-photo-two');
    DELETE FROM CAT_PHOTOS
    WHERE PHOTOID IN ('test-photo-one', 'test-photo-two');
    DELETE FROM CAT_CATS
    WHERE CATID = 'test-photo-cat';
    DELETE FROM SYS_USERS
    WHERE USERID = 'test-photo-user';
    DELETE FROM SYS_ROLES
    WHERE ROLEID = 'test-photo-role';
    COMMIT;

    SELECT COUNT(*)
    INTO record_count
    FROM USER_INDEXES
    WHERE INDEX_NAME = 'UQ_CAT_PHOTOS_PRIMARY';
    IF record_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20010, 'UQ_CAT_PHOTOS_PRIMARY is missing.');
    END IF;

    INSERT INTO SYS_ROLES (ROLEID, ROLENAME)
    VALUES ('test-photo-role', 'Photo Test Role');

    INSERT INTO SYS_USERS (USERID, ROLEID, USERNAME, PASSWORDHASH, STATUS)
    VALUES ('test-photo-user', 'test-photo-role', 'photo_acceptance_user', 'TEST_ONLY', 'ACTIVE');

    INSERT INTO CAT_CATS (CATID, CATNAME, COLORPATTERN, LIFESTATUS, ARCHIVESTATUS)
    VALUES ('test-photo-cat', 'Photo Acceptance Cat', 'TEST', 'ON_CAMPUS', 'PUBLISHED');

    INSERT INTO CAT_PHOTOS (
        PHOTOID, CATID, PHOTOURL, FEATUREVECTOR,
        UPLOADUSERID, UPLOADTIME, ISPRIMARY
    ) VALUES (
        'test-photo-one', 'test-photo-cat',
        '/uploads/cats/test-photo-cat/test-photo-one.png', NULL,
        'test-photo-user', TO_DATE('2026-08-03 08:00:00', 'YYYY-MM-DD HH24:MI:SS'), 1
    );

    BEGIN
        INSERT INTO CAT_PHOTOS (
            PHOTOID, CATID, PHOTOURL, FEATUREVECTOR,
            UPLOADUSERID, UPLOADTIME, ISPRIMARY
        ) VALUES (
            'test-photo-two', 'test-photo-cat',
            '/uploads/cats/test-photo-cat/test-photo-two.png', '[0.12,-0.34,0.56]',
            'test-photo-user', TO_DATE('2026-08-03 09:00:00', 'YYYY-MM-DD HH24:MI:SS'), 1
        );
        RAISE_APPLICATION_ERROR(-20011, 'Unique primary photo index did not reject duplicate data.');
    EXCEPTION
        WHEN DUP_VAL_ON_INDEX THEN
            DBMS_OUTPUT.PUT_LINE('PASS: duplicate primary photo rejected.');
    END;

    INSERT INTO CAT_PHOTOS (
        PHOTOID, CATID, PHOTOURL, FEATUREVECTOR,
        UPLOADUSERID, UPLOADTIME, ISPRIMARY
    ) VALUES (
        'test-photo-two', 'test-photo-cat',
        '/uploads/cats/test-photo-cat/test-photo-two.png', '[0.12,-0.34,0.56]',
        'test-photo-user', TO_DATE('2026-08-03 09:00:00', 'YYYY-MM-DD HH24:MI:SS'), 0
    );

    SELECT COUNT(*)
    INTO record_count
    FROM CAT_PHOTOS
    WHERE PHOTOID = 'test-photo-two'
      AND FEATUREVECTOR IS JSON;
    IF record_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20012, 'Feature vector is not stored as JSON.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: feature vector stored as JSON CLOB.');

    UPDATE CAT_PHOTOS
    SET ISPRIMARY = 0
    WHERE CATID = 'test-photo-cat'
      AND ISPRIMARY = 1;

    UPDATE CAT_PHOTOS
    SET ISPRIMARY = 1
    WHERE CATID = 'test-photo-cat'
      AND PHOTOID = 'test-photo-two';

    SELECT COUNT(*)
    INTO record_count
    FROM CAT_PHOTOS
    WHERE CATID = 'test-photo-cat'
      AND ISPRIMARY = 1
      AND PHOTOID = 'test-photo-two';
    IF record_count <> 1 THEN
        RAISE_APPLICATION_ERROR(-20013, 'Primary photo switch failed.');
    END IF;
    DBMS_OUTPUT.PUT_LINE('PASS: primary photo switched atomically.');

    INSERT INTO CAT_MATCHRECORDS (MATCHID, SOURCEPHOTOID, CANDIDATECATID)
    VALUES ('test-photo-match', 'test-photo-two', 'test-photo-cat');

    BEGIN
        DELETE FROM CAT_PHOTOS
        WHERE PHOTOID = 'test-photo-two';
        RAISE_APPLICATION_ERROR(-20014, 'Referenced photo deletion was not rejected.');
    EXCEPTION
        WHEN OTHERS THEN
            IF SQLCODE <> -2292 THEN
                RAISE;
            END IF;
            DBMS_OUTPUT.PUT_LINE('PASS: referenced photo deletion rejected.');
    END;

    DELETE FROM CAT_MATCHRECORDS
    WHERE MATCHID = 'test-photo-match';
    DELETE FROM CAT_PHOTOS
    WHERE PHOTOID IN ('test-photo-one', 'test-photo-two');
    DELETE FROM CAT_CATS
    WHERE CATID = 'test-photo-cat';
    DELETE FROM SYS_USERS
    WHERE USERID = 'test-photo-user';
    DELETE FROM SYS_ROLES
    WHERE ROLEID = 'test-photo-role';
    COMMIT;

    DBMS_OUTPUT.PUT_LINE('PASS: acceptance data cleaned.');
END;
/

PROMPT ===== Cat photos acceptance test complete =====;
