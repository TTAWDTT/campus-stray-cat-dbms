SET DEFINE OFF;
PROMPT ===== Cat match demo data =====;

-- This script depends on the demo user, area, and source cat created by the
-- other B-group demo scripts. Every ID is intentionally stable so the script
-- can be run repeatedly during local API demonstrations.
MERGE INTO CAT_CATS target
USING (
    SELECT 'demo-cat-match-002' AS CATID,
           '匹配候选二号' AS CATNAME,
           'UNKNOWN' AS GENDER,
           '黑白' AS COLORPATTERN,
           'demo-area-library' AS MAINAREAID,
           'ON_CAMPUS' AS LIFESTATUS,
           'PUBLISHED' AS ARCHIVESTATUS
    FROM DUAL
    UNION ALL
    SELECT 'demo-cat-match-003',
           '匹配候选三号',
           'MALE',
           '橘白',
           'demo-area-library',
           'ON_CAMPUS',
           'PUBLISHED'
    FROM DUAL
) source
ON (target.CATID = source.CATID)
WHEN MATCHED THEN UPDATE SET
    target.CATNAME = source.CATNAME,
    target.GENDER = source.GENDER,
    target.COLORPATTERN = source.COLORPATTERN,
    target.MAINAREAID = source.MAINAREAID,
    target.LIFESTATUS = source.LIFESTATUS,
    target.ARCHIVESTATUS = source.ARCHIVESTATUS
WHEN NOT MATCHED THEN INSERT
    (CATID, CATNAME, GENDER, COLORPATTERN, MAINAREAID, LIFESTATUS, ARCHIVESTATUS)
VALUES
    (source.CATID, source.CATNAME, source.GENDER, source.COLORPATTERN,
     source.MAINAREAID, source.LIFESTATUS, source.ARCHIVESTATUS);

MERGE INTO CAT_PHOTOS target
USING (
    SELECT 'demo-match-source-001' AS PHOTOID,
           'demo-cat-campus-001' AS CATID,
           '/uploads/cats/demo-cat-campus-001/demo-match-source-001.jpg' AS PHOTOURL,
           'demo-user-zhaoqing' AS UPLOADUSERID,
           TO_DATE('2026-08-04 09:00:00', 'YYYY-MM-DD HH24:MI:SS') AS UPLOADTIME,
           0 AS ISPRIMARY
    FROM DUAL
    UNION ALL
    SELECT 'demo-match-candidate-002-primary',
           'demo-cat-match-002',
           '/uploads/cats/demo-cat-match-002/demo-match-candidate-002-primary.jpg',
           'demo-user-zhaoqing',
           TO_DATE('2026-08-04 09:05:00', 'YYYY-MM-DD HH24:MI:SS'),
           1
    FROM DUAL
    UNION ALL
    SELECT 'demo-match-candidate-003-primary',
           'demo-cat-match-003',
           '/uploads/cats/demo-cat-match-003/demo-match-candidate-003-primary.jpg',
           'demo-user-zhaoqing',
           TO_DATE('2026-08-04 09:06:00', 'YYYY-MM-DD HH24:MI:SS'),
           1
    FROM DUAL
) source
ON (target.PHOTOID = source.PHOTOID)
WHEN MATCHED THEN UPDATE SET
    target.CATID = source.CATID,
    target.PHOTOURL = source.PHOTOURL,
    target.UPLOADUSERID = source.UPLOADUSERID,
    target.UPLOADTIME = source.UPLOADTIME,
    target.ISPRIMARY = source.ISPRIMARY
WHEN NOT MATCHED THEN INSERT
    (PHOTOID, CATID, PHOTOURL, UPLOADUSERID, UPLOADTIME, ISPRIMARY)
VALUES
    (source.PHOTOID, source.CATID, source.PHOTOURL, source.UPLOADUSERID,
     source.UPLOADTIME, source.ISPRIMARY);

MERGE INTO CAT_MATCHRECORDS target
USING (
    SELECT 'demo-match-record-001' AS MATCHID,
           'demo-match-source-001' AS SOURCEPHOTOID,
           'demo-cat-match-002' AS CANDIDATECATID,
           91.25 AS SIMILARITYSCORE,
           1 AS RANKNO,
           'PENDING' AS CONFIRMSTATUS,
           CAST(NULL AS VARCHAR2(36)) AS CONFIRMUSERID
    FROM DUAL
    UNION ALL
    SELECT 'demo-match-record-002',
           'demo-match-source-001',
           'demo-cat-match-003',
           83.40,
           2,
           'PENDING',
           CAST(NULL AS VARCHAR2(36))
    FROM DUAL
) source
ON (target.MATCHID = source.MATCHID)
WHEN MATCHED THEN UPDATE SET
    target.SOURCEPHOTOID = source.SOURCEPHOTOID,
    target.CANDIDATECATID = source.CANDIDATECATID,
    target.SIMILARITYSCORE = source.SIMILARITYSCORE,
    target.RANKNO = source.RANKNO,
    target.CONFIRMSTATUS = source.CONFIRMSTATUS,
    target.CONFIRMUSERID = source.CONFIRMUSERID
WHEN NOT MATCHED THEN INSERT
    (MATCHID, SOURCEPHOTOID, CANDIDATECATID, SIMILARITYSCORE, RANKNO,
     CONFIRMSTATUS, CONFIRMUSERID)
VALUES
    (source.MATCHID, source.SOURCEPHOTOID, source.CANDIDATECATID,
     source.SIMILARITYSCORE, source.RANKNO, source.CONFIRMSTATUS,
     source.CONFIRMUSERID);

COMMIT;
PROMPT ===== Cat match demo data complete =====;
