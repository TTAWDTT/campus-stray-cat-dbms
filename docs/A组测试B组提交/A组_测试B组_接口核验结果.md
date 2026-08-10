# A 组测试 B 组：接口核验结果

对应文档：[B组_猫咪档案与校园位置接口文档.md](../B组_猫咪档案与校园位置接口文档.md)

## 1. 核验基本信息

| 项目 | 填写内容 |
|---|---|
| 测试人 | 陈美希 黄子天 |
| 测试日期 | 2026-08-10 |
| 代码版本/Commit | `ff3de62` |
| 接口文档版本 | docs/B组_猫咪档案与校园位置接口文档.md |
| 后端地址 | `http://127.0.0.1:5047` |
| Oracle 环境 | Docker `campus-oracle` / FREEPDB1 / CAT_SYSTEM |
| 测试账号/角色 | `a_group_admin`(ADMIN) / `a_group_volunteer` / `a_group_user`，密码见演示脚本（提交时勿贴明文） |
| 是否使用演示数据 | 是（`setup_all.sql`） |
| Postman Runner | 11:41 汇总 **116 tests / 108 passed / 8 failed**（失败含 Postman 文件沙箱导致的照片连锁） |

## 2. 接口核验结果

状态统一填写：`未测`、`通过`、`不通过`、`阻塞`。不通过或阻塞项必须填写问题详情。

