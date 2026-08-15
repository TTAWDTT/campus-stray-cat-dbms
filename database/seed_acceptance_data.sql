SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== Loading fixed acceptance seed data =====;
PROMPT This file is optional and may be executed after setup_all.sql.

-- This file intentionally contains fixed values rather than runtime randomness.
-- Every ID starts with seed- so the data can be inspected or removed safely.

-- The generated acceptance dataset is kept in a separate script so a normal
-- database rebuild stays quick.  It covers list pagination, role visibility,
-- status transitions, map points, adoption, rescue, volunteering and finance.

-- Reference roles and login accounts
MERGE INTO SYS_ROLES t
USING (
    SELECT 'seed-role-admin' ROLEID, 'ADMIN' ROLENAME, '固定验收管理员' DESCRIPTION, 'USER_MANAGE,ROLE_MANAGE,BLACKLIST_MANAGE' PERMISSIONSCOPE FROM DUAL
    UNION ALL SELECT 'seed-role-volunteer', 'VOLUNTEER', '固定验收志愿者', 'CAT_VIEW,SIGHTING_WRITE,SHIFT_CHECKIN' FROM DUAL
    UNION ALL SELECT 'seed-role-user', 'USER', '固定验收普通用户', 'CAT_VIEW,ADOPT_APPLY' FROM DUAL
    UNION ALL SELECT 'seed-role-vet', 'VET', '固定验收兽医', 'CAT_VIEW,MEDICAL_WRITE' FROM DUAL
) s ON (t.ROLEID = s.ROLEID)
WHEN MATCHED THEN UPDATE SET t.ROLENAME=s.ROLENAME, t.DESCRIPTION=s.DESCRIPTION, t.PERMISSIONSCOPE=s.PERMISSIONSCOPE
WHEN NOT MATCHED THEN INSERT (ROLEID, ROLENAME, DESCRIPTION, PERMISSIONSCOPE)
VALUES (s.ROLEID, s.ROLENAME, s.DESCRIPTION, s.PERMISSIONSCOPE);

MERGE INTO SYS_USERS t
USING (
    SELECT 'seed-user-admin' USERID, 'seed-role-admin' ROLEID, 'seed_admin' USERNAME,
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==' PASSWORDHASH,
           '验收管理员' REALNAME, 'SEED00001' STUDENTNO, '13900000001' PHONE, 'VERIFIED' VERIFYSTATUS, 'ACTIVE' STATUS FROM DUAL
    UNION ALL SELECT 'seed-user-vol-01','seed-role-volunteer','seed_volunteer_01','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','林小满','SEED00002','13900000002','VERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-vol-02','seed-role-volunteer','seed_volunteer_02','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','周小禾','SEED00003','13900000003','VERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-vol-03','seed-role-volunteer','seed_volunteer_03','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','许安然','SEED00004','13900000004','VERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-user-01','seed-role-user','seed_user_01','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','陈同学','SEED00005','13900000005','VERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-user-02','seed-role-user','seed_user_02','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','李同学','SEED00006','13900000006','UNVERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-blacklisted','seed-role-user','seed_user_blacklisted','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','受限用户','SEED00007','13900000007','VERIFIED','ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-user-vet','seed-role-vet','seed_vet','AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==','顾医生','SEED00008','13900000008','VERIFIED','ACTIVE' FROM DUAL
) s ON (t.USERID=s.USERID)
WHEN MATCHED THEN UPDATE SET t.ROLEID=s.ROLEID, t.USERNAME=s.USERNAME, t.PASSWORDHASH=s.PASSWORDHASH,
    t.REALNAME=s.REALNAME, t.STUDENTNO=s.STUDENTNO, t.PHONE=s.PHONE, t.VERIFYSTATUS=s.VERIFYSTATUS, t.STATUS=s.STATUS
WHEN NOT MATCHED THEN INSERT (USERID,ROLEID,USERNAME,PASSWORDHASH,REALNAME,STUDENTNO,PHONE,VERIFYSTATUS,STATUS)
VALUES (s.USERID,s.ROLEID,s.USERNAME,s.PASSWORDHASH,s.REALNAME,s.STUDENTNO,s.PHONE,s.VERIFYSTATUS,s.STATUS);

-- Campus areas: one root plus eleven child areas
MERGE INTO MAP_CAMPUSAREAS t
USING (
    SELECT 'seed-area-campus' AREAID,'四平路校区' AREANAME,'四平路校区' CAMPUSNAME,CAST(NULL AS VARCHAR2(36)) PARENTAREAID,'CAMPUS' AREATYPE,'LOW' RISKLEVEL,NULL GEOBOUNDARY FROM DUAL
    UNION ALL SELECT 'seed-area-library','图书馆周边','四平路校区','seed-area-campus','PUBLIC_AREA','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-north-woods','西北小树林','四平路校区','seed-area-campus','GREENBELT','MEDIUM',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-east-gate','东门入口','四平路校区','seed-area-campus','GATE','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-west-gate','西门入口','四平路校区','seed-area-campus','GATE','MEDIUM',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-dorm-1','一号宿舍区','四平路校区','seed-area-campus','PUBLIC_AREA','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-dorm-2','二号宿舍区','四平路校区','seed-area-campus','PUBLIC_AREA','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-canteen','学生食堂','四平路校区','seed-area-campus','PUBLIC_AREA','MEDIUM',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-gym','体育馆南侧','四平路校区','seed-area-campus','ACTIVITY_AREA','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-lake','人工湖步道','四平路校区','seed-area-campus','GREENBELT','MEDIUM',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-teaching','教学楼连廊','四平路校区','seed-area-campus','PUBLIC_AREA','LOW',NULL FROM DUAL
    UNION ALL SELECT 'seed-area-medical','校医院附近','四平路校区','seed-area-campus','PUBLIC_AREA','HIGH',NULL FROM DUAL
) s ON (t.AREAID=s.AREAID)
WHEN MATCHED THEN UPDATE SET t.AREANAME=s.AREANAME,t.CAMPUSNAME=s.CAMPUSNAME,t.PARENTAREAID=s.PARENTAREAID,t.AREATYPE=s.AREATYPE,t.RISKLEVEL=s.RISKLEVEL,t.GEOBOUNDARY=s.GEOBOUNDARY
WHEN NOT MATCHED THEN INSERT (AREAID,AREANAME,CAMPUSNAME,PARENTAREAID,AREATYPE,RISKLEVEL,GEOBOUNDARY)
VALUES (s.AREAID,s.AREANAME,s.CAMPUSNAME,s.PARENTAREAID,s.AREATYPE,s.RISKLEVEL,s.GEOBOUNDARY);

-- 31 cats: 21 on campus, 4 missing, 4 adopted and 2 deceased.
MERGE INTO CAT_CATS t
USING (
    SELECT 'seed-cat-001' CATID,'花卷' CATNAME,'FEMALE' GENDER,'DOMESTIC_SHORTHAIR' BREED,'狸花' COLORPATTERN,1 STERILIZEDFLAG,1 EARTIPFLAG,'亲人,贪吃' PERSONALITYTAGS,'seed-area-library' MAINAREAID,'ON_CAMPUS' LIFESTATUS,'PUBLISHED' ARCHIVESTATUS FROM DUAL
    UNION ALL SELECT 'seed-cat-002','芝麻','MALE','DOMESTIC_SHORTHAIR','黑白',1,1,'安静,谨慎','seed-area-north-woods','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-003','奶糖','FEMALE','DOMESTIC_SHORTHAIR','奶牛',0,0,'活泼,好奇','seed-area-canteen','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-004','小麦','MALE','DOMESTIC_SHORTHAIR','橘白',1,1,'亲人,慢热','seed-area-dorm-1','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-005','团子','FEMALE','DOMESTIC_SHORTHAIR','三花',1,1,'温柔,贪玩','seed-area-gym','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-006','煤球','MALE','DOMESTIC_SHORTHAIR','纯黑',1,1,'胆小,敏捷','seed-area-east-gate','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-007','豆包','MALE','DOMESTIC_SHORTHAIR','橘色',0,0,'亲人,贪吃','seed-area-lake','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-008','奶盖','FEMALE','DOMESTIC_SHORTHAIR','白色',1,1,'安静,爱晒太阳','seed-area-teaching','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-009','芝士','MALE','DOMESTIC_SHORTHAIR','橘白',1,1,'活泼,话多','seed-area-library','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-010','布丁','FEMALE','DOMESTIC_SHORTHAIR','浅三花',1,1,'温柔,慢热','seed-area-dorm-2','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-011','可乐','MALE','DOMESTIC_SHORTHAIR','黑白',1,1,'警觉,聪明','seed-area-west-gate','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-012','泡芙','FEMALE','DOMESTIC_SHORTHAIR','白底狸花',0,0,'亲人,贪玩','seed-area-medical','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-013','椰椰','FEMALE','DOMESTIC_SHORTHAIR','奶油色',1,1,'安静,粘人','seed-area-canteen','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-014','小虎','MALE','DOMESTIC_SHORTHAIR','虎斑',1,1,'敏捷,好奇','seed-area-north-woods','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-015','元宝','MALE','DOMESTIC_SHORTHAIR','橘色',1,1,'贪吃,亲人','seed-area-library','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-016','云朵','FEMALE','DOMESTIC_SHORTHAIR','白色',1,1,'胆小,温柔','seed-area-dorm-1','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-017','焦糖','MALE','DOMESTIC_SHORTHAIR','棕色',0,0,'活泼,好奇','seed-area-lake','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-018','柚子','FEMALE','DOMESTIC_SHORTHAIR','橘白',1,1,'慢热,亲人','seed-area-gym','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-019','小葵','FEMALE','DOMESTIC_SHORTHAIR','三花',1,1,'安静,爱晒太阳','seed-area-teaching','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-020','麦芽','MALE','DOMESTIC_SHORTHAIR','狸花',1,1,'聪明,谨慎','seed-area-east-gate','ON_CAMPUS','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-021','阿福','MALE','DOMESTIC_SHORTHAIR','橘白',1,1,'警觉,亲人','seed-area-west-gate','MISSING','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-022','小满','FEMALE','DOMESTIC_SHORTHAIR','黑白',1,1,'胆小,安静','seed-area-dorm-2','MISSING','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-023','椰果','MALE','DOMESTIC_SHORTHAIR','纯黑',1,1,'敏捷,谨慎','seed-area-north-woods','MISSING','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-024','糯米','FEMALE','DOMESTIC_SHORTHAIR','白色',0,0,'亲人,贪吃','seed-area-canteen','MISSING','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-025','星河','MALE','DOMESTIC_SHORTHAIR','狸花',1,1,'温柔,亲人','seed-area-library','ADOPTED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-026','铃铛','FEMALE','DOMESTIC_SHORTHAIR','三花',1,1,'活泼,粘人','seed-area-dorm-1','ADOPTED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-027','橙子','MALE','DOMESTIC_SHORTHAIR','橘色',1,1,'贪吃,好奇','seed-area-gym','ADOPTED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-028','小贝','FEMALE','DOMESTIC_SHORTHAIR','奶牛',1,1,'安静,慢热','seed-area-lake','ADOPTED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-029','阿布','MALE','DOMESTIC_SHORTHAIR','黑白',1,1,'谨慎,聪明','seed-area-medical','DECEASED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-030','冬至','FEMALE','DOMESTIC_SHORTHAIR','白底狸花',1,1,'温柔,安静','seed-area-teaching','DECEASED','PUBLISHED' FROM DUAL
    UNION ALL SELECT 'seed-cat-031','奶龙','UNKNOWN','DOMESTIC_SHORTHAIR','金黄色',0,0,'活泼,亲人','seed-area-gym','ON_CAMPUS','PUBLISHED' FROM DUAL
) s ON (t.CATID=s.CATID)
WHEN MATCHED THEN UPDATE SET t.CATNAME=s.CATNAME,t.GENDER=s.GENDER,t.BREED=s.BREED,t.COLORPATTERN=s.COLORPATTERN,t.STERILIZEDFLAG=s.STERILIZEDFLAG,t.EARTIPFLAG=s.EARTIPFLAG,t.PERSONALITYTAGS=s.PERSONALITYTAGS,t.MAINAREAID=s.MAINAREAID,t.LIFESTATUS=s.LIFESTATUS,t.ARCHIVESTATUS=s.ARCHIVESTATUS
WHEN NOT MATCHED THEN INSERT (CATID,CATNAME,GENDER,BREED,COLORPATTERN,STERILIZEDFLAG,EARTIPFLAG,PERSONALITYTAGS,MAINAREAID,LIFESTATUS,ARCHIVESTATUS)
VALUES (s.CATID,s.CATNAME,s.GENDER,s.BREED,s.COLORPATTERN,s.STERILIZEDFLAG,s.EARTIPFLAG,s.PERSONALITYTAGS,s.MAINAREAID,s.LIFESTATUS,s.ARCHIVESTATUS);

