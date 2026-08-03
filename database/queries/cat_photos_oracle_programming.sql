SET DEFINE OFF;
SET SERVEROUTPUT ON;
PROMPT ===== Cat photos Oracle constraints =====;

DECLARE
    duplicate_cat_count NUMBER;
    index_count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO duplicate_cat_count
    FROM (
        SELECT CATID
        FROM CAT_PHOTOS
        WHERE ISPRIMARY = 1
        GROUP BY CATID
        HAVING COUNT(*) > 1
    );

    IF duplicate_cat_count > 0 THEN
        RAISE_APPLICATION_ERROR(
            -20001,
            'CAT_PHOTOS contains cats with more than one primary photo; resolve duplicates before creating UQ_CAT_PHOTOS_PRIMARY.'
        );
    END IF;

    SELECT COUNT(*)
    INTO index_count
    FROM USER_INDEXES
    WHERE INDEX_NAME = 'UQ_CAT_PHOTOS_PRIMARY';

    IF index_count = 0 THEN
        EXECUTE IMMEDIATE
            'CREATE UNIQUE INDEX UQ_CAT_PHOTOS_PRIMARY ' ||
            'ON CAT_PHOTOS (CASE WHEN ISPRIMARY = 1 THEN CATID END)';
        DBMS_OUTPUT.PUT_LINE('Created UQ_CAT_PHOTOS_PRIMARY.');
    ELSE
        DBMS_OUTPUT.PUT_LINE('UQ_CAT_PHOTOS_PRIMARY already exists.');
    END IF;
END;
/

PROMPT ===== Cat photos Oracle constraints complete =====;
