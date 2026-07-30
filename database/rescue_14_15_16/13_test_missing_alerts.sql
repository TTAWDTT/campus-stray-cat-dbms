PROMPT Requirement 16: missing alerts...

COLUMN ALERTID FORMAT A36
COLUMN CATID FORMAT A20
COLUMN LASTSIGHTINGID FORMAT A36
COLUMN THRESHOLDDAYS FORMAT 999
COLUMN ALERTSTATUS FORMAT A12
COLUMN HANDLERUSERID FORMAT A26
COLUMN REMARK FORMAT A45

-- 验证 C# 调用路径：创建目击记录 -> 创建失踪预警 -> 更新为已寻回。
DECLARE
    V_SIGHTINGID CAT_SIGHTINGS.SIGHTINGID%TYPE;
    V_ALERTID CAT_MISSINGALERTS.ALERTID%TYPE;
BEGIN
    PKG_RESCUE_141516.CREATE_SIGHTING(
        P_CATID        => 'cat-demo-141516',
        P_USERID       => 'user-demo-reporter-141516',
        P_AREAID       => 'area-demo-141516',
        P_LONGITUDE    => 121.215,
        P_LATITUDE     => 31.289,
        P_PHOTOURL     => 'https://example.com/sighting-cat.jpg',
        P_SIGHTINGTIME => SYSDATE - 8,
        P_REMARK       => 'Last seen near Library East Gate',
        O_SIGHTINGID   => V_SIGHTINGID
    );

    DBMS_OUTPUT.PUT_LINE('Created sighting: ' || V_SIGHTINGID);

    PKG_RESCUE_141516.CREATE_MISSING_ALERT(
        P_CATID            => 'cat-demo-141516',
        P_LASTSIGHTINGID   => V_SIGHTINGID,
        P_LASTSIGHTINGTIME => SYSDATE - 8,
        P_THRESHOLDDAYS    => 7,
        P_HANDLERUSERID    => 'user-demo-handler-141516',
        P_REMARK           => 'Demo missing alert',
        O_ALERTID          => V_ALERTID
    );

    DBMS_OUTPUT.PUT_LINE('Created missing alert: ' || V_ALERTID);

    PKG_RESCUE_141516.UPDATE_MISSING_STATUS(
        V_ALERTID,
        'FOUND',
        'user-demo-handler-141516',
        'Cat found again near the Library East Gate'
    );
    COMMIT;
END;
/

-- 验证 Repository 查询使用的视图可读。
SELECT ALERTID,
       CATID,
       LASTSIGHTINGID,
       LASTSIGHTINGTIME,
       THRESHOLDDAYS,
       ALERTSTATUS,
       HANDLERUSERID,
       CLOSETIME,
       REMARK
FROM V_MISSING_ALERTS
WHERE CATID = 'cat-demo-141516'
ORDER BY ALERTTIME DESC;
