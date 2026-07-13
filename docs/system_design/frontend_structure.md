# 前端与代码结构说明

本项目的“前端”指 C# 应用界面，不是单独的网页前端。源码按职责拆成四层，便于分组协作。

## 代码分层

- `src/CampusStrayCatSystem.App/`：界面层，放窗体、页面、按钮和表格
- `src/CampusStrayCatSystem.Core/`：业务逻辑层，放登录、权限、流程规则
- `src/CampusStrayCatSystem.Data/`：数据访问层，放 Oracle 连接和 SQL 封装
- `src/CampusStrayCatSystem.Models/`：模型层，放实体类和通用数据对象
- `src/CampusStrayCatSystem.Tests/`：测试层，放关键功能验证

## 协作顺序

1. 先定义模型。
2. 再补数据访问。
3. 再写业务逻辑。
4. 最后接入界面。

这样前端不会和数据库脚本搅在一起，后面合并也更顺。
