SET DEFINE OFF;
SET SERVEROUTPUT ON;
WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;
PROMPT ===== Cat match Oracle constraints =====;

DECLARE
    invalid_score_count NUMBER;
    invalid_rank_count NUMBER;
    invalid_status_count NUMBER;
    duplicate_candidate_count NUMBER;
    duplicate_rank_count NUMBER;
    constraint_count NUMBER;

    PROCEDURE fail_if_present(p_count NUMBER, p_message VARCHAR2) IS
    BEGIN
        IF p_count > 0 THEN
            RAISE_APPLICATION_ERROR(-20020, p_message);
        END IF;
    END;
BEGIN
    SELECT COUNT(*)
    INTO invalid_score_count
    FROM CAT_MATCHRECORDS
    WHERE SIMILARITYSCORE IS NOT NULL
      AND (SIMILARITYSCORE < 0 OR SIMILARITYSCORE > 100);
    fail_if_present(invalid_score_count,
        'CAT_MATCHRECORDS contains similarity scores outside 0..100.');

    SELECT COUNT(*)
    INTO invalid_rank_count
    FROM CAT_MATCHRECORDS
    WHERE RANKNO IS NOT NULL
      AND RANKNO < 1;
    fail_if_present(invalid_rank_count,
        'CAT_MATCHRECORDS contains rank numbers below 1.');

    SELECT COUNT(*)
    INTO invalid_status_count
    FROM CAT_MATCHRECORDS
    WHERE CONFIRMSTATUS IS NOT NULL
      AND UPPER(TRIM(CONFIRMSTATUS)) NOT IN ('PENDING', 'CONFIRMED', 'REJECTED');
    fail_if_present(invalid_status_count,
        'CAT_MATCHRECORDS contains an unknown confirmation status.');

    SELECT COUNT(*)
    INTO duplicate_candidate_count
    FROM (
        SELECT SOURCEPHOTOID, CANDIDATECATID
        FROM CAT_MATCHRECORDS
        WHERE SOURCEPHOTOID IS NOT NULL
          AND CANDIDATECATID IS NOT NULL
        GROUP BY SOURCEPHOTOID, CANDIDATECATID
        HAVING COUNT(*) > 1
    );
    fail_if_present(duplicate_candidate_count,
        'CAT_MATCHRECORDS contains duplicate source-photo/candidate pairs.');

    SELECT COUNT(*)
    INTO duplicate_rank_count
    FROM (
        SELECT SOURCEPHOTOID, RANKNO
        FROM CAT_MATCHRECORDS
        WHERE SOURCEPHOTOID IS NOT NULL
          AND RANKNO IS NOT NULL
        GROUP BY SOURCEPHOTOID, RANKNO
        HAVING COUNT(*) > 1
    );
    fail_if_present(duplicate_rank_count,
        'CAT_MATCHRECORDS contains duplicate non-null ranks for a source photo.');

    UPDATE CAT_MATCHRECORDS
    SET CONFIRMSTATUS = UPPER(TRIM(CONFIRMSTATUS))
    WHERE CONFIRMSTATUS IS NOT NULL
      AND CONFIRMSTATUS <> UPPER(TRIM(CONFIRMSTATUS));
    COMMIT;
END;
/

DECLARE
    constraint_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO constraint_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME = 'CK_CAT_MATCH_SCORE';
    IF constraint_count = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE CAT_MATCHRECORDS ADD CONSTRAINT CK_CAT_MATCH_SCORE ' ||
            'CHECK (SIMILARITYSCORE BETWEEN 0 AND 100)';
        DBMS_OUTPUT.PUT_LINE('Created CK_CAT_MATCH_SCORE.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('CK_CAT_MATCH_SCORE already exists.');
    END IF;

    SELECT COUNT(*) INTO constraint_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME = 'CK_CAT_MATCH_RANK';
    IF constraint_count = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE CAT_MATCHRECORDS ADD CONSTRAINT CK_CAT_MATCH_RANK ' ||
            'CHECK (RANKNO IS NULL OR RANKNO >= 1)';
        DBMS_OUTPUT.PUT_LINE('Created CK_CAT_MATCH_RANK.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('CK_CAT_MATCH_RANK already exists.');
    END IF;

    SELECT COUNT(*) INTO constraint_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME = 'CK_CAT_MATCH_STATUS';
    IF constraint_count = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE CAT_MATCHRECORDS ADD CONSTRAINT CK_CAT_MATCH_STATUS ' ||
            'CHECK (CONFIRMSTATUS IS NULL OR CONFIRMSTATUS IN (''PENDING'', ''CONFIRMED'', ''REJECTED''))';
        DBMS_OUTPUT.PUT_LINE('Created CK_CAT_MATCH_STATUS.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('CK_CAT_MATCH_STATUS already exists.');
    END IF;

    SELECT COUNT(*) INTO constraint_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME = 'UQ_CAT_MATCH_SOURCE_CANDIDATE';
    IF constraint_count = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE CAT_MATCHRECORDS ADD CONSTRAINT UQ_CAT_MATCH_SOURCE_CANDIDATE ' ||
            'UNIQUE (SOURCEPHOTOID, CANDIDATECATID)';
        DBMS_OUTPUT.PUT_LINE('Created UQ_CAT_MATCH_SOURCE_CANDIDATE.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('UQ_CAT_MATCH_SOURCE_CANDIDATE already exists.');
    END IF;

    SELECT COUNT(*) INTO constraint_count
    FROM USER_CONSTRAINTS
    WHERE TABLE_NAME = 'CAT_MATCHRECORDS'
      AND CONSTRAINT_NAME = 'UQ_CAT_MATCH_SOURCE_RANK';
    IF constraint_count = 0 THEN
        EXECUTE IMMEDIATE
            'ALTER TABLE CAT_MATCHRECORDS ADD CONSTRAINT UQ_CAT_MATCH_SOURCE_RANK ' ||
            'UNIQUE (SOURCEPHOTOID, RANKNO)';
        DBMS_OUTPUT.PUT_LINE('Created UQ_CAT_MATCH_SOURCE_RANK.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('UQ_CAT_MATCH_SOURCE_RANK already exists.');
    END IF;
END;
/

PROMPT ===== Cat match Oracle constraints complete =====;
