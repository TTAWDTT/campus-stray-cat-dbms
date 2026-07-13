# Oracle 脚本运行说明

## 文件

- `create_tables.sql`: 创建 35 张表和外键
- `drop_tables.sql`: 删除 35 张表，便于重建

## 推荐运行顺序

1. 先连接到项目用户 `CAT_SYSTEM`，并确保当前容器是 `XEPDB1`。
2. 如果需要重建数据库，先执行 `drop_tables.sql`。
3. 再执行 `create_tables.sql`。

## SQL Developer

1. 打开 SQL Developer，连接到 `CAT_SYSTEM`。
2. 选择菜单 `File -> Open`，打开 `create_tables.sql`。
3. 按 `F5` 执行脚本。`Ctrl+Enter` 只会执行当前语句，不适合整份脚本。
4. 重建时先打开并执行 `drop_tables.sql`，再执行 `create_tables.sql`。

## 备注

- 如果你是用系统账号 `SYS` 或 `SYSTEM` 来建项目用户，先切到 `XEPDB1`。
- 表名和外键都采用 Oracle 默认的大写未加引号命名方式。
- 如果脚本报错，先看错误行号，再回到对应表的定义检查字段类型或外键关系。