-- Service points and cat photos
MERGE INTO MAP_SERVICEPOINTS t
USING (
    SELECT 'seed-point-001' POINTID,'seed-area-library' AREAID,'图书馆东侧投喂点' POINTNAME,'FEEDING' POINTTYPE,121.50650 LONGITUDE,31.28210 LATITUDE,'ACTIVE' FACILITYSTATUS,TO_DATE('2026-07-01 08:00:00','YYYY-MM-DD HH24:MI:SS') DEPLOYTIME FROM DUAL
    UNION ALL SELECT 'seed-point-002','seed-area-library','图书馆北侧猫窝','NEST',121.50630,31.28240,'MAINTENANCE',TO_DATE('2026-07-02 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-003','seed-area-north-woods','小树林西侧投喂点','FEEDING',121.50520,31.28320,'ACTIVE',TO_DATE('2026-07-03 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-004','seed-area-north-woods','小树林树下猫窝','NEST',121.50500,31.28340,'ACTIVE',TO_DATE('2026-07-04 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-005','seed-area-east-gate','东门值班室旁','ACTIVITY',121.50810,31.28090,'ACTIVE',TO_DATE('2026-07-05 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-006','seed-area-west-gate','西门花坛边','FEEDING',121.50390,31.28110,'INACTIVE',TO_DATE('2026-07-06 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-007','seed-area-dorm-1','一号宿舍楼下','FEEDING',121.50720,31.28410,'ACTIVE',TO_DATE('2026-07-07 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-008','seed-area-dorm-1','一号宿舍后猫窝','NEST',121.50700,31.28440,'ACTIVE',TO_DATE('2026-07-08 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-009','seed-area-dorm-2','二号宿舍南侧','ACTIVITY',121.50900,31.28420,'ACTIVE',TO_DATE('2026-07-09 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-010','seed-area-canteen','食堂后门投喂点','FEEDING',121.50690,31.27990,'MAINTENANCE',TO_DATE('2026-07-10 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-011','seed-area-canteen','食堂西侧猫窝','NEST',121.50640,31.27980,'ACTIVE',TO_DATE('2026-07-11 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-012','seed-area-gym','体育馆南门','ACTIVITY',121.50480,31.27860,'ACTIVE',TO_DATE('2026-07-12 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-013','seed-area-lake','湖边长椅旁','FEEDING',121.50290,31.28000,'ACTIVE',TO_DATE('2026-07-13 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-014','seed-area-lake','湖心步道猫窝','NEST',121.50250,31.28030,'ACTIVE',TO_DATE('2026-07-14 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-015','seed-area-teaching','教学楼一层连廊','ACTIVITY',121.50420,31.28200,'ACTIVE',TO_DATE('2026-07-15 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-016','seed-area-teaching','教学楼东侧投喂点','FEEDING',121.50460,31.28220,'ACTIVE',TO_DATE('2026-07-16 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-017','seed-area-medical','校医院门口','ACTIVITY',121.50940,31.28150,'ACTIVE',TO_DATE('2026-07-17 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-point-018','seed-area-medical','校医院后侧猫窝','NEST',121.50970,31.28180,'MAINTENANCE',TO_DATE('2026-07-18 08:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
) s ON (t.POINTID=s.POINTID)
WHEN MATCHED THEN UPDATE SET t.AREAID=s.AREAID,t.POINTNAME=s.POINTNAME,t.POINTTYPE=s.POINTTYPE,t.LONGITUDE=s.LONGITUDE,t.LATITUDE=s.LATITUDE,t.FACILITYSTATUS=s.FACILITYSTATUS,t.DEPLOYTIME=s.DEPLOYTIME
WHEN NOT MATCHED THEN INSERT (POINTID,AREAID,POINTNAME,POINTTYPE,LONGITUDE,LATITUDE,FACILITYSTATUS,DEPLOYTIME)
VALUES (s.POINTID,s.AREAID,s.POINTNAME,s.POINTTYPE,s.LONGITUDE,s.LATITUDE,s.FACILITYSTATUS,s.DEPLOYTIME);

MERGE INTO CAT_PHOTOS t
USING (
    SELECT 'seed-photo-001' PHOTOID,'seed-cat-001' CATID,'https://placehold.co/800x600/png?text=seed-cat-001' PHOTOURL,NULL FEATUREVECTOR,'seed-user-vol-01' UPLOADUSERID,TO_DATE('2026-07-01 09:00:00','YYYY-MM-DD HH24:MI:SS') UPLOADTIME,1 ISPRIMARY FROM DUAL
    UNION ALL SELECT 'seed-photo-002','seed-cat-002','https://placehold.co/800x600/png?text=seed-cat-002',NULL,'seed-user-vol-01',TO_DATE('2026-07-02 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-003','seed-cat-003','https://placehold.co/800x600/png?text=seed-cat-003',NULL,'seed-user-vol-01',TO_DATE('2026-07-03 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-004','seed-cat-004','https://placehold.co/800x600/png?text=seed-cat-004',NULL,'seed-user-vol-01',TO_DATE('2026-07-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-005','seed-cat-005','https://placehold.co/800x600/png?text=seed-cat-005',NULL,'seed-user-vol-01',TO_DATE('2026-07-05 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-006','seed-cat-006','https://placehold.co/800x600/png?text=seed-cat-006',NULL,'seed-user-vol-01',TO_DATE('2026-07-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-007','seed-cat-007','/images/cats/doubao.jpg',NULL,'seed-user-vol-01',TO_DATE('2026-07-07 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-008','seed-cat-008','https://placehold.co/800x600/png?text=seed-cat-008',NULL,'seed-user-vol-01',TO_DATE('2026-07-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-009','seed-cat-009','https://placehold.co/800x600/png?text=seed-cat-009',NULL,'seed-user-vol-01',TO_DATE('2026-07-09 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-010','seed-cat-010','https://placehold.co/800x600/png?text=seed-cat-010',NULL,'seed-user-vol-01',TO_DATE('2026-07-10 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-011','seed-cat-011','https://placehold.co/800x600/png?text=seed-cat-011',NULL,'seed-user-vol-01',TO_DATE('2026-07-11 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-012','seed-cat-012','https://placehold.co/800x600/png?text=seed-cat-012',NULL,'seed-user-vol-01',TO_DATE('2026-07-12 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-013','seed-cat-013','https://placehold.co/800x600/png?text=seed-cat-013',NULL,'seed-user-vol-01',TO_DATE('2026-07-13 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-014','seed-cat-014','https://placehold.co/800x600/png?text=seed-cat-014',NULL,'seed-user-vol-01',TO_DATE('2026-07-14 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-015','seed-cat-015','https://placehold.co/800x600/png?text=seed-cat-015',NULL,'seed-user-vol-01',TO_DATE('2026-07-15 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-016','seed-cat-016','https://placehold.co/800x600/png?text=seed-cat-016',NULL,'seed-user-vol-01',TO_DATE('2026-07-16 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-017','seed-cat-017','https://placehold.co/800x600/png?text=seed-cat-017',NULL,'seed-user-vol-01',TO_DATE('2026-07-17 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-018','seed-cat-018','https://placehold.co/800x600/png?text=seed-cat-018',NULL,'seed-user-vol-01',TO_DATE('2026-07-18 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-019','seed-cat-019','https://placehold.co/800x600/png?text=seed-cat-019',NULL,'seed-user-vol-01',TO_DATE('2026-07-19 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-020','seed-cat-020','https://placehold.co/800x600/png?text=seed-cat-020',NULL,'seed-user-vol-01',TO_DATE('2026-07-20 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-021','seed-cat-021','https://placehold.co/800x600/png?text=seed-cat-021',NULL,'seed-user-vol-01',TO_DATE('2026-07-21 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-022','seed-cat-022','https://placehold.co/800x600/png?text=seed-cat-022',NULL,'seed-user-vol-01',TO_DATE('2026-07-22 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-023','seed-cat-023','https://placehold.co/800x600/png?text=seed-cat-023',NULL,'seed-user-vol-01',TO_DATE('2026-07-23 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-024','seed-cat-024','https://placehold.co/800x600/png?text=seed-cat-024',NULL,'seed-user-vol-01',TO_DATE('2026-07-24 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-025','seed-cat-025','https://placehold.co/800x600/png?text=seed-cat-025',NULL,'seed-user-vol-01',TO_DATE('2026-07-25 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-026','seed-cat-026','https://placehold.co/800x600/png?text=seed-cat-026',NULL,'seed-user-vol-01',TO_DATE('2026-07-26 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-027','seed-cat-027','https://placehold.co/800x600/png?text=seed-cat-027',NULL,'seed-user-vol-01',TO_DATE('2026-07-27 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-028','seed-cat-028','https://placehold.co/800x600/png?text=seed-cat-028',NULL,'seed-user-vol-01',TO_DATE('2026-07-28 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-029','seed-cat-029','https://placehold.co/800x600/png?text=seed-cat-029',NULL,'seed-user-vol-01',TO_DATE('2026-07-29 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-030','seed-cat-030','https://placehold.co/800x600/png?text=seed-cat-030',NULL,'seed-user-vol-01',TO_DATE('2026-07-30 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
    UNION ALL SELECT 'seed-photo-031','seed-cat-001','https://placehold.co/800x600/png?text=花卷-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-07-31 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-032','seed-cat-002','https://placehold.co/800x600/png?text=芝麻-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-01 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-033','seed-cat-003','https://placehold.co/800x600/png?text=奶糖-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-02 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-034','seed-cat-004','https://placehold.co/800x600/png?text=小麦-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-03 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-035','seed-cat-005','https://placehold.co/800x600/png?text=团子-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-036','seed-cat-006','https://placehold.co/800x600/png?text=煤球-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-05 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-037','seed-cat-007','https://placehold.co/800x600/png?text=豆包-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-038','seed-cat-008','https://placehold.co/800x600/png?text=奶盖-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-07 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-039','seed-cat-009','https://placehold.co/800x600/png?text=芝士-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-040','seed-cat-010','https://placehold.co/800x600/png?text=布丁-侧面',NULL,'seed-user-vol-02',TO_DATE('2026-08-09 09:00:00','YYYY-MM-DD HH24:MI:SS'),0 FROM DUAL
    UNION ALL SELECT 'seed-photo-041','seed-cat-031','/images/cats/nailong.jpg',NULL,'seed-user-vol-01',TO_DATE('2026-08-10 09:00:00','YYYY-MM-DD HH24:MI:SS'),1 FROM DUAL
) s ON (t.PHOTOID=s.PHOTOID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.PHOTOURL=s.PHOTOURL,t.FEATUREVECTOR=s.FEATUREVECTOR,t.UPLOADUSERID=s.UPLOADUSERID,t.UPLOADTIME=s.UPLOADTIME,t.ISPRIMARY=s.ISPRIMARY
WHEN NOT MATCHED THEN INSERT (PHOTOID,CATID,PHOTOURL,FEATUREVECTOR,UPLOADUSERID,UPLOADTIME,ISPRIMARY)
VALUES (s.PHOTOID,s.CATID,s.PHOTOURL,s.FEATUREVECTOR,s.UPLOADUSERID,s.UPLOADTIME,s.ISPRIMARY);

-- 90 sightings: three observations per cat, enough for the map trajectory and paging views.
MERGE INTO CAT_SIGHTINGS t
USING (
    SELECT 'seed-sighting-001' SIGHTINGID,'seed-cat-001' CATID,'seed-user-user-01' USERID,'seed-area-library' AREAID,121.50650 LONGITUDE,31.28210 LATITUDE,CAST(NULL AS VARCHAR2(300)) PHOTOURL,TO_DATE('2026-07-01 18:10:00','YYYY-MM-DD HH24:MI:SS') SIGHTINGTIME,'在图书馆东侧投喂点附近活动' REMARK FROM DUAL
    UNION ALL SELECT 'seed-sighting-002','seed-cat-001','seed-user-vol-01','seed-area-library',121.50642,31.28214,NULL,TO_DATE('2026-07-04 18:20:00','YYYY-MM-DD HH24:MI:SS'),'精神状态正常，靠近志愿者' FROM DUAL
    UNION ALL SELECT 'seed-sighting-003','seed-cat-001','seed-user-user-02','seed-area-teaching',121.50435,31.28203,NULL,TO_DATE('2026-07-08 17:50:00','YYYY-MM-DD HH24:MI:SS'),'在教学楼连廊短暂停留' FROM DUAL
    UNION ALL SELECT 'seed-sighting-004','seed-cat-002','seed-user-user-01','seed-area-north-woods',121.50510,31.28320,NULL,TO_DATE('2026-07-02 08:15:00','YYYY-MM-DD HH24:MI:SS'),'在小树林边缘晒太阳' FROM DUAL
    UNION ALL SELECT 'seed-sighting-005','seed-cat-002','seed-user-vol-02','seed-area-north-woods',121.50504,31.28330,NULL,TO_DATE('2026-07-06 08:30:00','YYYY-MM-DD HH24:MI:SS'),'听到叫声但猫咪较警觉' FROM DUAL
    UNION ALL SELECT 'seed-sighting-006','seed-cat-002','seed-user-user-02','seed-area-library',121.50620,31.28255,NULL,TO_DATE('2026-07-11 09:00:00','YYYY-MM-DD HH24:MI:SS'),'沿图书馆后墙活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-007','seed-cat-003','seed-user-user-01','seed-area-canteen',121.50690,31.27990,NULL,TO_DATE('2026-07-03 12:10:00','YYYY-MM-DD HH24:MI:SS'),'食堂后门附近觅食' FROM DUAL
    UNION ALL SELECT 'seed-sighting-008','seed-cat-003','seed-user-vol-01','seed-area-canteen',121.50682,31.27995,NULL,TO_DATE('2026-07-07 12:20:00','YYYY-MM-DD HH24:MI:SS'),'已完成补水' FROM DUAL
    UNION ALL SELECT 'seed-sighting-009','seed-cat-003','seed-user-user-02','seed-area-dorm-2',121.50900,31.28420,NULL,TO_DATE('2026-07-12 18:40:00','YYYY-MM-DD HH24:MI:SS'),'向二号宿舍方向移动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-010','seed-cat-004','seed-user-user-01','seed-area-dorm-1',121.50720,31.28410,NULL,TO_DATE('2026-07-04 07:40:00','YYYY-MM-DD HH24:MI:SS'),'宿舍楼下活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-011','seed-cat-004','seed-user-vol-02','seed-area-dorm-1',121.50725,31.28405,NULL,TO_DATE('2026-07-09 07:50:00','YYYY-MM-DD HH24:MI:SS'),'正在休息' FROM DUAL
    UNION ALL SELECT 'seed-sighting-012','seed-cat-004','seed-user-user-02','seed-area-canteen',121.50675,31.28000,NULL,TO_DATE('2026-07-13 12:00:00','YYYY-MM-DD HH24:MI:SS'),'午间在食堂附近出现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-013','seed-cat-005','seed-user-user-01','seed-area-gym',121.50480,31.27860,NULL,TO_DATE('2026-07-05 16:20:00','YYYY-MM-DD HH24:MI:SS'),'体育馆南门附近' FROM DUAL
    UNION ALL SELECT 'seed-sighting-014','seed-cat-005','seed-user-vol-01','seed-area-gym',121.50472,31.27865,NULL,TO_DATE('2026-07-10 16:30:00','YYYY-MM-DD HH24:MI:SS'),'与另一只猫短暂追逐' FROM DUAL
    UNION ALL SELECT 'seed-sighting-015','seed-cat-005','seed-user-user-02','seed-area-lake',121.50300,31.28000,NULL,TO_DATE('2026-07-14 17:40:00','YYYY-MM-DD HH24:MI:SS'),'沿湖边步道活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-016','seed-cat-006','seed-user-user-01','seed-area-east-gate',121.50810,31.28090,NULL,TO_DATE('2026-07-06 18:00:00','YYYY-MM-DD HH24:MI:SS'),'东门值班室旁出现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-017','seed-cat-006','seed-user-vol-02','seed-area-east-gate',121.50800,31.28100,NULL,TO_DATE('2026-07-11 18:10:00','YYYY-MM-DD HH24:MI:SS'),'耳尖标记清晰' FROM DUAL
    UNION ALL SELECT 'seed-sighting-018','seed-cat-006','seed-user-user-02','seed-area-library',121.50660,31.28220,NULL,TO_DATE('2026-07-15 18:20:00','YYYY-MM-DD HH24:MI:SS'),'晚间经过图书馆' FROM DUAL
    UNION ALL SELECT 'seed-sighting-019','seed-cat-007','seed-user-user-01','seed-area-lake',121.50290,31.28000,NULL,TO_DATE('2026-07-07 09:10:00','YYYY-MM-DD HH24:MI:SS'),'湖边长椅旁活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-020','seed-cat-007','seed-user-vol-01','seed-area-lake',121.50285,31.28008,NULL,TO_DATE('2026-07-12 09:20:00','YYYY-MM-DD HH24:MI:SS'),'完成一次投喂' FROM DUAL
    UNION ALL SELECT 'seed-sighting-021','seed-cat-007','seed-user-user-02','seed-area-gym',121.50470,31.27870,NULL,TO_DATE('2026-07-16 09:30:00','YYYY-MM-DD HH24:MI:SS'),'向体育馆方向离开' FROM DUAL
    UNION ALL SELECT 'seed-sighting-022','seed-cat-008','seed-user-user-01','seed-area-teaching',121.50420,31.28200,NULL,TO_DATE('2026-07-08 10:00:00','YYYY-MM-DD HH24:MI:SS'),'教学楼连廊晒太阳' FROM DUAL
    UNION ALL SELECT 'seed-sighting-023','seed-cat-008','seed-user-vol-02','seed-area-library',121.50630,31.28240,NULL,TO_DATE('2026-07-13 10:10:00','YYYY-MM-DD HH24:MI:SS'),'猫窝内休息' FROM DUAL
    UNION ALL SELECT 'seed-sighting-024','seed-cat-008','seed-user-user-02','seed-area-teaching',121.50430,31.28205,NULL,TO_DATE('2026-07-17 10:20:00','YYYY-MM-DD HH24:MI:SS'),'状态稳定' FROM DUAL
    UNION ALL SELECT 'seed-sighting-025','seed-cat-009','seed-user-user-01','seed-area-library',121.50650,31.28210,NULL,TO_DATE('2026-07-09 19:00:00','YYYY-MM-DD HH24:MI:SS'),'在投喂点等待' FROM DUAL
    UNION ALL SELECT 'seed-sighting-026','seed-cat-009','seed-user-vol-01','seed-area-library',121.50655,31.28212,NULL,TO_DATE('2026-07-14 19:10:00','YYYY-MM-DD HH24:MI:SS'),'已进食' FROM DUAL
    UNION ALL SELECT 'seed-sighting-027','seed-cat-009','seed-user-user-02','seed-area-north-woods',121.50510,31.28315,NULL,TO_DATE('2026-07-18 19:20:00','YYYY-MM-DD HH24:MI:SS'),'前往小树林' FROM DUAL
    UNION ALL SELECT 'seed-sighting-028','seed-cat-010','seed-user-user-01','seed-area-dorm-2',121.50900,31.28420,NULL,TO_DATE('2026-07-10 07:20:00','YYYY-MM-DD HH24:MI:SS'),'二号宿舍南侧' FROM DUAL
    UNION ALL SELECT 'seed-sighting-029','seed-cat-010','seed-user-vol-02','seed-area-dorm-2',121.50910,31.28418,NULL,TO_DATE('2026-07-15 07:30:00','YYYY-MM-DD HH24:MI:SS'),'靠近猫窝' FROM DUAL
    UNION ALL SELECT 'seed-sighting-030','seed-cat-010','seed-user-user-02','seed-area-canteen',121.50690,31.27990,NULL,TO_DATE('2026-07-19 07:40:00','YYYY-MM-DD HH24:MI:SS'),'食堂后门活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-031','seed-cat-011','seed-user-user-01','seed-area-west-gate',121.50390,31.28110,NULL,TO_DATE('2026-07-11 08:40:00','YYYY-MM-DD HH24:MI:SS'),'西门花坛边' FROM DUAL
    UNION ALL SELECT 'seed-sighting-032','seed-cat-011','seed-user-vol-01','seed-area-west-gate',121.50400,31.28115,NULL,TO_DATE('2026-07-16 08:50:00','YYYY-MM-DD HH24:MI:SS'),'远离人群' FROM DUAL
    UNION ALL SELECT 'seed-sighting-033','seed-cat-011','seed-user-user-02','seed-area-library',121.50640,31.28225,NULL,TO_DATE('2026-07-20 09:00:00','YYYY-MM-DD HH24:MI:SS'),'沿围墙行走' FROM DUAL
    UNION ALL SELECT 'seed-sighting-034','seed-cat-012','seed-user-user-01','seed-area-medical',121.50940,31.28150,NULL,TO_DATE('2026-07-12 15:00:00','YYYY-MM-DD HH24:MI:SS'),'校医院附近' FROM DUAL
    UNION ALL SELECT 'seed-sighting-035','seed-cat-012','seed-user-vol-02','seed-area-medical',121.50935,31.28155,NULL,TO_DATE('2026-07-17 15:10:00','YYYY-MM-DD HH24:MI:SS'),'脚部无明显异常' FROM DUAL
    UNION ALL SELECT 'seed-sighting-036','seed-cat-012','seed-user-user-02','seed-area-canteen',121.50680,31.28000,NULL,TO_DATE('2026-07-21 15:20:00','YYYY-MM-DD HH24:MI:SS'),'离开校医院' FROM DUAL
    UNION ALL SELECT 'seed-sighting-037','seed-cat-013','seed-user-user-01','seed-area-canteen',121.50690,31.27990,NULL,TO_DATE('2026-07-13 12:30:00','YYYY-MM-DD HH24:MI:SS'),'食堂后门午休' FROM DUAL
    UNION ALL SELECT 'seed-sighting-038','seed-cat-013','seed-user-vol-01','seed-area-canteen',121.50685,31.27995,NULL,TO_DATE('2026-07-18 12:40:00','YYYY-MM-DD HH24:MI:SS'),'接受抚摸' FROM DUAL
    UNION ALL SELECT 'seed-sighting-039','seed-cat-013','seed-user-user-02','seed-area-dorm-1',121.50720,31.28410,NULL,TO_DATE('2026-07-22 12:50:00','YYYY-MM-DD HH24:MI:SS'),'前往宿舍区' FROM DUAL
    UNION ALL SELECT 'seed-sighting-040','seed-cat-014','seed-user-user-01','seed-area-north-woods',121.50520,31.28320,NULL,TO_DATE('2026-07-14 08:00:00','YYYY-MM-DD HH24:MI:SS'),'树丛间快速穿过' FROM DUAL
    UNION ALL SELECT 'seed-sighting-041','seed-cat-014','seed-user-vol-02','seed-area-north-woods',121.50515,31.28325,NULL,TO_DATE('2026-07-19 08:10:00','YYYY-MM-DD HH24:MI:SS'),'有轻微打喷嚏' FROM DUAL
    UNION ALL SELECT 'seed-sighting-042','seed-cat-014','seed-user-user-02','seed-area-library',121.50630,31.28235,NULL,TO_DATE('2026-07-23 08:20:00','YYYY-MM-DD HH24:MI:SS'),'在图书馆背面出现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-043','seed-cat-015','seed-user-user-01','seed-area-library',121.50650,31.28210,NULL,TO_DATE('2026-07-15 18:30:00','YYYY-MM-DD HH24:MI:SS'),'等待晚餐' FROM DUAL
    UNION ALL SELECT 'seed-sighting-044','seed-cat-015','seed-user-vol-01','seed-area-library',121.50652,31.28205,NULL,TO_DATE('2026-07-20 18:40:00','YYYY-MM-DD HH24:MI:SS'),'状态良好' FROM DUAL
    UNION ALL SELECT 'seed-sighting-045','seed-cat-015','seed-user-user-02','seed-area-teaching',121.50425,31.28200,NULL,TO_DATE('2026-07-24 18:50:00','YYYY-MM-DD HH24:MI:SS'),'在教学楼外墙边' FROM DUAL
    UNION ALL SELECT 'seed-sighting-046','seed-cat-016','seed-user-user-01','seed-area-dorm-1',121.50720,31.28410,NULL,TO_DATE('2026-07-16 07:30:00','YYYY-MM-DD HH24:MI:SS'),'宿舍楼下' FROM DUAL
    UNION ALL SELECT 'seed-sighting-047','seed-cat-016','seed-user-vol-02','seed-area-dorm-1',121.50715,31.28405,NULL,TO_DATE('2026-07-21 07:40:00','YYYY-MM-DD HH24:MI:SS'),'在车棚旁休息' FROM DUAL
    UNION ALL SELECT 'seed-sighting-048','seed-cat-016','seed-user-user-02','seed-area-lake',121.50290,31.28000,NULL,TO_DATE('2026-07-25 07:50:00','YYYY-MM-DD HH24:MI:SS'),'清晨沿湖散步' FROM DUAL
    UNION ALL SELECT 'seed-sighting-049','seed-cat-017','seed-user-user-01','seed-area-lake',121.50290,31.28000,NULL,TO_DATE('2026-07-17 16:00:00','YYYY-MM-DD HH24:MI:SS'),'湖边草地' FROM DUAL
    UNION ALL SELECT 'seed-sighting-050','seed-cat-017','seed-user-vol-01','seed-area-lake',121.50285,31.28005,NULL,TO_DATE('2026-07-22 16:10:00','YYYY-MM-DD HH24:MI:SS'),'完成一次喂水' FROM DUAL
    UNION ALL SELECT 'seed-sighting-051','seed-cat-017','seed-user-user-02','seed-area-gym',121.50480,31.27860,NULL,TO_DATE('2026-07-26 16:20:00','YYYY-MM-DD HH24:MI:SS'),'体育馆南侧' FROM DUAL
    UNION ALL SELECT 'seed-sighting-052','seed-cat-018','seed-user-user-01','seed-area-gym',121.50480,31.27860,NULL,TO_DATE('2026-07-18 17:00:00','YYYY-MM-DD HH24:MI:SS'),'体育馆外墙' FROM DUAL
    UNION ALL SELECT 'seed-sighting-053','seed-cat-018','seed-user-vol-02','seed-area-gym',121.50475,31.27865,NULL,TO_DATE('2026-07-23 17:10:00','YYYY-MM-DD HH24:MI:SS'),'与志愿者互动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-054','seed-cat-018','seed-user-user-02','seed-area-lake',121.50300,31.28005,NULL,TO_DATE('2026-07-27 17:20:00','YYYY-MM-DD HH24:MI:SS'),'向湖边移动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-055','seed-cat-019','seed-user-user-01','seed-area-teaching',121.50420,31.28200,NULL,TO_DATE('2026-07-19 10:30:00','YYYY-MM-DD HH24:MI:SS'),'连廊柱子旁' FROM DUAL
    UNION ALL SELECT 'seed-sighting-056','seed-cat-019','seed-user-vol-01','seed-area-teaching',121.50425,31.28205,NULL,TO_DATE('2026-07-24 10:40:00','YYYY-MM-DD HH24:MI:SS'),'状态良好' FROM DUAL
    UNION ALL SELECT 'seed-sighting-057','seed-cat-019','seed-user-user-02','seed-area-library',121.50630,31.28230,NULL,TO_DATE('2026-07-28 10:50:00','YYYY-MM-DD HH24:MI:SS'),'图书馆西侧' FROM DUAL
    UNION ALL SELECT 'seed-sighting-058','seed-cat-020','seed-user-user-01','seed-area-east-gate',121.50810,31.28090,NULL,TO_DATE('2026-07-20 08:30:00','YYYY-MM-DD HH24:MI:SS'),'东门入口' FROM DUAL
    UNION ALL SELECT 'seed-sighting-059','seed-cat-020','seed-user-vol-02','seed-area-east-gate',121.50805,31.28095,NULL,TO_DATE('2026-07-25 08:40:00','YYYY-MM-DD HH24:MI:SS'),'在人行道边活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-060','seed-cat-020','seed-user-user-02','seed-area-library',121.50645,31.28215,NULL,TO_DATE('2026-07-29 08:50:00','YYYY-MM-DD HH24:MI:SS'),'返回图书馆附近' FROM DUAL
    UNION ALL SELECT 'seed-sighting-061','seed-cat-021','seed-user-user-01','seed-area-west-gate',121.50390,31.28110,NULL,TO_DATE('2026-07-10 20:00:00','YYYY-MM-DD HH24:MI:SS'),'最后一次在西门附近目击' FROM DUAL
    UNION ALL SELECT 'seed-sighting-062','seed-cat-021','seed-user-vol-01','seed-area-west-gate',121.50400,31.28115,NULL,TO_DATE('2026-07-11 20:10:00','YYYY-MM-DD HH24:MI:SS'),'向校外方向移动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-063','seed-cat-021','seed-user-user-02','seed-area-west-gate',121.50395,31.28112,NULL,TO_DATE('2026-07-12 20:20:00','YYYY-MM-DD HH24:MI:SS'),'未再发现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-064','seed-cat-022','seed-user-user-01','seed-area-dorm-2',121.50900,31.28420,NULL,TO_DATE('2026-07-11 21:00:00','YYYY-MM-DD HH24:MI:SS'),'最后一次在宿舍区目击' FROM DUAL
    UNION ALL SELECT 'seed-sighting-065','seed-cat-022','seed-user-vol-02','seed-area-dorm-2',121.50910,31.28418,NULL,TO_DATE('2026-07-12 21:10:00','YYYY-MM-DD HH24:MI:SS'),'疑似向北侧离开' FROM DUAL
    UNION ALL SELECT 'seed-sighting-066','seed-cat-022','seed-user-user-02','seed-area-dorm-2',121.50905,31.28422,NULL,TO_DATE('2026-07-13 21:20:00','YYYY-MM-DD HH24:MI:SS'),'未再发现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-067','seed-cat-023','seed-user-user-01','seed-area-north-woods',121.50520,31.28320,NULL,TO_DATE('2026-07-13 22:00:00','YYYY-MM-DD HH24:MI:SS'),'最后一次在小树林目击' FROM DUAL
    UNION ALL SELECT 'seed-sighting-068','seed-cat-023','seed-user-vol-01','seed-area-north-woods',121.50510,31.28325,NULL,TO_DATE('2026-07-14 22:10:00','YYYY-MM-DD HH24:MI:SS'),'夜间活动' FROM DUAL
    UNION ALL SELECT 'seed-sighting-069','seed-cat-023','seed-user-user-02','seed-area-north-woods',121.50515,31.28322,NULL,TO_DATE('2026-07-15 22:20:00','YYYY-MM-DD HH24:MI:SS'),'未再发现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-070','seed-cat-024','seed-user-user-01','seed-area-canteen',121.50690,31.27990,NULL,TO_DATE('2026-07-14 19:00:00','YYYY-MM-DD HH24:MI:SS'),'最后一次在食堂附近目击' FROM DUAL
    UNION ALL SELECT 'seed-sighting-071','seed-cat-024','seed-user-vol-02','seed-area-canteen',121.50685,31.27995,NULL,TO_DATE('2026-07-15 19:10:00','YYYY-MM-DD HH24:MI:SS'),'食堂后门未见踪迹' FROM DUAL
    UNION ALL SELECT 'seed-sighting-072','seed-cat-024','seed-user-user-02','seed-area-canteen',121.50688,31.27992,NULL,TO_DATE('2026-07-16 19:20:00','YYYY-MM-DD HH24:MI:SS'),'未再发现' FROM DUAL
    UNION ALL SELECT 'seed-sighting-073','seed-cat-025','seed-user-user-01','seed-area-library',121.50650,31.28210,NULL,TO_DATE('2026-07-15 11:00:00','YYYY-MM-DD HH24:MI:SS'),'领养前观察' FROM DUAL
    UNION ALL SELECT 'seed-sighting-074','seed-cat-026','seed-user-vol-01','seed-area-dorm-1',121.50720,31.28410,NULL,TO_DATE('2026-07-16 11:10:00','YYYY-MM-DD HH24:MI:SS'),'领养前观察' FROM DUAL
    UNION ALL SELECT 'seed-sighting-075','seed-cat-027','seed-user-user-02','seed-area-gym',121.50480,31.27860,NULL,TO_DATE('2026-07-17 11:20:00','YYYY-MM-DD HH24:MI:SS'),'领养前观察' FROM DUAL
    UNION ALL SELECT 'seed-sighting-076','seed-cat-028','seed-user-user-01','seed-area-lake',121.50290,31.28000,NULL,TO_DATE('2026-07-18 11:30:00','YYYY-MM-DD HH24:MI:SS'),'领养前观察' FROM DUAL
    UNION ALL SELECT 'seed-sighting-077','seed-cat-029','seed-user-vol-01','seed-area-medical',121.50940,31.28150,NULL,TO_DATE('2026-07-19 13:00:00','YYYY-MM-DD HH24:MI:SS'),'医疗观察记录' FROM DUAL
    UNION ALL SELECT 'seed-sighting-078','seed-cat-030','seed-user-vol-02','seed-area-medical',121.50945,31.28155,NULL,TO_DATE('2026-07-20 13:10:00','YYYY-MM-DD HH24:MI:SS'),'医疗观察记录' FROM DUAL
    UNION ALL SELECT 'seed-sighting-079','seed-cat-025','seed-user-user-01','seed-area-library',121.50655,31.28215,NULL,TO_DATE('2026-07-22 11:00:00','YYYY-MM-DD HH24:MI:SS'),'已进入领养流程' FROM DUAL
    UNION ALL SELECT 'seed-sighting-080','seed-cat-026','seed-user-vol-01','seed-area-dorm-1',121.50725,31.28405,NULL,TO_DATE('2026-07-23 11:10:00','YYYY-MM-DD HH24:MI:SS'),'已进入领养流程' FROM DUAL
    UNION ALL SELECT 'seed-sighting-081','seed-cat-027','seed-user-user-02','seed-area-gym',121.50475,31.27865,NULL,TO_DATE('2026-07-24 11:20:00','YYYY-MM-DD HH24:MI:SS'),'已进入领养流程' FROM DUAL
    UNION ALL SELECT 'seed-sighting-082','seed-cat-028','seed-user-user-01','seed-area-lake',121.50295,31.28005,NULL,TO_DATE('2026-07-25 11:30:00','YYYY-MM-DD HH24:MI:SS'),'已进入领养流程' FROM DUAL
    UNION ALL SELECT 'seed-sighting-083','seed-cat-001','seed-user-vol-02','seed-area-library',121.50648,31.28208,NULL,TO_DATE('2026-07-26 18:00:00','YYYY-MM-DD HH24:MI:SS'),'晚间巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-084','seed-cat-002','seed-user-vol-01','seed-area-north-woods',121.50508,31.28318,NULL,TO_DATE('2026-07-27 08:00:00','YYYY-MM-DD HH24:MI:SS'),'早间巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-085','seed-cat-003','seed-user-vol-02','seed-area-canteen',121.50688,31.27992,NULL,TO_DATE('2026-07-28 12:00:00','YYYY-MM-DD HH24:MI:SS'),'午间巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-086','seed-cat-004','seed-user-vol-01','seed-area-dorm-1',121.50718,31.28408,NULL,TO_DATE('2026-07-29 07:30:00','YYYY-MM-DD HH24:MI:SS'),'晨间巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-087','seed-cat-005','seed-user-vol-02','seed-area-gym',121.50478,31.27862,NULL,TO_DATE('2026-07-30 16:00:00','YYYY-MM-DD HH24:MI:SS'),'傍晚巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-088','seed-cat-006','seed-user-vol-01','seed-area-east-gate',121.50808,31.28092,NULL,TO_DATE('2026-07-31 18:00:00','YYYY-MM-DD HH24:MI:SS'),'东门巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-089','seed-cat-007','seed-user-vol-02','seed-area-lake',121.50288,31.28002,NULL,TO_DATE('2026-08-01 09:00:00','YYYY-MM-DD HH24:MI:SS'),'湖边巡查' FROM DUAL
    UNION ALL SELECT 'seed-sighting-090','seed-cat-008','seed-user-vol-01','seed-area-teaching',121.50422,31.28202,NULL,TO_DATE('2026-08-02 10:00:00','YYYY-MM-DD HH24:MI:SS'),'教学楼巡查' FROM DUAL
) s ON (t.SIGHTINGID=s.SIGHTINGID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.USERID=s.USERID,t.AREAID=s.AREAID,t.LONGITUDE=s.LONGITUDE,t.LATITUDE=s.LATITUDE,t.PHOTOURL=s.PHOTOURL,t.SIGHTINGTIME=s.SIGHTINGTIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (SIGHTINGID,CATID,USERID,AREAID,LONGITUDE,LATITUDE,PHOTOURL,SIGHTINGTIME,REMARK)
VALUES (s.SIGHTINGID,s.CATID,s.USERID,s.AREAID,s.LONGITUDE,s.LATITUDE,s.PHOTOURL,s.SIGHTINGTIME,s.REMARK);

-- The remaining workflow rows are intentionally compact but cover every state.
-- TNR cases and status history
MERGE INTO TNR_CASES t
USING (
    SELECT 'seed-tnr-001' CASEID,'seed-cat-003' CATID,'seed-user-vet' RESPONSIBLEUSERID,'DISCOVERED' CURRENTSTATUS,'同济动物医院' HOSPITALNAME,CAST(NULL AS DATE) CAPTURETIME,CAST(NULL AS DATE) SURGERYTIME,CAST(NULL AS DATE) RELEASETIME,0 TOTALCOST FROM DUAL
    UNION ALL SELECT 'seed-tnr-002','seed-cat-004','seed-user-vet','CAPTURED','同济动物医院',TO_DATE('2026-07-03 09:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,NULL,80 FROM DUAL
    UNION ALL SELECT 'seed-tnr-003','seed-cat-005','seed-user-vet','SURGERY','校园合作医院',TO_DATE('2026-07-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-05 14:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,260 FROM DUAL
    UNION ALL SELECT 'seed-tnr-004','seed-cat-006','seed-user-vet','RECOVERING','校园合作医院',TO_DATE('2026-07-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-07 14:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,320 FROM DUAL
    UNION ALL SELECT 'seed-tnr-005','seed-cat-007','seed-user-vet','RELEASED','同济动物医院',TO_DATE('2026-07-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-09 14:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-12 10:00:00','YYYY-MM-DD HH24:MI:SS'),380 FROM DUAL
    UNION ALL SELECT 'seed-tnr-006','seed-cat-008','seed-user-vet','CANCELLED','校园合作医院',TO_DATE('2026-07-10 09:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,NULL,50 FROM DUAL
    UNION ALL SELECT 'seed-tnr-007','seed-cat-009','seed-user-vet','RELEASED','同济动物医院',TO_DATE('2026-07-12 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-13 14:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-16 10:00:00','YYYY-MM-DD HH24:MI:SS'),420 FROM DUAL
    UNION ALL SELECT 'seed-tnr-008','seed-cat-010','seed-user-vet','RECOVERING','同济动物医院',TO_DATE('2026-07-15 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-07-16 14:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,300 FROM DUAL
) s ON (t.CASEID=s.CASEID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.RESPONSIBLEUSERID=s.RESPONSIBLEUSERID,t.CURRENTSTATUS=s.CURRENTSTATUS,t.HOSPITALNAME=s.HOSPITALNAME,t.CAPTURETIME=s.CAPTURETIME,t.SURGERYTIME=s.SURGERYTIME,t.RELEASETIME=s.RELEASETIME,t.TOTALCOST=s.TOTALCOST
WHEN NOT MATCHED THEN INSERT (CASEID,CATID,RESPONSIBLEUSERID,CURRENTSTATUS,HOSPITALNAME,CAPTURETIME,SURGERYTIME,RELEASETIME,TOTALCOST)
VALUES (s.CASEID,s.CATID,s.RESPONSIBLEUSERID,s.CURRENTSTATUS,s.HOSPITALNAME,s.CAPTURETIME,s.SURGERYTIME,s.RELEASETIME,s.TOTALCOST);

MERGE INTO TNR_STATUSLOGS t
USING (
    SELECT 'seed-tnr-log-001' LOGID,'seed-tnr-001' CASEID,'DISCOVERED' FROMSTATUS,'CAPTURED' TOSTATUS,'seed-user-vet' OPERATORID,TO_DATE('2026-07-02 09:00:00','YYYY-MM-DD HH24:MI:SS') OPTIME,'已发现并安排捕捉' REMARK FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-002','seed-tnr-002','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-03 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-003','seed-tnr-002','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-04 14:00:00','YYYY-MM-DD HH24:MI:SS'),'送医手术' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-004','seed-tnr-003','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-005','seed-tnr-003','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-05 14:00:00','YYYY-MM-DD HH24:MI:SS'),'手术完成' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-006','seed-tnr-004','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-007','seed-tnr-004','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-07 14:00:00','YYYY-MM-DD HH24:MI:SS'),'手术完成' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-008','seed-tnr-004','SURGERY','RECOVERING','seed-user-vet',TO_DATE('2026-07-08 10:00:00','YYYY-MM-DD HH24:MI:SS'),'进入恢复期' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-009','seed-tnr-005','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-010','seed-tnr-005','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-09 14:00:00','YYYY-MM-DD HH24:MI:SS'),'手术完成' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-011','seed-tnr-005','SURGERY','RELEASED','seed-user-vet',TO_DATE('2026-07-12 10:00:00','YYYY-MM-DD HH24:MI:SS'),'恢复后放归' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-012','seed-tnr-006','DISCOVERED','CANCELLED','seed-user-vet',TO_DATE('2026-07-11 10:00:00','YYYY-MM-DD HH24:MI:SS'),'因状态异常取消' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-013','seed-tnr-007','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-12 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-014','seed-tnr-007','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-13 14:00:00','YYYY-MM-DD HH24:MI:SS'),'手术完成' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-015','seed-tnr-007','SURGERY','RELEASED','seed-user-vet',TO_DATE('2026-07-16 10:00:00','YYYY-MM-DD HH24:MI:SS'),'恢复后放归' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-016','seed-tnr-008','DISCOVERED','CAPTURED','seed-user-vet',TO_DATE('2026-07-15 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成捕捉' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-017','seed-tnr-008','CAPTURED','SURGERY','seed-user-vet',TO_DATE('2026-07-16 14:00:00','YYYY-MM-DD HH24:MI:SS'),'手术完成' FROM DUAL
    UNION ALL SELECT 'seed-tnr-log-018','seed-tnr-008','SURGERY','RECOVERING','seed-user-vet',TO_DATE('2026-07-17 10:00:00','YYYY-MM-DD HH24:MI:SS'),'进入恢复期' FROM DUAL
) s ON (t.LOGID=s.LOGID)
WHEN MATCHED THEN UPDATE SET t.CASEID=s.CASEID,t.FROMSTATUS=s.FROMSTATUS,t.TOSTATUS=s.TOSTATUS,t.OPERATORID=s.OPERATORID,t.OPTIME=s.OPTIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (LOGID,CASEID,FROMSTATUS,TOSTATUS,OPERATORID,OPTIME,REMARK)
VALUES (s.LOGID,s.CASEID,s.FROMSTATUS,s.TOSTATUS,s.OPERATORID,s.OPTIME,s.REMARK);

-- Medical records and reminders
MERGE INTO MED_HEALTHRECORDS t
USING (
    SELECT 'seed-health-001' RECORDID,'seed-cat-001' CATID,'VACCINATION' RECORDTYPE,'同济动物医院' HOSPITALNAME,'三联疫苗第一针' DIAGNOSIS,TO_DATE('2026-07-01','YYYY-MM-DD') RECORDDATE,TO_DATE('2026-08-01','YYYY-MM-DD') NEXTDUEDATE,CAST(NULL AS VARCHAR2(300)) ATTACHMENTURL FROM DUAL
    UNION ALL SELECT 'seed-health-002','seed-cat-002','DEWORMING','校园宠物门诊','体内驱虫',TO_DATE('2026-07-02','YYYY-MM-DD'),TO_DATE('2026-10-02','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-003','seed-cat-003','CHECKUP','同济动物医院','年度健康检查',TO_DATE('2026-07-03','YYYY-MM-DD'),TO_DATE('2027-07-03','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-004','seed-cat-004','TREATMENT','同济动物医院','轻微皮肤炎',TO_DATE('2026-07-04','YYYY-MM-DD'),TO_DATE('2026-08-04','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-005','seed-cat-005','SURGERY','校园合作医院','绝育手术',TO_DATE('2026-07-05','YYYY-MM-DD'),NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-health-006','seed-cat-006','VACCINATION','同济动物医院','狂犬疫苗',TO_DATE('2026-07-06','YYYY-MM-DD'),TO_DATE('2027-07-06','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-007','seed-cat-007','DEWORMING','校园宠物门诊','体外驱虫',TO_DATE('2026-07-07','YYYY-MM-DD'),TO_DATE('2026-10-07','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-008','seed-cat-008','CHECKUP','同济动物医院','恢复期复查',TO_DATE('2026-07-08','YYYY-MM-DD'),TO_DATE('2026-08-08','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-009','seed-cat-009','TREATMENT','同济动物医院','耳螨治疗',TO_DATE('2026-07-09','YYYY-MM-DD'),TO_DATE('2026-08-09','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-010','seed-cat-010','VACCINATION','校园宠物门诊','三联疫苗加强针',TO_DATE('2026-07-10','YYYY-MM-DD'),TO_DATE('2026-08-10','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-011','seed-cat-011','CHECKUP','同济动物医院','常规体检',TO_DATE('2026-07-11','YYYY-MM-DD'),TO_DATE('2027-07-11','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-012','seed-cat-012','EMERGENCY','同济动物医院','轻微外伤处理',TO_DATE('2026-07-12','YYYY-MM-DD'),TO_DATE('2026-07-19','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-013','seed-cat-013','DEWORMING','校园宠物门诊','体内驱虫',TO_DATE('2026-07-13','YYYY-MM-DD'),TO_DATE('2026-10-13','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-014','seed-cat-014','VACCINATION','同济动物医院','狂犬疫苗',TO_DATE('2026-07-14','YYYY-MM-DD'),TO_DATE('2027-07-14','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-015','seed-cat-015','CHECKUP','同济动物医院','年度健康检查',TO_DATE('2026-07-15','YYYY-MM-DD'),TO_DATE('2027-07-15','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-016','seed-cat-016','TREATMENT','同济动物医院','口炎治疗',TO_DATE('2026-07-16','YYYY-MM-DD'),TO_DATE('2026-08-16','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-017','seed-cat-017','SURGERY','校园合作医院','绝育手术',TO_DATE('2026-07-17','YYYY-MM-DD'),NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-health-018','seed-cat-018','DEWORMING','校园宠物门诊','体外驱虫',TO_DATE('2026-07-18','YYYY-MM-DD'),TO_DATE('2026-10-18','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-019','seed-cat-019','VACCINATION','同济动物医院','三联疫苗第一针',TO_DATE('2026-07-19','YYYY-MM-DD'),TO_DATE('2026-08-19','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-020','seed-cat-020','CHECKUP','同济动物医院','常规体检',TO_DATE('2026-07-20','YYYY-MM-DD'),TO_DATE('2027-07-20','YYYY-MM-DD'),NULL FROM DUAL
    UNION ALL SELECT 'seed-health-021','seed-cat-021','EMERGENCY','同济动物医院','失踪前外伤记录',TO_DATE('2026-07-21','YYYY-MM-DD'),NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-health-022','seed-cat-022','CHECKUP','同济动物医院','失踪前体检',TO_DATE('2026-07-22','YYYY-MM-DD'),NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-health-023','seed-cat-025','VACCINATION','同济动物医院','领养前疫苗核验',TO_DATE('2026-07-23','YYYY-MM-DD'),NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-health-024','seed-cat-026','CHECKUP','同济动物医院','领养前健康检查',TO_DATE('2026-07-24','YYYY-MM-DD'),NULL,NULL FROM DUAL
) s ON (t.RECORDID=s.RECORDID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.RECORDTYPE=s.RECORDTYPE,t.HOSPITALNAME=s.HOSPITALNAME,t.DIAGNOSIS=s.DIAGNOSIS,t.RECORDDATE=s.RECORDDATE,t.NEXTDUEDATE=s.NEXTDUEDATE,t.ATTACHMENTURL=s.ATTACHMENTURL
WHEN NOT MATCHED THEN INSERT (RECORDID,CATID,RECORDTYPE,HOSPITALNAME,DIAGNOSIS,RECORDDATE,NEXTDUEDATE,ATTACHMENTURL)
VALUES (s.RECORDID,s.CATID,s.RECORDTYPE,s.HOSPITALNAME,s.DIAGNOSIS,s.RECORDDATE,s.NEXTDUEDATE,s.ATTACHMENTURL);

-- The health-record trigger creates a reminder automatically.  Replace those
-- generated rows for seed records with the fixed reminder set below so reruns
-- remain deterministic and the UI gets all three reminder states.
DELETE FROM MED_REMINDERS WHERE RECORDID LIKE 'seed-%' OR CATID LIKE 'seed-%';

MERGE INTO MED_REMINDERS t
USING (
    SELECT 'seed-reminder-001' REMINDERID,'seed-health-001' RECORDID,'seed-cat-001' CATID,'VACCINATION' REMINDERTYPE,'seed-user-vet' RECEIVERUSERID,TO_DATE('2026-08-01','YYYY-MM-DD') REMINDERTIME,'PENDING' SENDSTATUS FROM DUAL
    UNION ALL SELECT 'seed-reminder-002','seed-health-002','seed-cat-002','DEWORMING','seed-user-vet',TO_DATE('2026-10-02','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-003','seed-health-003','seed-cat-003','CHECKUP','seed-user-vet',TO_DATE('2027-07-03','YYYY-MM-DD'),'SENT' FROM DUAL
    UNION ALL SELECT 'seed-reminder-004','seed-health-004','seed-cat-004','TREATMENT','seed-user-vet',TO_DATE('2026-08-04','YYYY-MM-DD'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-reminder-005','seed-health-006','seed-cat-006','VACCINATION','seed-user-vet',TO_DATE('2027-07-06','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-006','seed-health-007','seed-cat-007','DEWORMING','seed-user-vet',TO_DATE('2026-10-07','YYYY-MM-DD'),'SENT' FROM DUAL
    UNION ALL SELECT 'seed-reminder-007','seed-health-009','seed-cat-009','TREATMENT','seed-user-vet',TO_DATE('2026-08-09','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-008','seed-health-010','seed-cat-010','VACCINATION','seed-user-vet',TO_DATE('2026-08-10','YYYY-MM-DD'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-reminder-009','seed-health-013','seed-cat-013','DEWORMING','seed-user-vet',TO_DATE('2026-10-13','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-010','seed-health-014','seed-cat-014','VACCINATION','seed-user-vet',TO_DATE('2027-07-14','YYYY-MM-DD'),'SENT' FROM DUAL
    UNION ALL SELECT 'seed-reminder-011','seed-health-016','seed-cat-016','TREATMENT','seed-user-vet',TO_DATE('2026-08-16','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-012','seed-health-018','seed-cat-018','DEWORMING','seed-user-vet',TO_DATE('2026-10-18','YYYY-MM-DD'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-reminder-013','seed-health-019','seed-cat-019','VACCINATION','seed-user-vet',TO_DATE('2026-08-19','YYYY-MM-DD'),'PENDING' FROM DUAL
    UNION ALL SELECT 'seed-reminder-014','seed-health-020','seed-cat-020','CHECKUP','seed-user-vet',TO_DATE('2027-07-20','YYYY-MM-DD'),'SENT' FROM DUAL
    UNION ALL SELECT 'seed-reminder-015','seed-health-021','seed-cat-021','EMERGENCY','seed-user-vet',TO_DATE('2026-08-15','YYYY-MM-DD'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-reminder-016','seed-health-022','seed-cat-022','CHECKUP','seed-user-vet',TO_DATE('2026-08-20','YYYY-MM-DD'),'PENDING' FROM DUAL
) s ON (t.REMINDERID=s.REMINDERID)
WHEN MATCHED THEN UPDATE SET t.RECORDID=s.RECORDID,t.CATID=s.CATID,t.REMINDERTYPE=s.REMINDERTYPE,t.RECEIVERUSERID=s.RECEIVERUSERID,t.REMINDERTIME=s.REMINDERTIME,t.SENDSTATUS=s.SENDSTATUS
WHEN NOT MATCHED THEN INSERT (REMINDERID,RECORDID,CATID,REMINDERTYPE,RECEIVERUSERID,REMINDERTIME,SENDSTATUS)
VALUES (s.REMINDERID,s.RECORDID,s.CATID,s.REMINDERTYPE,s.RECEIVERUSERID,s.REMINDERTIME,s.SENDSTATUS);

-- Adoption applications and completed visit records
MERGE INTO ADOPT_APPLICATIONS t
USING (
    SELECT 'seed-app-001' APPLICATIONID,'seed-cat-025' CATID,'seed-user-user-01' APPLICANTUSERID,TO_DATE('2026-07-01 10:00:00','YYYY-MM-DD HH24:MI:SS') APPLYTIME,'APPROVED' CURRENTSTATUS,'seed-user-admin' REVIEWERUSERID,'SEED-AGREE-001' AGREEMENTNO,TO_DATE('2026-07-03 10:00:00','YYYY-MM-DD HH24:MI:SS') CONFIRMTIME FROM DUAL
    UNION ALL SELECT 'seed-app-002','seed-cat-026','seed-user-user-02',TO_DATE('2026-07-02 10:00:00','YYYY-MM-DD HH24:MI:SS'),'APPROVED','seed-user-admin','SEED-AGREE-002',TO_DATE('2026-07-04 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-003','seed-cat-027','seed-user-user-01',TO_DATE('2026-07-03 10:00:00','YYYY-MM-DD HH24:MI:SS'),'APPROVED','seed-user-vol-01','SEED-AGREE-003',TO_DATE('2026-07-05 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-004','seed-cat-028','seed-user-user-02',TO_DATE('2026-07-04 10:00:00','YYYY-MM-DD HH24:MI:SS'),'APPROVED','seed-user-admin','SEED-AGREE-004',TO_DATE('2026-07-06 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-005','seed-cat-015','seed-user-user-01',TO_DATE('2026-07-05 10:00:00','YYYY-MM-DD HH24:MI:SS'),'APPROVED','seed-user-vol-02','SEED-AGREE-005',TO_DATE('2026-07-07 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-006','seed-cat-016','seed-user-user-02',TO_DATE('2026-07-06 10:00:00','YYYY-MM-DD HH24:MI:SS'),'APPROVED','seed-user-admin','SEED-AGREE-006',TO_DATE('2026-07-08 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-007','seed-cat-017','seed-user-user-01',TO_DATE('2026-07-07 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-008','seed-cat-018','seed-user-user-02',TO_DATE('2026-07-08 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-009','seed-cat-019','seed-user-user-01',TO_DATE('2026-07-09 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-010','seed-cat-020','seed-user-user-02',TO_DATE('2026-07-10 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-011','seed-cat-011','seed-user-user-01',TO_DATE('2026-07-11 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-012','seed-cat-012','seed-user-user-02',TO_DATE('2026-07-12 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-013','seed-cat-013','seed-user-user-01',TO_DATE('2026-07-13 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-014','seed-cat-014','seed-user-user-02',TO_DATE('2026-07-14 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PENDING',NULL,NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-app-015','seed-cat-001','seed-user-user-01',TO_DATE('2026-07-15 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-vol-01',NULL,TO_DATE('2026-07-16 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-016','seed-cat-002','seed-user-user-02',TO_DATE('2026-07-16 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-admin',NULL,TO_DATE('2026-07-17 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-017','seed-cat-003','seed-user-blacklisted',TO_DATE('2026-07-17 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-admin',NULL,TO_DATE('2026-07-18 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-018','seed-cat-004','seed-user-user-02',TO_DATE('2026-07-18 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-vol-02',NULL,TO_DATE('2026-07-19 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-019','seed-cat-005','seed-user-user-01',TO_DATE('2026-07-19 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-admin',NULL,TO_DATE('2026-07-20 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
    UNION ALL SELECT 'seed-app-020','seed-cat-006','seed-user-blacklisted',TO_DATE('2026-07-20 10:00:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','seed-user-admin',NULL,TO_DATE('2026-07-21 10:00:00','YYYY-MM-DD HH24:MI:SS') FROM DUAL
) s ON (t.APPLICATIONID=s.APPLICATIONID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.APPLICANTUSERID=s.APPLICANTUSERID,t.APPLYTIME=s.APPLYTIME,t.CURRENTSTATUS=s.CURRENTSTATUS,t.REVIEWERUSERID=s.REVIEWERUSERID,t.AGREEMENTNO=s.AGREEMENTNO,t.CONFIRMTIME=s.CONFIRMTIME
WHEN NOT MATCHED THEN INSERT (APPLICATIONID,CATID,APPLICANTUSERID,APPLYTIME,CURRENTSTATUS,REVIEWERUSERID,AGREEMENTNO,CONFIRMTIME)
VALUES (s.APPLICATIONID,s.CATID,s.APPLICANTUSERID,s.APPLYTIME,s.CURRENTSTATUS,s.REVIEWERUSERID,s.AGREEMENTNO,s.CONFIRMTIME);

MERGE INTO ADOPT_VISITS t
USING (
    SELECT 'seed-visit-001' VISITID,'seed-app-001' APPLICATIONID,'INITIAL' VISITTYPE,TO_DATE('2026-07-04 14:00:00','YYYY-MM-DD HH24:MI:SS') VISITTIME,'seed-user-vol-01' VISITORUSERID,'居住环境合适，已完成首次回访' CONCLUSION,1 PASSFLAG FROM DUAL
    UNION ALL SELECT 'seed-visit-002','seed-app-002','INITIAL',TO_DATE('2026-07-05 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-02','家人支持领养',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-003','seed-app-003','INITIAL',TO_DATE('2026-07-06 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-01','已完成环境核验',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-004','seed-app-004','FOLLOW_UP',TO_DATE('2026-07-10 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-02','猫咪适应良好',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-005','seed-app-005','FOLLOW_UP',TO_DATE('2026-07-11 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-01','饮食和作息正常',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-006','seed-app-006','FINAL',TO_DATE('2026-07-15 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-02','完成最终回访',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-007','seed-app-001','FOLLOW_UP',TO_DATE('2026-07-20 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-01','持续观察中',1 FROM DUAL
    UNION ALL SELECT 'seed-visit-008','seed-app-002','FINAL',TO_DATE('2026-07-22 14:00:00','YYYY-MM-DD HH24:MI:SS'),'seed-user-vol-02','回访完成',1 FROM DUAL
) s ON (t.VISITID=s.VISITID)
WHEN MATCHED THEN UPDATE SET t.APPLICATIONID=s.APPLICATIONID,t.VISITTYPE=s.VISITTYPE,t.VISITTIME=s.VISITTIME,t.VISITORUSERID=s.VISITORUSERID,t.CONCLUSION=s.CONCLUSION,t.PASSFLAG=s.PASSFLAG
WHEN NOT MATCHED THEN INSERT (VISITID,APPLICATIONID,VISITTYPE,VISITTIME,VISITORUSERID,CONCLUSION,PASSFLAG)
VALUES (s.VISITID,s.APPLICATIONID,s.VISITTYPE,s.VISITTIME,s.VISITORUSERID,s.CONCLUSION,s.PASSFLAG);

-- Blacklist rows reference the applications above.
MERGE INTO USER_BLACKLIST t
USING (
     SELECT 'seed-blacklist-001' BLACKLISTID,'seed-user-blacklisted' USERID,'ABANDONMENT' REASONTYPE,'曾提交领养申请后多次弃养' REASONDETAIL,'seed-app-020' RELATEDAPPLICATIONID,'seed-user-admin' CREATEUSERID,TO_DATE('2026-07-21','YYYY-MM-DD') CREATETIME,'ACTIVE' BLACKLISTSTATUS,CAST(NULL AS DATE) RELEASETIME,CAST(NULL AS VARCHAR2(36)) RELEASEDBY FROM DUAL
    UNION ALL SELECT 'seed-blacklist-002','seed-user-user-02','FALSE_INFORMATION','申请信息与实际居住情况不符',NULL,'seed-user-admin',TO_DATE('2026-07-10','YYYY-MM-DD'),'ACTIVE',NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-blacklist-003','seed-user-vol-03','OTHER','历史违规记录已整改',NULL,'seed-user-admin',TO_DATE('2026-06-01','YYYY-MM-DD'),'RELEASED',TO_DATE('2026-07-01','YYYY-MM-DD'),'seed-user-admin' FROM DUAL
) s ON (t.BLACKLISTID=s.BLACKLISTID)
WHEN MATCHED THEN UPDATE SET t.USERID=s.USERID,t.REASONTYPE=s.REASONTYPE,t.REASONDETAIL=s.REASONDETAIL,t.RELATEDAPPLICATIONID=s.RELATEDAPPLICATIONID,t.CREATEUSERID=s.CREATEUSERID,t.CREATETIME=s.CREATETIME,t.BLACKLISTSTATUS=s.BLACKLISTSTATUS,t.RELEASETIME=s.RELEASETIME,t.RELEASEDBY=s.RELEASEDBY
WHEN NOT MATCHED THEN INSERT (BLACKLISTID,USERID,REASONTYPE,REASONDETAIL,RELATEDAPPLICATIONID,CREATEUSERID,CREATETIME,BLACKLISTSTATUS,RELEASETIME,RELEASEDBY)
VALUES (s.BLACKLISTID,s.USERID,s.REASONTYPE,s.REASONDETAIL,s.RELATEDAPPLICATIONID,s.CREATEUSERID,s.CREATETIME,s.BLACKLISTSTATUS,s.RELEASETIME,s.RELEASEDBY);

-- Volunteers, shifts, check-ins, credits and handovers
MERGE INTO VOL_VOLUNTEERS t
USING (
    SELECT 'seed-volunteer-001' VOLUNTEERID,'seed-user-vol-01' USERID,TO_DATE('2026-03-01','YYYY-MM-DD') JOINDATE,86 SERVICESCORE,'L2' CREDITLEVEL,'ACTIVE' ACTIVESTATUS,'2028' GRADUATIONYEAR FROM DUAL
    UNION ALL SELECT 'seed-volunteer-002','seed-user-vol-02',TO_DATE('2026-03-15','YYYY-MM-DD'),62,'L2','ACTIVE','2027' FROM DUAL
    UNION ALL SELECT 'seed-volunteer-003','seed-user-vol-03',TO_DATE('2026-04-01','YYYY-MM-DD'),25,'L1','ACTIVE','2029' FROM DUAL
) s ON (t.VOLUNTEERID=s.VOLUNTEERID)
WHEN MATCHED THEN UPDATE SET t.USERID=s.USERID,t.JOINDATE=s.JOINDATE,t.SERVICESCORE=s.SERVICESCORE,t.CREDITLEVEL=s.CREDITLEVEL,t.ACTIVESTATUS=s.ACTIVESTATUS,t.GRADUATIONYEAR=s.GRADUATIONYEAR
WHEN NOT MATCHED THEN INSERT (VOLUNTEERID,USERID,JOINDATE,SERVICESCORE,CREDITLEVEL,ACTIVESTATUS,GRADUATIONYEAR)
VALUES (s.VOLUNTEERID,s.USERID,s.JOINDATE,s.SERVICESCORE,s.CREDITLEVEL,s.ACTIVESTATUS,s.GRADUATIONYEAR);

MERGE INTO VOL_SHIFTS t
USING (
    SELECT 'seed-shift-001' SHIFTID,'seed-volunteer-001' VOLUNTEERID,'seed-point-001' POINTID,'seed-volunteer-002' BACKUPVOLUNTEERID,TO_DATE('2026-08-01 08:00:00','YYYY-MM-DD HH24:MI:SS') PLANSTARTTIME,TO_DATE('2026-08-01 10:00:00','YYYY-MM-DD HH24:MI:SS') PLANENDTIME,'COMPLETED' SHIFTSTATUS FROM DUAL
    UNION ALL SELECT 'seed-shift-002','seed-volunteer-002','seed-point-003','seed-volunteer-001',TO_DATE('2026-08-01 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-01 11:00:00','YYYY-MM-DD HH24:MI:SS'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-shift-003','seed-volunteer-003','seed-point-005','seed-volunteer-001',TO_DATE('2026-08-02 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-02 10:00:00','YYYY-MM-DD HH24:MI:SS'),'ASSIGNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-004','seed-volunteer-001','seed-point-007','seed-volunteer-002',TO_DATE('2026-08-02 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-02 11:00:00','YYYY-MM-DD HH24:MI:SS'),'IN_PROGRESS' FROM DUAL
    UNION ALL SELECT 'seed-shift-005','seed-volunteer-002','seed-point-010','seed-volunteer-003',TO_DATE('2026-08-03 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-03 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PLANNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-006','seed-volunteer-003','seed-point-013','seed-volunteer-001',TO_DATE('2026-08-03 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-03 11:00:00','YYYY-MM-DD HH24:MI:SS'),'MISSED' FROM DUAL
    UNION ALL SELECT 'seed-shift-007','seed-volunteer-001','seed-point-016','seed-volunteer-002',TO_DATE('2026-08-04 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-04 10:00:00','YYYY-MM-DD HH24:MI:SS'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-shift-008','seed-volunteer-002','seed-point-002','seed-volunteer-003',TO_DATE('2026-08-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-04 11:00:00','YYYY-MM-DD HH24:MI:SS'),'ASSIGNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-009','seed-volunteer-003','seed-point-004','seed-volunteer-001',TO_DATE('2026-08-05 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-05 10:00:00','YYYY-MM-DD HH24:MI:SS'),'IN_PROGRESS' FROM DUAL
    UNION ALL SELECT 'seed-shift-010','seed-volunteer-001','seed-point-008','seed-volunteer-002',TO_DATE('2026-08-05 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-05 11:00:00','YYYY-MM-DD HH24:MI:SS'),'PLANNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-011','seed-volunteer-002','seed-point-011','seed-volunteer-003',TO_DATE('2026-08-06 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-06 10:00:00','YYYY-MM-DD HH24:MI:SS'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-shift-012','seed-volunteer-003','seed-point-014','seed-volunteer-001',TO_DATE('2026-08-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-06 11:00:00','YYYY-MM-DD HH24:MI:SS'),'ASSIGNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-013','seed-volunteer-001','seed-point-017','seed-volunteer-002',TO_DATE('2026-08-07 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-07 10:00:00','YYYY-MM-DD HH24:MI:SS'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-shift-014','seed-volunteer-002','seed-point-018','seed-volunteer-003',TO_DATE('2026-08-07 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-07 11:00:00','YYYY-MM-DD HH24:MI:SS'),'IN_PROGRESS' FROM DUAL
    UNION ALL SELECT 'seed-shift-015','seed-volunteer-003','seed-point-006','seed-volunteer-001',TO_DATE('2026-08-08 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-08 10:00:00','YYYY-MM-DD HH24:MI:SS'),'PLANNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-016','seed-volunteer-001','seed-point-009','seed-volunteer-002',TO_DATE('2026-08-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-08 11:00:00','YYYY-MM-DD HH24:MI:SS'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-shift-017','seed-volunteer-002','seed-point-012','seed-volunteer-003',TO_DATE('2026-08-09 08:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-09 10:00:00','YYYY-MM-DD HH24:MI:SS'),'ASSIGNED' FROM DUAL
    UNION ALL SELECT 'seed-shift-018','seed-volunteer-003','seed-point-015','seed-volunteer-001',TO_DATE('2026-08-09 09:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-09 11:00:00','YYYY-MM-DD HH24:MI:SS'),'MISSED' FROM DUAL
) s ON (t.SHIFTID=s.SHIFTID)
WHEN MATCHED THEN UPDATE SET t.VOLUNTEERID=s.VOLUNTEERID,t.POINTID=s.POINTID,t.BACKUPVOLUNTEERID=s.BACKUPVOLUNTEERID,t.PLANSTARTTIME=s.PLANSTARTTIME,t.PLANENDTIME=s.PLANENDTIME,t.SHIFTSTATUS=s.SHIFTSTATUS
WHEN NOT MATCHED THEN INSERT (SHIFTID,VOLUNTEERID,POINTID,BACKUPVOLUNTEERID,PLANSTARTTIME,PLANENDTIME,SHIFTSTATUS)
VALUES (s.SHIFTID,s.VOLUNTEERID,s.POINTID,s.BACKUPVOLUNTEERID,s.PLANSTARTTIME,s.PLANENDTIME,s.SHIFTSTATUS);

MERGE INTO VOL_CHECKINS t
USING (
    SELECT 'seed-checkin-001' CHECKINID,'seed-shift-001' SHIFTID,TO_DATE('2026-08-01 08:20:00','YYYY-MM-DD HH24:MI:SS') CHECKINTIME,121.50650 LONGITUDE,31.28210 LATITUDE,'https://placehold.co/640x480/png?text=checkin-001' PHOTOURL,3 DISTANCEMETERS,'CHECKED_IN' CHECKINSTATUS FROM DUAL
    UNION ALL SELECT 'seed-checkin-002','seed-shift-002',TO_DATE('2026-08-01 09:20:00','YYYY-MM-DD HH24:MI:SS'),121.50520,31.28320,'https://placehold.co/640x480/png?text=checkin-002',5,'CHECKED_IN' FROM DUAL
    UNION ALL SELECT 'seed-checkin-003','seed-shift-007',TO_DATE('2026-08-04 08:30:00','YYYY-MM-DD HH24:MI:SS'),121.50460,31.28220,'https://placehold.co/640x480/png?text=checkin-003',8,'LATE' FROM DUAL
    UNION ALL SELECT 'seed-checkin-004','seed-shift-011',TO_DATE('2026-08-06 08:10:00','YYYY-MM-DD HH24:MI:SS'),121.50640,31.27980,'https://placehold.co/640x480/png?text=checkin-004',4,'CHECKED_IN' FROM DUAL
    UNION ALL SELECT 'seed-checkin-005','seed-shift-013',TO_DATE('2026-08-07 08:15:00','YYYY-MM-DD HH24:MI:SS'),121.50940,31.28150,'https://placehold.co/640x480/png?text=checkin-005',6,'CHECKED_IN' FROM DUAL
    UNION ALL SELECT 'seed-checkin-006','seed-shift-016',TO_DATE('2026-08-08 09:20:00','YYYY-MM-DD HH24:MI:SS'),121.50900,31.28420,'https://placehold.co/640x480/png?text=checkin-006',7,'LATE' FROM DUAL
) s ON (t.CHECKINID=s.CHECKINID)
WHEN MATCHED THEN UPDATE SET t.SHIFTID=s.SHIFTID,t.CHECKINTIME=s.CHECKINTIME,t.LONGITUDE=s.LONGITUDE,t.LATITUDE=s.LATITUDE,t.PHOTOURL=s.PHOTOURL,t.DISTANCEMETERS=s.DISTANCEMETERS,t.CHECKINSTATUS=s.CHECKINSTATUS
WHEN NOT MATCHED THEN INSERT (CHECKINID,SHIFTID,CHECKINTIME,LONGITUDE,LATITUDE,PHOTOURL,DISTANCEMETERS,CHECKINSTATUS)
VALUES (s.CHECKINID,s.SHIFTID,s.CHECKINTIME,s.LONGITUDE,s.LATITUDE,s.PHOTOURL,s.DISTANCEMETERS,s.CHECKINSTATUS);

MERGE INTO VOL_CREDITLOGS t
USING (
    SELECT 'seed-credit-001' CREDITLOGID,'seed-volunteer-001' VOLUNTEERID,'CHECKIN' SOURCETYPE,'seed-checkin-001' SOURCEID,10 SCORECHANGE,'L2' CREDITLEVELAFTER,TO_DATE('2026-08-01','YYYY-MM-DD') CREATETIME,'完成图书馆投喂' REMARK FROM DUAL
    UNION ALL SELECT 'seed-credit-002','seed-volunteer-002','CHECKIN','seed-checkin-002',10,'L2',TO_DATE('2026-08-01','YYYY-MM-DD'),'完成小树林投喂' FROM DUAL
    UNION ALL SELECT 'seed-credit-003','seed-volunteer-001','CHECKIN','seed-checkin-003',6,'L2',TO_DATE('2026-08-04','YYYY-MM-DD'),'迟到签到' FROM DUAL
    UNION ALL SELECT 'seed-credit-004','seed-volunteer-002','CHECKIN','seed-checkin-004',10,'L2',TO_DATE('2026-08-06','YYYY-MM-DD'),'完成食堂投喂' FROM DUAL
    UNION ALL SELECT 'seed-credit-005','seed-volunteer-001','CHECKIN','seed-checkin-005',10,'L2',TO_DATE('2026-08-07','YYYY-MM-DD'),'完成校医院巡查' FROM DUAL
    UNION ALL SELECT 'seed-credit-006','seed-volunteer-002','CHECKIN','seed-checkin-006',6,'L2',TO_DATE('2026-08-08','YYYY-MM-DD'),'迟到签到' FROM DUAL
) s ON (t.CREDITLOGID=s.CREDITLOGID)
WHEN MATCHED THEN UPDATE SET t.VOLUNTEERID=s.VOLUNTEERID,t.SOURCETYPE=s.SOURCETYPE,t.SOURCEID=s.SOURCEID,t.SCORECHANGE=s.SCORECHANGE,t.CREDITLEVELAFTER=s.CREDITLEVELAFTER,t.CREATETIME=s.CREATETIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (CREDITLOGID,VOLUNTEERID,SOURCETYPE,SOURCEID,SCORECHANGE,CREDITLEVELAFTER,CREATETIME,REMARK)
VALUES (s.CREDITLOGID,s.VOLUNTEERID,s.SOURCETYPE,s.SOURCEID,s.SCORECHANGE,s.CREDITLEVELAFTER,s.CREATETIME,s.REMARK);

MERGE INTO VOL_HANDOVERS t
USING (
    SELECT 'seed-handover-001' HANDOVERID,'seed-volunteer-001' FROMVOLUNTEERID,'seed-volunteer-002' TOVOLUNTEERID,'SHIFT' HANDOVERTYPE,'SHIFT' RELATEDTYPE,'seed-shift-004' RELATEDID,TO_DATE('2026-08-02 07:00:00','YYYY-MM-DD HH24:MI:SS') APPLYTIME,CAST(NULL AS DATE) CONFIRMTIME,'PENDING' HANDOVERSTATUS,'临时有课，请协助完成投喂' REMARK FROM DUAL
    UNION ALL SELECT 'seed-handover-002','seed-volunteer-002','seed-volunteer-003','SHIFT','SHIFT','seed-shift-005',TO_DATE('2026-08-03 07:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-03 07:30:00','YYYY-MM-DD HH24:MI:SS'),'CONFIRMED','已确认交接' FROM DUAL
    UNION ALL SELECT 'seed-handover-003','seed-volunteer-003','seed-volunteer-001','SHIFT','SHIFT','seed-shift-006',TO_DATE('2026-08-03 07:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-03 07:30:00','YYYY-MM-DD HH24:MI:SS'),'REJECTED','接收方当日无法到场' FROM DUAL
    UNION ALL SELECT 'seed-handover-004','seed-volunteer-001','seed-volunteer-002','SHIFT','SHIFT','seed-shift-010',TO_DATE('2026-08-05 07:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-05 07:30:00','YYYY-MM-DD HH24:MI:SS'),'CANCELLED','发起方自行撤销' FROM DUAL
    UNION ALL SELECT 'seed-handover-005','seed-volunteer-002','seed-volunteer-003','SHIFT','SHIFT','seed-shift-014',TO_DATE('2026-08-07 07:00:00','YYYY-MM-DD HH24:MI:SS'),NULL,'PENDING','猫窝维护冲突' FROM DUAL
    UNION ALL SELECT 'seed-handover-006','seed-volunteer-003','seed-volunteer-001','SHIFT','SHIFT','seed-shift-015',TO_DATE('2026-08-08 07:00:00','YYYY-MM-DD HH24:MI:SS'),TO_DATE('2026-08-08 07:20:00','YYYY-MM-DD HH24:MI:SS'),'CONFIRMED','已完成接收' FROM DUAL
) s ON (t.HANDOVERID=s.HANDOVERID)
WHEN MATCHED THEN UPDATE SET t.FROMVOLUNTEERID=s.FROMVOLUNTEERID,t.TOVOLUNTEERID=s.TOVOLUNTEERID,t.HANDOVERTYPE=s.HANDOVERTYPE,t.RELATEDTYPE=s.RELATEDTYPE,t.RELATEDID=s.RELATEDID,t.APPLYTIME=s.APPLYTIME,t.CONFIRMTIME=s.CONFIRMTIME,t.HANDOVERSTATUS=s.HANDOVERSTATUS,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (HANDOVERID,FROMVOLUNTEERID,TOVOLUNTEERID,HANDOVERTYPE,RELATEDTYPE,RELATEDID,APPLYTIME,CONFIRMTIME,HANDOVERSTATUS,REMARK)
VALUES (s.HANDOVERID,s.FROMVOLUNTEERID,s.TOVOLUNTEERID,s.HANDOVERTYPE,s.RELATEDTYPE,s.RELATEDID,s.APPLYTIME,s.CONFIRMTIME,s.HANDOVERSTATUS,s.REMARK);

-- Missing alerts, emergencies and nest maintenance
MERGE INTO CAT_MISSINGALERTS t
USING (
    SELECT 'seed-alert-001' ALERTID,'seed-cat-021' CATID,'seed-sighting-063' LASTSIGHTINGID,TO_DATE('2026-07-12 20:20:00','YYYY-MM-DD HH24:MI:SS') LASTSIGHTINGTIME,3 THRESHOLDDAYS,TO_DATE('2026-07-15 09:00:00','YYYY-MM-DD HH24:MI:SS') ALERTTIME,'PROCESSING' ALERTSTATUS,'seed-user-vol-01' HANDLERUSERID,CAST(NULL AS DATE) CLOSETIME,'持续寻找中' REMARK FROM DUAL
    UNION ALL SELECT 'seed-alert-002','seed-cat-022','seed-sighting-066',TO_DATE('2026-07-13 21:20:00','YYYY-MM-DD HH24:MI:SS'),3,TO_DATE('2026-07-16 09:00:00','YYYY-MM-DD HH24:MI:SS'),'FOUND','seed-user-vol-02',TO_DATE('2026-07-18 10:00:00','YYYY-MM-DD HH24:MI:SS'),'已在宿舍区找回' FROM DUAL
    UNION ALL SELECT 'seed-alert-003','seed-cat-023','seed-sighting-069',TO_DATE('2026-07-15 22:20:00','YYYY-MM-DD HH24:MI:SS'),3,TO_DATE('2026-07-18 09:00:00','YYYY-MM-DD HH24:MI:SS'),'CLOSED','seed-user-vol-01',TO_DATE('2026-07-21 10:00:00','YYYY-MM-DD HH24:MI:SS'),'核查后关闭' FROM DUAL
    UNION ALL SELECT 'seed-alert-004','seed-cat-024','seed-sighting-072',TO_DATE('2026-07-16 19:20:00','YYYY-MM-DD HH24:MI:SS'),3,TO_DATE('2026-07-19 09:00:00','YYYY-MM-DD HH24:MI:SS'),'PROCESSING','seed-user-vol-03',NULL,'需要继续扩大搜索范围' FROM DUAL
) s ON (t.ALERTID=s.ALERTID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.LASTSIGHTINGID=s.LASTSIGHTINGID,t.LASTSIGHTINGTIME=s.LASTSIGHTINGTIME,t.THRESHOLDDAYS=s.THRESHOLDDAYS,t.ALERTTIME=s.ALERTTIME,t.ALERTSTATUS=s.ALERTSTATUS,t.HANDLERUSERID=s.HANDLERUSERID,t.CLOSETIME=s.CLOSETIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (ALERTID,CATID,LASTSIGHTINGID,LASTSIGHTINGTIME,THRESHOLDDAYS,ALERTTIME,ALERTSTATUS,HANDLERUSERID,CLOSETIME,REMARK)
VALUES (s.ALERTID,s.CATID,s.LASTSIGHTINGID,s.LASTSIGHTINGTIME,s.THRESHOLDDAYS,s.ALERTTIME,s.ALERTSTATUS,s.HANDLERUSERID,s.CLOSETIME,s.REMARK);

MERGE INTO EMERGENCY_REPORTS t
USING (
    SELECT 'seed-emergency-001' REPORTID,'seed-user-user-01' REPORTERUSERID,'seed-area-library' AREAID,'CAT' ANIMALTYPE,'https://placehold.co/640x480/png?text=emergency-001' PHOTOURL,121.50650 LONGITUDE,31.28210 LATITUDE,TO_DATE('2026-07-05 12:00:00','YYYY-MM-DD HH24:MI:SS') REPORTTIME,'HIGH' URGENCYLEVEL,'RESOLVED' PROCESSSTATUS,'seed-user-vol-01' HANDLERUSERID,'已完成现场处理' PROCESSRESULT FROM DUAL
    UNION ALL SELECT 'seed-emergency-002','seed-user-user-02','seed-area-north-woods','CAT',NULL,121.50520,31.28320,TO_DATE('2026-07-06 12:00:00','YYYY-MM-DD HH24:MI:SS'),'MEDIUM','PROCESSING','seed-user-vol-02','已安排志愿者巡查' FROM DUAL
    UNION ALL SELECT 'seed-emergency-003','seed-user-vol-01','seed-area-east-gate','CAT',NULL,121.50810,31.28090,TO_DATE('2026-07-07 12:00:00','YYYY-MM-DD HH24:MI:SS'),'CRITICAL','CLOSED','seed-user-admin','已送医并关闭报告' FROM DUAL
    UNION ALL SELECT 'seed-emergency-004','seed-user-user-01','seed-area-dorm-1','CAT',NULL,121.50720,31.28410,TO_DATE('2026-07-08 12:00:00','YYYY-MM-DD HH24:MI:SS'),'LOW','SUBMITTED',NULL,NULL FROM DUAL
    UNION ALL SELECT 'seed-emergency-005','seed-user-user-02','seed-area-canteen','DOG',NULL,121.50690,31.27990,TO_DATE('2026-07-09 12:00:00','YYYY-MM-DD HH24:MI:SS'),'MEDIUM','ASSIGNED','seed-user-vol-03',NULL FROM DUAL
    UNION ALL SELECT 'seed-emergency-006','seed-user-vol-02','seed-area-gym','CAT',NULL,121.50480,31.27860,TO_DATE('2026-07-10 12:00:00','YYYY-MM-DD HH24:MI:SS'),'HIGH','PROCESSING','seed-user-vol-01','等待医疗反馈' FROM DUAL
    UNION ALL SELECT 'seed-emergency-007','seed-user-user-01','seed-area-lake','CAT',NULL,121.50290,31.28000,TO_DATE('2026-07-11 12:00:00','YYYY-MM-DD HH24:MI:SS'),'LOW','RESOLVED','seed-user-vol-02','确认无明显伤情' FROM DUAL
    UNION ALL SELECT 'seed-emergency-008','seed-user-user-02','seed-area-teaching','OTHER',NULL,121.50420,31.28200,TO_DATE('2026-07-12 12:00:00','YYYY-MM-DD HH24:MI:SS'),'MEDIUM','CLOSED','seed-user-admin','已转交相关部门' FROM DUAL
    UNION ALL SELECT 'seed-emergency-009','seed-user-vol-03','seed-area-medical','CAT',NULL,121.50940,31.28150,TO_DATE('2026-07-13 12:00:00','YYYY-MM-DD HH24:MI:SS'),'HIGH','RESOLVED','seed-user-vet','已完成检查' FROM DUAL
    UNION ALL SELECT 'seed-emergency-010','seed-user-user-01','seed-area-west-gate','CAT',NULL,121.50390,31.28110,TO_DATE('2026-07-14 12:00:00','YYYY-MM-DD HH24:MI:SS'),'CRITICAL','SUBMITTED',NULL,NULL FROM DUAL
) s ON (t.REPORTID=s.REPORTID)
WHEN MATCHED THEN UPDATE SET t.REPORTERUSERID=s.REPORTERUSERID,t.AREAID=s.AREAID,t.ANIMALTYPE=s.ANIMALTYPE,t.PHOTOURL=s.PHOTOURL,t.LONGITUDE=s.LONGITUDE,t.LATITUDE=s.LATITUDE,t.REPORTTIME=s.REPORTTIME,t.URGENCYLEVEL=s.URGENCYLEVEL,t.PROCESSSTATUS=s.PROCESSSTATUS,t.HANDLERUSERID=s.HANDLERUSERID,t.PROCESSRESULT=s.PROCESSRESULT
WHEN NOT MATCHED THEN INSERT (REPORTID,REPORTERUSERID,AREAID,ANIMALTYPE,PHOTOURL,LONGITUDE,LATITUDE,REPORTTIME,URGENCYLEVEL,PROCESSSTATUS,HANDLERUSERID,PROCESSRESULT)
VALUES (s.REPORTID,s.REPORTERUSERID,s.AREAID,s.ANIMALTYPE,s.PHOTOURL,s.LONGITUDE,s.LATITUDE,s.REPORTTIME,s.URGENCYLEVEL,s.PROCESSSTATUS,s.HANDLERUSERID,s.PROCESSRESULT);

MERGE INTO NEST_MAINTENANCERECORDS t
USING (
    SELECT 'seed-maint-001' MAINTENANCEID,'seed-point-002' POINTID,'INSULATION_BOX' MATERIALTYPE,TO_DATE('2026-07-03 09:00:00','YYYY-MM-DD HH24:MI:SS') CHECKTIME,'SUNNY' WEATHERCONDITION,'MINOR' DAMAGELEVEL,'REPLACE' ACTIONTYPE,'seed-user-vol-01' OPERATORUSERID,TO_DATE('2026-08-03 09:00:00','YYYY-MM-DD HH24:MI:SS') NEXTCHECKTIME,'更换保温垫' REMARK FROM DUAL
    UNION ALL SELECT 'seed-maint-002','seed-point-004','FOOD_BOWL',TO_DATE('2026-07-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),'CLOUDY','NONE','CLEAN','seed-user-vol-02',TO_DATE('2026-08-04 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成清洁' FROM DUAL
    UNION ALL SELECT 'seed-maint-003','seed-point-008','WATER_BOWL',TO_DATE('2026-07-05 09:00:00','YYYY-MM-DD HH24:MI:SS'),'RAINY','MINOR','REPAIR','seed-user-vol-03',TO_DATE('2026-08-05 09:00:00','YYYY-MM-DD HH24:MI:SS'),'修复漏水' FROM DUAL
    UNION ALL SELECT 'seed-maint-004','seed-point-011','FOOD_BOWL',TO_DATE('2026-07-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),'SUNNY','NONE','CLEAN','seed-user-vol-01',TO_DATE('2026-08-06 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成清洁' FROM DUAL
    UNION ALL SELECT 'seed-maint-005','seed-point-014','INSULATION_BOX',TO_DATE('2026-07-07 09:00:00','YYYY-MM-DD HH24:MI:SS'),'CLOUDY','MAJOR','REPAIR','seed-user-vol-02',TO_DATE('2026-07-14 09:00:00','YYYY-MM-DD HH24:MI:SS'),'结构加固' FROM DUAL
    UNION ALL SELECT 'seed-maint-006','seed-point-018','OTHER',TO_DATE('2026-07-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),'RAINY','MINOR','REPLACE','seed-user-vol-03',TO_DATE('2026-08-08 09:00:00','YYYY-MM-DD HH24:MI:SS'),'更换防雨布' FROM DUAL
    UNION ALL SELECT 'seed-maint-007','seed-point-002','WATER_BOWL',TO_DATE('2026-07-09 09:00:00','YYYY-MM-DD HH24:MI:SS'),'SUNNY','NONE','CLEAN','seed-user-vol-01',TO_DATE('2026-08-09 09:00:00','YYYY-MM-DD HH24:MI:SS'),'补充饮水' FROM DUAL
    UNION ALL SELECT 'seed-maint-008','seed-point-004','FOOD_BOWL',TO_DATE('2026-07-10 09:00:00','YYYY-MM-DD HH24:MI:SS'),'CLOUDY','MINOR','CLEAN','seed-user-vol-02',TO_DATE('2026-08-10 09:00:00','YYYY-MM-DD HH24:MI:SS'),'清理残余食物' FROM DUAL
    UNION ALL SELECT 'seed-maint-009','seed-point-008','INSULATION_BOX',TO_DATE('2026-07-11 09:00:00','YYYY-MM-DD HH24:MI:SS'),'SUNNY','NONE','CLEAN','seed-user-vol-03',TO_DATE('2026-08-11 09:00:00','YYYY-MM-DD HH24:MI:SS'),'完成检查' FROM DUAL
    UNION ALL SELECT 'seed-maint-010','seed-point-011','WATER_BOWL',TO_DATE('2026-07-12 09:00:00','YYYY-MM-DD HH24:MI:SS'),'CLOUDY','MINOR','REPAIR','seed-user-vol-01',TO_DATE('2026-08-12 09:00:00','YYYY-MM-DD HH24:MI:SS'),'修复支架' FROM DUAL
) s ON (t.MAINTENANCEID=s.MAINTENANCEID)
WHEN MATCHED THEN UPDATE SET t.POINTID=s.POINTID,t.MATERIALTYPE=s.MATERIALTYPE,t.CHECKTIME=s.CHECKTIME,t.WEATHERCONDITION=s.WEATHERCONDITION,t.DAMAGELEVEL=s.DAMAGELEVEL,t.ACTIONTYPE=s.ACTIONTYPE,t.OPERATORUSERID=s.OPERATORUSERID,t.NEXTCHECKTIME=s.NEXTCHECKTIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (MAINTENANCEID,POINTID,MATERIALTYPE,CHECKTIME,WEATHERCONDITION,DAMAGELEVEL,ACTIONTYPE,OPERATORUSERID,NEXTCHECKTIME,REMARK)
VALUES (s.MAINTENANCEID,s.POINTID,s.MATERIALTYPE,s.CHECKTIME,s.WEATHERCONDITION,s.DAMAGELEVEL,s.ACTIONTYPE,s.OPERATORUSERID,s.NEXTCHECKTIME,s.REMARK);

-- Naming votes and cat matching records
MERGE INTO VOTE_NAMINGCANDIDATES t
USING (
    SELECT 'seed-candidate-001' CANDIDATEID,'seed-cat-001' CATID,'小花' CANDIDATENAME,'seed-user-user-01' PROPOSERUSERID,3 VOTECOUNT,TO_DATE('2026-09-01','YYYY-MM-DD') DEADLINE,0 WINFLAG FROM DUAL
    UNION ALL SELECT 'seed-candidate-002','seed-cat-001','花生','seed-user-user-02',2,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-003','seed-cat-001','花卷','seed-user-vol-01',5,TO_DATE('2026-09-01','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-candidate-004','seed-cat-001','卷卷','seed-user-vol-02',1,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-005','seed-cat-002','芝麻糊','seed-user-user-01',4,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-006','seed-cat-002','黑豆','seed-user-user-02',6,TO_DATE('2026-09-01','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-candidate-007','seed-cat-002','墨玉','seed-user-vol-01',2,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-008','seed-cat-002','小黑','seed-user-vol-02',1,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-009','seed-cat-003','奶酪','seed-user-user-01',3,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-010','seed-cat-003','糖糖','seed-user-user-02',7,TO_DATE('2026-09-01','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-candidate-011','seed-cat-003','小甜','seed-user-vol-01',1,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-candidate-012','seed-cat-003','云朵','seed-user-vol-02',2,TO_DATE('2026-09-01','YYYY-MM-DD'),0 FROM DUAL
) s ON (t.CANDIDATEID=s.CANDIDATEID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.CANDIDATENAME=s.CANDIDATENAME,t.PROPOSERUSERID=s.PROPOSERUSERID,t.VOTECOUNT=s.VOTECOUNT,t.DEADLINE=s.DEADLINE,t.WINFLAG=s.WINFLAG
WHEN NOT MATCHED THEN INSERT (CANDIDATEID,CATID,CANDIDATENAME,PROPOSERUSERID,VOTECOUNT,DEADLINE,WINFLAG)
VALUES (s.CANDIDATEID,s.CATID,s.CANDIDATENAME,s.PROPOSERUSERID,s.VOTECOUNT,s.DEADLINE,s.WINFLAG);

MERGE INTO VOTE_NAMINGRECORDS t
USING (
    SELECT 'seed-vote-001' RECORDID,'seed-candidate-001' CANDIDATEID,'seed-user-user-01' VOTERUSERID,TO_DATE('2026-08-01','YYYY-MM-DD') VOTETIME,'127.0.0.1' CLIENTIP,'VALID' VOTESTATUS FROM DUAL
    UNION ALL SELECT 'seed-vote-002','seed-candidate-001','seed-user-user-02',TO_DATE('2026-08-02','YYYY-MM-DD'),'127.0.0.2','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-003','seed-candidate-002','seed-user-vol-01',TO_DATE('2026-08-03','YYYY-MM-DD'),'127.0.0.3','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-004','seed-candidate-003','seed-user-vol-02',TO_DATE('2026-08-04','YYYY-MM-DD'),'127.0.0.4','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-005','seed-candidate-005','seed-user-user-01',TO_DATE('2026-08-05','YYYY-MM-DD'),'127.0.0.1','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-006','seed-candidate-006','seed-user-user-02',TO_DATE('2026-08-06','YYYY-MM-DD'),'127.0.0.2','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-007','seed-candidate-006','seed-user-vol-01',TO_DATE('2026-08-07','YYYY-MM-DD'),'127.0.0.3','VALID' FROM DUAL
    UNION ALL SELECT 'seed-vote-008','seed-candidate-010','seed-user-vol-02',TO_DATE('2026-08-08','YYYY-MM-DD'),'127.0.0.4','VALID' FROM DUAL
) s ON (t.RECORDID=s.RECORDID)
WHEN MATCHED THEN UPDATE SET t.CANDIDATEID=s.CANDIDATEID,t.VOTERUSERID=s.VOTERUSERID,t.VOTETIME=s.VOTETIME,t.CLIENTIP=s.CLIENTIP,t.VOTESTATUS=s.VOTESTATUS
WHEN NOT MATCHED THEN INSERT (RECORDID,CANDIDATEID,VOTERUSERID,VOTETIME,CLIENTIP,VOTESTATUS)
VALUES (s.RECORDID,s.CANDIDATEID,s.VOTERUSERID,s.VOTETIME,s.CLIENTIP,s.VOTESTATUS);

MERGE INTO CAT_MATCHRECORDS t
USING (
    SELECT 'seed-match-001' MATCHID,'seed-photo-001' SOURCEPHOTOID,'seed-cat-001' CANDIDATECATID,98.20 SIMILARITYSCORE,1 RANKNO,'CONFIRMED' CONFIRMSTATUS,'seed-user-admin' CONFIRMUSERID FROM DUAL
    UNION ALL SELECT 'seed-match-002','seed-photo-031','seed-cat-001',96.40,2,'PENDING',NULL FROM DUAL
    UNION ALL SELECT 'seed-match-003','seed-photo-002','seed-cat-002',97.10,1,'CONFIRMED','seed-user-vol-01' FROM DUAL
    UNION ALL SELECT 'seed-match-004','seed-photo-003','seed-cat-003',91.50,1,'REJECTED','seed-user-admin' FROM DUAL
    UNION ALL SELECT 'seed-match-005','seed-photo-004','seed-cat-004',88.80,1,'PENDING',NULL FROM DUAL
    UNION ALL SELECT 'seed-match-006','seed-photo-005','seed-cat-005',93.60,1,'CONFIRMED','seed-user-vol-02' FROM DUAL
) s ON (t.MATCHID=s.MATCHID)
WHEN MATCHED THEN UPDATE SET t.SOURCEPHOTOID=s.SOURCEPHOTOID,t.CANDIDATECATID=s.CANDIDATECATID,t.SIMILARITYSCORE=s.SIMILARITYSCORE,t.RANKNO=s.RANKNO,t.CONFIRMSTATUS=s.CONFIRMSTATUS,t.CONFIRMUSERID=s.CONFIRMUSERID
WHEN NOT MATCHED THEN INSERT (MATCHID,SOURCEPHOTOID,CANDIDATECATID,SIMILARITYSCORE,RANKNO,CONFIRMSTATUS,CONFIRMUSERID)
VALUES (s.MATCHID,s.SOURCEPHOTOID,s.CANDIDATECATID,s.SIMILARITYSCORE,s.RANKNO,s.CONFIRMSTATUS,s.CONFIRMUSERID);

-- Finance projects, donations, expense records and report snapshots
MERGE INTO FUND_CROWDFUNDINGPROJECTS t
USING (
    SELECT 'seed-project-001' PROJECTID,'seed-cat-003' CATID,'奶糖的绝育募捐' TITLE,1200 TARGETAMOUNT,880 RAISEDAMOUNT,TO_DATE('2026-07-01','YYYY-MM-DD') STARTTIME,TO_DATE('2026-08-31','YYYY-MM-DD') ENDTIME,'ACTIVE' PROJECTSTATUS FROM DUAL
    UNION ALL SELECT 'seed-project-002','seed-cat-005','团子的术后护理',1800,1800,TO_DATE('2026-06-01','YYYY-MM-DD'),TO_DATE('2026-07-15','YYYY-MM-DD'),'COMPLETED' FROM DUAL
    UNION ALL SELECT 'seed-project-003','seed-cat-021','阿福失踪搜寻',800,250,TO_DATE('2026-07-15','YYYY-MM-DD'),TO_DATE('2026-08-15','YYYY-MM-DD'),'ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-project-004','seed-cat-012','校园猫粮补给',3000,2600,TO_DATE('2026-07-10','YYYY-MM-DD'),TO_DATE('2026-09-10','YYYY-MM-DD'),'ACTIVE' FROM DUAL
    UNION ALL SELECT 'seed-project-005','seed-cat-029','医疗专项结余',500,500,TO_DATE('2026-05-01','YYYY-MM-DD'),TO_DATE('2026-06-01','YYYY-MM-DD'),'CANCELLED' FROM DUAL
) s ON (t.PROJECTID=s.PROJECTID)
WHEN MATCHED THEN UPDATE SET t.CATID=s.CATID,t.TITLE=s.TITLE,t.TARGETAMOUNT=s.TARGETAMOUNT,t.RAISEDAMOUNT=s.RAISEDAMOUNT,t.STARTTIME=s.STARTTIME,t.ENDTIME=s.ENDTIME,t.PROJECTSTATUS=s.PROJECTSTATUS
WHEN NOT MATCHED THEN INSERT (PROJECTID,CATID,TITLE,TARGETAMOUNT,RAISEDAMOUNT,STARTTIME,ENDTIME,PROJECTSTATUS)
VALUES (s.PROJECTID,s.CATID,s.TITLE,s.TARGETAMOUNT,s.RAISEDAMOUNT,s.STARTTIME,s.ENDTIME,s.PROJECTSTATUS);

MERGE INTO FUND_DONATIONS t
USING (
    SELECT 'seed-donation-001' DONATIONID,'seed-project-001' PROJECTID,'seed-user-user-01' DONORUSERID,200 AMOUNT,'ALIPAY' PAYMETHOD,TO_DATE('2026-07-02','YYYY-MM-DD') PAYTIME,1 PUBLICFLAG FROM DUAL
    UNION ALL SELECT 'seed-donation-002','seed-project-001','seed-user-user-02',120,'WECHAT',TO_DATE('2026-07-03','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-003','seed-project-001','seed-user-vol-01',300,'BANK_TRANSFER',TO_DATE('2026-07-04','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-004','seed-project-001','seed-user-vol-02',260,'CASH',TO_DATE('2026-07-05','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-donation-005','seed-project-002','seed-user-user-01',500,'ALIPAY',TO_DATE('2026-06-02','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-006','seed-project-002','seed-user-user-02',300,'WECHAT',TO_DATE('2026-06-03','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-007','seed-project-002','seed-user-vol-01',400,'ALIPAY',TO_DATE('2026-06-04','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-008','seed-project-002','seed-user-vol-02',600,'BANK_TRANSFER',TO_DATE('2026-06-05','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-donation-009','seed-project-003','seed-user-user-01',100,'WECHAT',TO_DATE('2026-07-16','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-010','seed-project-003','seed-user-user-02',50,'ALIPAY',TO_DATE('2026-07-17','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-011','seed-project-003','seed-user-vol-01',100,'CASH',TO_DATE('2026-07-18','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-donation-012','seed-project-004','seed-user-user-01',800,'ALIPAY',TO_DATE('2026-07-11','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-013','seed-project-004','seed-user-user-02',600,'WECHAT',TO_DATE('2026-07-12','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-014','seed-project-004','seed-user-vol-01',700,'BANK_TRANSFER',TO_DATE('2026-07-13','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-015','seed-project-004','seed-user-vol-02',500,'CASH',TO_DATE('2026-07-14','YYYY-MM-DD'),0 FROM DUAL
    UNION ALL SELECT 'seed-donation-016','seed-project-005','seed-user-user-01',200,'ALIPAY',TO_DATE('2026-05-02','YYYY-MM-DD'),1 FROM DUAL
    UNION ALL SELECT 'seed-donation-017','seed-project-005','seed-user-user-02',150,'WECHAT',TO_DATE('2026-05-03','YYYY-MM-DD'),1 FROM DUAL
) s ON (t.DONATIONID=s.DONATIONID)
WHEN MATCHED THEN UPDATE SET t.PROJECTID=s.PROJECTID,t.DONORUSERID=s.DONORUSERID,t.AMOUNT=s.AMOUNT,t.PAYMETHOD=s.PAYMETHOD,t.PAYTIME=s.PAYTIME,t.PUBLICFLAG=s.PUBLICFLAG
WHEN NOT MATCHED THEN INSERT (DONATIONID,PROJECTID,DONORUSERID,AMOUNT,PAYMETHOD,PAYTIME,PUBLICFLAG)
VALUES (s.DONATIONID,s.PROJECTID,s.DONORUSERID,s.AMOUNT,s.PAYMETHOD,s.PAYTIME,s.PUBLICFLAG);

MERGE INTO FUND_FINANCERECORDS t
USING (
    SELECT 'seed-finance-001' FINANCEID,'seed-project-001' PROJECTID,'MEDICAL' RECORDTYPE,260 AMOUNT,'https://example.invalid/invoice-001' INVOICEURL,'seed-user-admin' AUDITUSERID,'APPROVED' AUDITSTATUS,TO_DATE('2026-07-06','YYYY-MM-DD') PUBLICTIME FROM DUAL
    UNION ALL SELECT 'seed-finance-002','seed-project-001','FOOD',120,'https://example.invalid/invoice-002','seed-user-admin','PENDING',NULL FROM DUAL
    UNION ALL SELECT 'seed-finance-003','seed-project-002','MEDICAL',1200,'https://example.invalid/invoice-003','seed-user-admin','APPROVED',TO_DATE('2026-06-08','YYYY-MM-DD') FROM DUAL
    UNION ALL SELECT 'seed-finance-004','seed-project-002','SUPPLIES',300,'https://example.invalid/invoice-004','seed-user-admin','APPROVED',TO_DATE('2026-06-09','YYYY-MM-DD') FROM DUAL
    UNION ALL SELECT 'seed-finance-005','seed-project-003','OTHER',100,'https://example.invalid/invoice-005','seed-user-admin','PENDING',NULL FROM DUAL
    UNION ALL SELECT 'seed-finance-006','seed-project-004','FOOD',1600,'https://example.invalid/invoice-006','seed-user-admin','APPROVED',TO_DATE('2026-07-20','YYYY-MM-DD') FROM DUAL
    UNION ALL SELECT 'seed-finance-007','seed-project-004','SUPPLIES',500,'https://example.invalid/invoice-007','seed-user-admin','REJECTED',NULL FROM DUAL
    UNION ALL SELECT 'seed-finance-008','seed-project-005','MEDICAL',500,'https://example.invalid/invoice-008','seed-user-admin','APPROVED',TO_DATE('2026-05-20','YYYY-MM-DD') FROM DUAL
) s ON (t.FINANCEID=s.FINANCEID)
WHEN MATCHED THEN UPDATE SET t.PROJECTID=s.PROJECTID,t.RECORDTYPE=s.RECORDTYPE,t.AMOUNT=s.AMOUNT,t.INVOICEURL=s.INVOICEURL,t.AUDITUSERID=s.AUDITUSERID,t.AUDITSTATUS=s.AUDITSTATUS,t.PUBLICTIME=s.PUBLICTIME
WHEN NOT MATCHED THEN INSERT (FINANCEID,PROJECTID,RECORDTYPE,AMOUNT,INVOICEURL,AUDITUSERID,AUDITSTATUS,PUBLICTIME)
VALUES (s.FINANCEID,s.PROJECTID,s.RECORDTYPE,s.AMOUNT,s.INVOICEURL,s.AUDITUSERID,s.AUDITSTATUS,s.PUBLICTIME);

MERGE INTO RPT_STATISTICSSNAPSHOTS t
USING (
    SELECT 'seed-report-001' SNAPSHOTID,TO_DATE('2026-07-31','YYYY-MM-DD') SNAPSHOTDATE,'TOTAL_DONATION' METRICCODE,'MONTH' DIMENSIONTYPE,'2026-07' DIMENSIONVALUE,2480 METRICVALUE,'CNY' UNIT,TO_DATE('2026-08-01','YYYY-MM-DD') GENERATETIME,'七月捐赠总额' REMARK FROM DUAL
    UNION ALL SELECT 'seed-report-002',TO_DATE('2026-07-31','YYYY-MM-DD'),'TOTAL_EXPENSE','MONTH','2026-07',2080,'CNY',TO_DATE('2026-08-01','YYYY-MM-DD'),'七月支出总额' FROM DUAL
    UNION ALL SELECT 'seed-report-003',TO_DATE('2026-07-31','YYYY-MM-DD'),'NET_BALANCE','MONTH','2026-07',400,'CNY',TO_DATE('2026-08-01','YYYY-MM-DD'),'七月结余' FROM DUAL
    UNION ALL SELECT 'seed-report-004',TO_DATE('2026-07-31','YYYY-MM-DD'),'DONATION_COUNT','MONTH','2026-07',15,'COUNT',TO_DATE('2026-08-01','YYYY-MM-DD'),'七月捐赠笔数' FROM DUAL
    UNION ALL SELECT 'seed-report-005',TO_DATE('2026-08-01','YYYY-MM-DD'),'TOTAL_DONATION','PROJECT','seed-project-001',880,'CNY',TO_DATE('2026-08-02','YYYY-MM-DD'),'奶糖项目已筹金额' FROM DUAL
    UNION ALL SELECT 'seed-report-006',TO_DATE('2026-08-01','YYYY-MM-DD'),'TOTAL_EXPENSE','PROJECT','seed-project-002',1500,'CNY',TO_DATE('2026-08-02','YYYY-MM-DD'),'团子项目支出' FROM DUAL
) s ON (t.SNAPSHOTID=s.SNAPSHOTID)
WHEN MATCHED THEN UPDATE SET t.SNAPSHOTDATE=s.SNAPSHOTDATE,t.METRICCODE=s.METRICCODE,t.DIMENSIONTYPE=s.DIMENSIONTYPE,t.DIMENSIONVALUE=s.DIMENSIONVALUE,t.METRICVALUE=s.METRICVALUE,t.UNIT=s.UNIT,t.GENERATETIME=s.GENERATETIME,t.REMARK=s.REMARK
WHEN NOT MATCHED THEN INSERT (SNAPSHOTID,SNAPSHOTDATE,METRICCODE,DIMENSIONTYPE,DIMENSIONVALUE,METRICVALUE,UNIT,GENERATETIME,REMARK)
VALUES (s.SNAPSHOTID,s.SNAPSHOTDATE,s.METRICCODE,s.DIMENSIONTYPE,s.DIMENSIONVALUE,s.METRICVALUE,s.UNIT,s.GENERATETIME,s.REMARK);

COMMIT;
PROMPT ===== Fixed acceptance seed data loaded =====;
