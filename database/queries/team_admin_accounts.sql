SET DEFINE OFF;

-- 组员统一管理员账号初始化。
-- 账号：姓名拼音（无空格）；密码：Passw0rd!
-- PASSWORDHASH 使用 ASP.NET Core Identity PasswordHasher V3，适用于当前后端。

MERGE INTO SYS_USERS target
USING (
    SELECT 'user-luozhen-team' AS USERID, 'role-admin-a-group' AS ROLEID, 'luozhen' AS USERNAME,
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==' AS PASSWORDHASH,
           '罗臻' AS REALNAME, 'VERIFIED' AS VERIFYSTATUS, 'ACTIVE' AS STATUS FROM DUAL
    UNION ALL
    SELECT 'user-fanyu-team', 'role-admin-a-group', 'fanyu',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '范羽', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-chenmeixi-team', 'role-admin-a-group', 'chenmeixi',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '陈美希', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-huangzitian-team', 'role-admin-a-group', 'huangzitian',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '黄子天', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-xuqianshun-team', 'role-admin-a-group', 'xuqianshun',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '徐千顺', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-zhaoqing-team', 'role-admin-a-group', 'zhaoqing',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '赵晴', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-yinjiawei-team', 'role-admin-a-group', 'yinjiawei',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '尹佳玮', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-songxinyue-team', 'role-admin-a-group', 'songxinyue',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '宋新悦', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-mengshengyu-team', 'role-admin-a-group', 'mengshengyu',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '孟圣雨', 'VERIFIED', 'ACTIVE' FROM DUAL
    UNION ALL
    SELECT 'user-licanwen-team', 'role-admin-a-group', 'licanwen',
           'AQAAAAIAAYagAAAAEDFMfZFW7ApQ6JxnJYQ+fOkEJBIfjO01z8/CgnIVwgOBDc7tC304pf0BqTWhf1Afag==',
           '李灿文', 'VERIFIED', 'ACTIVE' FROM DUAL
) source
ON (target.USERID = source.USERID)
WHEN MATCHED THEN UPDATE SET
    target.ROLEID = source.ROLEID,
    target.USERNAME = source.USERNAME,
    target.PASSWORDHASH = source.PASSWORDHASH,
    target.REALNAME = source.REALNAME,
    target.VERIFYSTATUS = source.VERIFYSTATUS,
    target.STATUS = source.STATUS
WHEN NOT MATCHED THEN INSERT
    (USERID, ROLEID, USERNAME, PASSWORDHASH, REALNAME, VERIFYSTATUS, STATUS)
VALUES
    (source.USERID, source.ROLEID, source.USERNAME, source.PASSWORDHASH,
     source.REALNAME, source.VERIFYSTATUS, source.STATUS);

COMMIT;

PROMPT Team admin accounts are ready. Password: Passw0rd!
