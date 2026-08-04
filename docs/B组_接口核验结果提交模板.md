# B 组（功能点 5-10）：接口核验结果提交模板

对应文档：[B组_猫咪档案与校园位置接口文档.md](B组_猫咪档案与校园位置接口文档.md)

## 1. 核验基本信息

| 项目 | 填写内容 |
|---|---|
| 测试人 |  |
| 测试日期 |  |
| 代码版本/Commit |  |
| 接口文档版本 |  |
| 后端地址 | `http://localhost:5047` |
| Oracle 环境 |  |
| 测试账号/角色 |  |
| 是否使用演示数据 | 是 / 否 |

## 2. 接口核验结果

状态统一填写：`未测`、`通过`、`不通过`、`阻塞`。不通过或阻塞项必须填写问题详情。

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| B-01 | 查询猫咪列表 | GET | `/api/cats` | 公开 | 未测 |  |
| B-02 | 查询猫咪详情 | GET | `/api/cats/{catId}` | 公开 | 未测 |  |
| B-03 | 新增猫咪档案 | POST | `/api/cats` | ADMIN/VOLUNTEER | 未测 |  |
| B-04 | 编辑猫咪档案 | PUT | `/api/cats/{catId}` | ADMIN/VOLUNTEER | 未测 |  |
| B-05 | 归档猫咪 | DELETE | `/api/cats/{catId}` | ADMIN/VOLUNTEER | 未测 |  |
| B-06 | 条件查询区域 | GET | `/api/campus-areas` | 公开 | 未测 |  |
| B-07 | 查询根区域 | GET | `/api/campus-areas/roots` | 公开 | 未测 |  |
| B-08 | 查询区域层级 | GET | `/api/campus-areas/hierarchy` | 公开 | 未测 |  |
| B-09 | 查询区域详情 | GET | `/api/campus-areas/{id}` | 公开 | 未测 |  |
| B-10 | 查询下级区域 | GET | `/api/campus-areas/{id}/children` | 公开 | 未测 |  |
| B-11 | 新增区域 | POST | `/api/campus-areas` | ADMIN/VOLUNTEER | 未测 |  |
| B-12 | 更新区域 | PUT | `/api/campus-areas/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-13 | 删除区域 | DELETE | `/api/campus-areas/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-14 | 查询服务点 | GET | `/api/service-points` | ADMIN/VOLUNTEER | 未测 |  |
| B-15 | 查询服务点详情 | GET | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-16 | 新增服务点 | POST | `/api/service-points` | ADMIN/VOLUNTEER | 未测 |  |
| B-17 | 更新服务点 | PUT | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-18 | 删除服务点 | DELETE | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-19 | 查询猫窝维护记录 | GET | `/api/nest-maintenance-records` | ADMIN/VOLUNTEER | 未测 |  |
| B-20 | 查询猫窝维护详情 | GET | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-21 | 新增猫窝维护记录 | POST | `/api/nest-maintenance-records` | ADMIN/VOLUNTEER | 未测 |  |
| B-22 | 更新猫窝维护记录 | PUT | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-23 | 删除猫窝维护记录 | DELETE | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-24 | 查询目击记录 | GET | `/api/cat-sightings` | 公开 | 未测 |  |
| B-25 | 查询目击详情 | GET | `/api/cat-sightings/{id}` | 公开 | 未测 |  |
| B-26 | 查询最近目击 | GET | `/api/cat-sightings/recent/by-cat/{catId}` | 公开 | 未测 |  |
| B-27 | 新增目击记录 | POST | `/api/cat-sightings` | 已登录 | 未测 |  |
| B-28 | 更新目击记录 | PUT | `/api/cat-sightings/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-29 | 删除目击记录 | DELETE | `/api/cat-sightings/{id}` | ADMIN/VOLUNTEER | 未测 |  |
| B-30 | 查询照片列表 | GET | `/api/cats/{catId}/photos` | 公开 | 未测 |  |
| B-31 | 查询照片详情 | GET | `/api/cats/{catId}/photos/{photoId}` | 公开 | 未测 |  |
| B-32 | 上传照片 | POST | `/api/cats/{catId}/photos` | ADMIN/VOLUNTEER | 未测 |  |
| B-33 | 设置主图 | PUT | `/api/cats/{catId}/photos/{photoId}/primary` | ADMIN/VOLUNTEER | 未测 |  |
| B-34 | 查询照片特征 | GET | `/api/cats/{catId}/photos/{photoId}/feature` | 公开 | 未测 |  |
| B-35 | 删除照片 | DELETE | `/api/cats/{catId}/photos/{photoId}` | ADMIN/VOLUNTEER | 未测 |  |
| B-36 | 查询命名候选 | GET | `/api/naming-votes/cats/{catId}/candidates` | 已登录 | 未测 |  |
| B-37 | 发布命名候选 | POST | `/api/naming-votes/cats/{catId}/candidates` | ADMIN/VOLUNTEER | 未测 |  |
| B-38 | 投票 | POST | `/api/naming-votes/candidates/{candidateId}/vote` | 已登录 | 未测 |  |
| B-39 | 确定获胜名称 | POST | `/api/naming-votes/candidates/{candidateId}/winner` | ADMIN | 未测 |  |

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-B-001 |  | 阻塞/严重/一般 |  |  |  |  |  | 待修复 |

```text
问题编号：BUG-B-___
接口编号：___
测试人/时间：___
严重性：阻塞 / 严重 / 一般
前置条件：
请求方法与 URL：
请求头（隐藏 Token）：
请求参数/请求体：
预期结果：
实际结果：
响应状态码与响应体：
复现步骤：
1.
2.
附件/日志：
建议处理人：
复测结果：未修复 / 已修复 / 无法复测
```

## 4. 提交前检查

- [ ] 已填写测试人、日期、代码版本和数据库环境。
- [ ] 39 个接口均已填写状态。
- [ ] 每个不通过或阻塞项都有问题编号和复现信息。
- [ ] 已隐藏 Token、密码、数据库账号和服务器凭据。
