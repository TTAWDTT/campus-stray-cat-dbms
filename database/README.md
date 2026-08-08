# Oracle 脚本运行说明

## 文件

- `create_tables.sql`: 创建 35 张表和外键
- `drop_tables.sql`: 删除 35 张表，便于重建
- `insert_demo_data.sql`: 写入可重复执行的区域、点位、猫窝维护和目击演示数据
- `queries/cat_photos_oracle_programming.sql`: 为现有数据库幂等增加猫咪唯一主图约束
- `queries/field_contract_constraints.sql`: 将旧值迁移为统一业务编码，并为稳定枚举补充 CHECK 约束
- `queries/cat_photos_acceptance.sql`: 验证唯一主图、特征 JSON 和照片引用约束并清理测试数据
- `queries/cat_matches_oracle_programming.sql`: 为现有数据库幂等增加匹配记录校验和唯一约束
- `queries/cat_matches_acceptance.sql`: 验证匹配记录约束、排序和确认字段并清理测试数据
- `queries/cat_matches_demo_data.sql`: 写入可重复执行的匹配来源照片、候选猫和候选记录
- `queries/a_group_schema_upgrade.sql`: 为旧数据库补齐 A 组黑名单释放人字段及外键
- `queries/test_queries.sql`: 验证区域层级、点位、维护记录和目击查询

## 推荐运行顺序

1. 先连接到项目用户 `CAT_SYSTEM`，并确保当前容器是 `XEPDB1`。
2. 新环境直接执行 `setup_all.sql`，它会按依赖顺序创建表、统一字段约束、视图、Package 和演示数据，包括猫咪匹配约束与演示记录。
3. 已有数据的环境不要执行 `drop_tables.sql`，先执行 `queries/field_contract_constraints.sql`，再按 `scripts/setup_local_db.md` 的“仅初始化对象”顺序执行。
4. 如需接口联调，执行 `insert_demo_data.sql` 和 `queries/cat_matches_demo_data.sql`，再运行 `queries/test_queries.sql`、照片验收和匹配验收脚本验证数据。

已有数据库不需要重建即可执行 `queries/cat_photos_oracle_programming.sql`、`queries/cat_matches_oracle_programming.sql` 和 `queries/a_group_schema_upgrade.sql`。这些脚本都会先检查已有数据；发现主图、匹配分数、排名、状态、唯一组合或黑名单释放人孤儿值异常时会中止，需先人工核对并修正数据。

猫咪照片模块验收时先执行约束脚本，再执行 `queries/cat_photos_acceptance.sql`。验收脚本使用 `test-photo-*` 临时 ID，成功或失败退出时都不会提交半成品测试事务。

## SQL Developer

1. 打开 SQL Developer，连接到 `CAT_SYSTEM`。
2. 选择菜单 `File -> Open`，打开 `create_tables.sql`。
3. 按 `F5` 执行脚本。`Ctrl+Enter` 只会执行当前语句，不适合整份脚本。
4. 重建时先打开并执行 `drop_tables.sql`，再执行 `create_tables.sql`。

## SQL*Plus 编码

数据库脚本使用 UTF-8 编码。Windows SQL*Plus 如果使用默认 GBK 客户端字符集，执行包含中文数据的脚本时可能出现 `ORA-01756`。执行脚本前请先设置：

```powershell
$env:NLS_LANG = "SIMPLIFIED CHINESE_CHINA.AL32UTF8"
```

在 `cmd.exe` 中使用：

```bat
set NLS_LANG=SIMPLIFIED CHINESE_CHINA.AL32UTF8
```

## 备注

- 如果你是用系统账号 `SYS` 或 `SYSTEM` 来建项目用户，先切到 `XEPDB1`。
- 表名和外键都采用 Oracle 默认的大写未加引号命名方式。
- 如果脚本报错，先看错误行号，再回到对应表的定义检查字段类型或外键关系。
