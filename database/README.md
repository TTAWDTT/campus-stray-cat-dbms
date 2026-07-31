# Oracle 脚本运行说明

## 文件

- `create_tables.sql`: 创建 35 张表和外键
- `drop_tables.sql`: 删除 35 张表，便于重建
- `insert_demo_data.sql`: 写入可重复执行的区域、点位、猫窝维护和目击演示数据
- `queries/test_queries.sql`: 验证区域层级、点位、维护记录和目击查询

## 推荐运行顺序

1. 先连接到项目用户 `CAT_SYSTEM`，并确保当前容器是 `XEPDB1`。
2. 如果需要重建数据库，先执行 `drop_tables.sql`。
3. 再执行 `create_tables.sql`。
4. 如需接口联调，执行 `insert_demo_data.sql`，再运行 `queries/test_queries.sql` 验证数据。

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