| 编号 | 接口 | 方法 | URL | 测试角色 | 状态 | 问题编号 |
|---|---|---|---|---|---|---|
| B-01 | 查询猫咪列表 | GET | `/api/cats` | 公开 | 通过 |  |
| B-02 | 查询猫咪详情 | GET | `/api/cats/{catId}` | 公开 | 通过 |  |
| B-03 | 新增猫咪档案 | POST | `/api/cats` | ADMIN/VOLUNTEER | 不通过 | BUG-B-001 |
| B-04 | 编辑猫咪档案 | PUT | `/api/cats/{catId}` | ADMIN/VOLUNTEER | 不通过 | BUG-B-002 |
| B-05 | 归档猫咪 | DELETE | `/api/cats/{catId}` | ADMIN/VOLUNTEER | 通过 |  |
| B-06 | 条件查询区域 | GET | `/api/campus-areas` | 公开 | 通过 |  |
| B-07 | 查询根区域 | GET | `/api/campus-areas/roots` | 公开 | 通过 |  |
| B-08 | 查询区域层级 | GET | `/api/campus-areas/hierarchy` | 公开 | 通过 |  |
| B-09 | 查询区域详情 | GET | `/api/campus-areas/{id}` | 公开 | 通过 |  |
| B-10 | 查询下级区域 | GET | `/api/campus-areas/{id}/children` | 公开 | 通过 |  |
| B-11 | 新增区域 | POST | `/api/campus-areas` | ADMIN/VOLUNTEER | 通过 |  |
| B-12 | 更新区域 | PUT | `/api/campus-areas/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-13 | 删除区域 | DELETE | `/api/campus-areas/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-14 | 查询服务点 | GET | `/api/service-points` | ADMIN/VOLUNTEER | 通过 |  |
| B-15 | 查询服务点详情 | GET | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-16 | 新增服务点 | POST | `/api/service-points` | ADMIN/VOLUNTEER | 不通过 | BUG-B-003 |
| B-17 | 更新服务点 | PUT | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-18 | 删除服务点 | DELETE | `/api/service-points/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-19 | 查询猫窝维护记录 | GET | `/api/nest-maintenance-records` | ADMIN/VOLUNTEER | 通过 |  |
| B-20 | 查询猫窝维护详情 | GET | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-21 | 新增猫窝维护记录 | POST | `/api/nest-maintenance-records` | ADMIN/VOLUNTEER | 通过 |  |
| B-22 | 更新猫窝维护记录 | PUT | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-23 | 删除猫窝维护记录 | DELETE | `/api/nest-maintenance-records/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-24 | 查询目击记录 | GET | `/api/cat-sightings` | 公开 | 通过 |  |
| B-25 | 查询目击详情 | GET | `/api/cat-sightings/{id}` | 公开 | 通过 |  |
| B-26 | 查询最近目击 | GET | `/api/cat-sightings/recent/by-cat/{catId}` | 公开 | 通过 |  |
| B-27 | 新增目击记录 | POST | `/api/cat-sightings` | 已登录 | 通过 |  |
| B-28 | 更新目击记录 | PUT | `/api/cat-sightings/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-29 | 删除目击记录 | DELETE | `/api/cat-sightings/{id}` | ADMIN/VOLUNTEER | 通过 |  |
| B-30 | 查询照片列表 | GET | `/api/cats/{catId}/photos` | 公开 | 通过 |  |
| B-31 | 查询照片详情 | GET | `/api/cats/{catId}/photos/{photoId}` | 公开 | 通过 |  |
| B-32 | 上传照片 | POST | `/api/cats/{catId}/photos` | ADMIN/VOLUNTEER | 通过 |  |
| B-33 | 设置主图 | PUT | `/api/cats/{catId}/photos/{photoId}/primary` | ADMIN/VOLUNTEER | 通过 |  |
| B-34 | 查询照片特征 | GET | `/api/cats/{catId}/photos/{photoId}/feature` | 公开 | 通过 |  |
| B-35 | 删除照片 | DELETE | `/api/cats/{catId}/photos/{photoId}` | ADMIN/VOLUNTEER | 通过 |  |
| B-36 | 查询命名候选 | GET | `/api/naming-votes/cats/{catId}/candidates` | 已登录 | 通过 |  |
| B-37 | 发布命名候选 | POST | `/api/naming-votes/cats/{catId}/candidates` | ADMIN/VOLUNTEER | 通过 |  |
| B-38 | 投票 | POST | `/api/naming-votes/candidates/{candidateId}/vote` | 已登录 | 通过 |  |
| B-39 | 确定获胜名称 | POST | `/api/naming-votes/candidates/{candidateId}/winner` | ADMIN | 通过 |  |

说明：

- B-03 / B-04：**主流程在补齐文档未写明的必填字段后可通过**；但与官方文档「可选」约定冲突，故标不通过并开 BUG。
- B-16：合法完整新增通过；**缺字段仍 201** 与文档冲突，标不通过。
- B-30～B-35：Postman Runner 因文件沙箱常假失败；已用 curl 带真实 PNG 复核通过（见 `A组测试证据/curl_photo_evidence/`）。单独点 Send 时 `{{catId}}` 红线 / 401 属未先跑登录与建猫，不是接口缺陷。

## 3. 问题详情

| 问题编号 | 接口编号 | 严重性 | 测试数据/前置条件 | 预期结果 | 实际结果 | 响应码/错误信息 | 附件/链接 | 当前状态 |
|---|---|---|---|---|---|---|---|---|
| BUG-B-001 | B-03 | 严重 | POST `/api/cats`，不传 `colorPattern` | 文档：可选，应可创建（201）或仅业务默认 | 校验失败 | 400，`ColorPattern: 花色不能为空` | `curl_photo_evidence/BUG-B-001_missing_colorPattern.txt`；首轮 Runner | 待修复 |
| BUG-B-002 | B-04 | 一般 | PUT 更新不传 `sterilizedFlag`/`earTipFlag` | 文档：与新增相同且该两字段为否 | 校验失败 | 400，`绝育标志不能为空` / `剪耳标志不能为空` | `curl_photo_evidence/BUG-B-002_update_missing_flags.txt` | 待修复 |
| BUG-B-003 | B-16 | 严重 | POST `/api/service-points`，body 仅 `{"pointName":"无区域"}` | 缺关联区域等应 400 | 仍创建成功 | **201**，`areaID/pointType/...` 为 null | Postman Runner 11:41；`curl_photo_evidence/BUG-B-003_service_point_missing_fields.txt` | 待修复 |

```text
问题编号：BUG-B-001
接口编号：B-03
测试人/时间：A组 / 2026-08-10
严重性：严重
前置条件：管理员已登录
请求方法与 URL：POST /api/cats
请求体：{"catName":"缺花色","lifeStatus":"ON_CAMPUS"}
预期结果：文档 colorPattern 必填=否，应允许创建或给出与文档一致的约定
实际结果：400，花色不能为空（实现 [Required]）
附件：A组测试证据/curl_photo_evidence/BUG-B-001_missing_colorPattern.txt
建议处理人：B组
复测结果：未修复
```

```text
问题编号：BUG-B-002
接口编号：B-04
测试人/时间：A组 / 2026-08-10
严重性：一般
前置条件：存在有效 catId，管理员已登录
请求方法与 URL：PUT /api/cats/{catId}
请求体：含 colorPattern/gender/lifeStatus/archiveStatus，缺 sterilizedFlag、earTipFlag
预期结果：文档写上述标志可选
实际结果：400，绝育/剪耳标志不能为空
附件：A组测试证据/curl_photo_evidence/BUG-B-002_update_missing_flags.txt
建议处理人：B组
复测结果：未修复
```

```text
问题编号：BUG-B-003
接口编号：B-16
测试人/时间：A组 / 2026-08-10
严重性：严重
前置条件：管理员已登录
请求方法与 URL：POST /api/service-points
请求体：{"pointName":"无区域"}
预期结果：400（参数/关联非法）
实际结果：201 Created，areaID 等为 null
附件：Postman Runner Failed 列表；curl_photo_evidence/BUG-B-003_service_point_missing_fields.txt
建议处理人：B组
复测结果：未修复
```

## 4. 提交前检查

- [ ] 已填写测试人姓名/学号（上方仍有占位）
- [x] 已填写日期、代码版本和数据库环境
- [x] 39 个接口均已填写状态
- [x] 每个不通过项都有问题编号和复现信息
- [x] 已隐藏 Token、密码（证据中无完整 JWT）
- [x] Postman 截图已归档至 `A组测试证据/screenshots/`（见 `证据目录说明.md`）

## 5. 备注（特殊情况）

### 5.1 Runner 摘要

| 轮次 | 时间 | 结果 |
|---|---|---|
| 首轮 | 11:20 | 100 / 63 pass / 37 fail（缺 colorPattern 连锁） |
| 二轮 | 11:28 | 114 / 104 pass / 10 fail |
| 三轮 | 11:41 | 116 / 108 pass / 8 fail（含照片文件沙箱假失败 + BUG-B-003） |

### 5.2 照片模块与 Postman

单独 Send 出现 `{{catId}}` 红线、401、弹窗 Select Files，是因为：

1. 未先跑登录 / 建猫，集合变量为空；
2. Postman 沙箱未读到本地 PNG（Runner 更明显）。

curl 复核（`A组测试证据/curl_photo_evidence/`）：上传 201、详情 200、主图 204、不存在猫 404、归档上传 409、删除 204。

若要坚持 Postman 跑通照片：Settings → Working directory = `/Users/mecy/Postman`，文件选同目录 `test-cat.png`，且必须 **Run 整库** 而不是只点照片请求。

### 5.3 文档不一致清单（给 B 组）

1. `colorPattern` 文档可选 / 实现必填  
2. `sterilizedFlag`、`earTipFlag` 文档可选 / 更新实现必填  
3. 服务点写入校验过松，缺 `areaID` 仍 201  
