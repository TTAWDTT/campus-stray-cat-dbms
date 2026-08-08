# 本地 Oracle 初始化

以下脚本按顺序执行，默认连接到项目用户（例如 `CAT_SYSTEM` / `XEPDB1`）。

## 一次性重建

在 SQL Developer 中使用 `F5` 执行：

```sql
@database/setup_all.sql
```

也可以打开 `database/setup_all.sql` 后执行。脚本会先删除再重建表，**不要在需要保留数据的环境执行**。

## 仅初始化对象

已有业务数据时，不执行 `drop_tables.sql`，依次执行：

1. `database/create_tables.sql`
2. `database/queries/field_contract_constraints.sql`
3. `database/queries/cat_photos_oracle_programming.sql`
4. `database/queries/cat_matches_oracle_programming.sql`
5. `database/queries/a_group_schema_upgrade.sql`
6. `database/queries/a_group_advanced.sql`
7. `database/queries/task_17_18_19_oracle_programming.sql`
8. `database/queries/rescue_care_oracle_programming.sql`
9. `database/queries/a_group_demo_data.sql`（可选演示账号）
10. `database/insert_demo_data.sql`（可选校园数据）
11. `database/queries/cat_matches_demo_data.sql`（可选匹配演示数据）

演示账号密码统一为 `Passw0rd!`，只用于本地联调。

## 环境要求

- Oracle 21c 或兼容版本；
- SQL Developer 使用 `F5` 执行整份脚本；
- Windows SQL*Plus 执行中文脚本前设置 `NLS_LANG=SIMPLIFIED CHINESE_CHINA.AL32UTF8`；
- 后端连接串和 JWT 密钥通过未提交的环境变量或 `appsettings.Development.json` 配置。
