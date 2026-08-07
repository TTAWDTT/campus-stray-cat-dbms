# 财务管理模块 — 数据模型说明

> 本文档覆盖众筹项目管理、捐赠记录、支出记录、财务公示、统计报表五大功能域的数据模型。
> 模型源码位于 `CampusStrayCatSystem.Models` 项目，控制器位于 `CampusStrayCatSystem.Core`。

---

## 目录

1. [FundCrowdfundingProject — 众筹项目](#1-fundcrowdfundingproject--众筹项目)
2. [FundDonation — 捐赠记录](#2-funddonation--捐赠记录)
3. [FundExpenseRecord — 支出记录](#3-fundexpenserecord--支出记录)
4. [FinancialDisclosureDto — 财务公示视图对象](#4-financialdisclosuredto--财务公示视图对象)
5. [RptStatisticsSnapshot — 统计快照](#5-rptstatisticssnapshot--统计快照)
6. [请求 DTO 辅助模型](#6-请求-dto-辅助模型)
7. [业务编码常量汇总](#7-业务编码常量汇总)
8. [数据模型关系图](#8-数据模型关系图)

---

## 1. FundCrowdfundingProject — 众筹项目

**对应数据库表:** `FUND_CROWDFUNDINGPROJECTS`
**源码位置:** `CampusStrayCatSystem.Models/FundCrowdfundingProject.cs`

### 1.1 字段说明

| 字段 | C# 类型 | 数据库列 | 必填 | 说明 |
|------|---------|----------|------|------|
| `ProjectID` | `string` | `PROJECTID` | ✅ 主键 | 众筹项目唯一标识 |
| `CatID` | `string?` | `CATID` | ❌ | 关联猫咪 ID，外键引用 `CAT_CATS.CATID` |
| `Title` | `string` | `TITLE` | ✅ | 项目标题，不能为空或纯空白 |
| `TargetAmount` | `decimal?` | `TARGETAMOUNT` | ❌ | 目标募资金额（人民币元），不能为负数 |
| `RaisedAmount` | `decimal?` | `RAISEDAMOUNT` | ❌ | 已筹金额（人民币元），由系统在捐赠事务中自动累加，不可手动修改 |
| `StartTime` | `DateTime?` | `STARTTIME` | ❌ | 项目开始时间，不能晚于 EndTime |
| `EndTime` | `DateTime?` | `ENDTIME` | ❌ | 项目结束时间，不能早于 StartTime |
| `ProjectStatus` | `string?` | `PROJECTSTATUS` | ❌ | 项目状态编码，详见下方状态表 |

### 1.2 项目状态 (`ProjectStatuses`)

| 编码 | 中文含义 | 说明 |
|------|----------|------|
| `ACTIVE` | 进行中 | 项目正常运行，接受捐赠 |
| `COMPLETED` | 已结束 | 项目达标或到期关闭，不再接受捐赠 |
| `CANCELLED` | 已取消 | 项目被取消，不再接受捐赠 |

### 1.3 业务约束

| 约束 | 触发场景 | 说明 |
|------|----------|------|
| Title 非空 | 创建 / 更新 | `Title` 不能为 `null`、空字符串或纯空白 |
| CatID 存在性 | 创建 / 更新 | 若指定 `CatID`，必须在 `CAT_CATS` 表中存在 |
| TargetAmount ≥ 0 | 创建 / 更新 | 目标金额不能为负数 |
| StartTime ≤ EndTime | 创建 / 更新 | 结束时间不能早于开始时间 |
| ProjectStatus 合法性 | 创建 / 更新 / 状态变更 | 状态值必须在 `{ACTIVE, COMPLETED, CANCELLED}` 中 |
| RaisedAmount 初始化为 0 | 创建 | 新项目已筹金额强制设为 0，防止篡改 |
| 默认状态 ACTIVE | 创建 | 若未指定状态，默认设为 `ACTIVE` |

### 1.4 关联 API

| 方法 | 路径 | 权限 | 说明 |
|------|------|------|------|
| `GET` | `/api/crowdfunding-projects` | 公开 | 获取所有众筹项目 |
| `GET` | `/api/crowdfunding-projects/{id}` | 公开 | 按 ID 获取单个项目 |
| `GET` | `/api/crowdfunding-projects/by-status/{status}` | 公开 | 按状态筛选项目 |
| `GET` | `/api/crowdfunding-projects/by-cat/{catId}` | 公开 | 按猫咪 ID 查询关联项目 |
| `POST` | `/api/crowdfunding-projects` | `ADMIN` | 创建众筹项目 |
| `PUT` | `/api/crowdfunding-projects/{id}` | `ADMIN` | 更新项目基本信息 |
| `PUT` | `/api/crowdfunding-projects/{id}/status` | `ADMIN` | 更新项目状态 |

---

## 2. FundDonation — 捐赠记录

**对应数据库表:** `FUND_DONATIONS`
**源码位置:** `CampusStrayCatSystem.Models/FundDonation.cs`

### 2.1 字段说明

| 字段 | C# 类型 | 数据库列 | 必填 | 说明 |
|------|---------|----------|------|------|
| `DonationID` | `string` | `DONATIONID` | ✅ 主键 | 捐赠记录唯一标识 |
| `ProjectID` | `string` | `PROJECTID` | ✅ | 所属众筹项目 ID，外键引用 `FUND_CROWDFUNDINGPROJECTS.PROJECTID` |
| `DonorUserID` | `string?` | `DONORUSERID` | ❌ | 捐赠人用户 ID，外键引用 `SYS_USERS.USERID` |
| `Amount` | `decimal?` | `AMOUNT` | ✅ | 捐赠金额（人民币元） |
| `PayMethod` | `string?` | `PAYMETHOD` | ❌ | 支付方式编码，详见下方支付方式表 |
| `PayTime` | `DateTime?` | `PAYTIME` | ❌ | 支付完成时间 |
| `PublicFlag` | `int?` | `PUBLICFLAG` | ❌ | 是否公开身份：`1`=公开，`0`=匿名，默认 `0` |

### 2.2 支付方式 (`PaymentMethods`)

| 编码 | 中文含义 |
|------|----------|
| `ALIPAY` | 支付宝 |
| `WECHAT` | 微信支付 |
| `BANK_TRANSFER` | 银行转账 |
| `CASH` | 现金 |
| `OTHER` | 其他 |

### 2.3 数据注解约束

| 字段 | 注解 | 说明 |
|------|------|------|
| `Amount` | `[Range(0.01, 79228162514264337593543950335)]` | 金额必须大于 0（最小 0.01 元），上限为 `decimal.MaxValue` |
| `PublicFlag` | `[Range(0, 1)]` | 只能是 0（匿名）或 1（公开） |

### 2.4 业务约束

| 约束 | 触发场景 | 说明 |
|------|----------|------|
| ProjectID 非空 | 创建 | `ProjectID` 不能为空 |
| 项目必须存在 | 创建 | `ProjectID` 对应的项目必须在 `FUND_CROWDFUNDINGPROJECTS` 表中存在 |
| 项目状态为 ACTIVE | 创建 | 只有 `ACTIVE` 状态的项目才能接受捐赠 |
| DonorUserID 存在性 | 创建 | 若指定捐赠人，必须在 `SYS_USERS` 表中存在 |
| Amount > 0 | 创建 | 金额必须为正数（由服务端二次校验） |
| PublicFlag ∈ {0, 1} | 创建 | 公开标记只能是 0 或 1 |
| PayMethod 合法性 | 创建 | 支付方式必须在 `PaymentMethods.Allowed` 中，自动转为大写 |
| 事务性累加 | 创建 | 写入捐赠记录的同时，在数据库事务中累加对应项目的 `RaisedAmount` |

### 2.5 关联 API

| 方法 | 路径 | 权限 | 说明 |
|------|------|------|------|
| `GET` | `/api/donations` | `ADMIN` | 获取所有捐赠记录 |
| `GET` | `/api/donations/{id}` | `ADMIN` | 按 ID 获取单条捐赠 |
| `GET` | `/api/donations/by-project/{projectId}` | `ADMIN` | 按项目查询捐赠记录 |
| `GET` | `/api/donations/by-donor/{donorUserId}` | 本人或 `ADMIN` | 按捐赠人查询其捐赠记录 |
| `POST` | `/api/donations` | 登录用户 | 创建捐赠记录（含事务累加） |

---

## 3. FundExpenseRecord — 支出记录

**对应数据库表:** `FUND_FINANCERECORDS`
**源码位置:** `CampusStrayCatSystem.Models/FundExpenseRecord.cs`

> 注意：数据库表名为 `FUND_FINANCERECORDS`（财务记录），其中仅记录支出；收入由 `FUND_DONATIONS` 跟踪。

### 3.1 字段说明

| 字段 | C# 类型 | 数据库列 | 必填 | 说明 |
|------|---------|----------|------|------|
| `FinanceID` | `string` | `FINANCEID` | ✅ 主键 | 支出记录唯一标识 |
| `ProjectID` | `string` | `PROJECTID` | ✅ | 所属众筹项目 ID，外键引用 `FUND_CROWDFUNDINGPROJECTS.PROJECTID` |
| `RecordType` | `string?` | `RECORDTYPE` | ❌ | 财务记录类型编码，详见下方类型表 |
| `Amount` | `decimal?` | `AMOUNT` | ✅ | 支出金额（人民币元） |
| `InvoiceUrl` | `string?` | `INVOICEURL` | ❌ | 发票或凭证文件的 URL 地址 |
| `AuditUserID` | `string?` | `AUDITUSERID` | ❌ | 审核人用户 ID，外键引用 `SYS_USERS.USERID`，审核时由服务端自动填入 |
| `AuditStatus` | `string?` | `AUDITSTATUS` | ❌ | 审核状态编码，详见下方审核状态表，创建时强制设为 `PENDING` |
| `PublicTime` | `DateTime?` | `PUBLICTIME` | ❌ | 公示时间，审核通过时由系统自动记录 |

### 3.2 审核状态 (`AuditStatuses`)

| 编码 | 中文含义 | 说明 |
|------|----------|------|
| `PENDING` | 待审核 | 初始状态，等待管理员审核 |
| `APPROVED` | 已通过 | 审核通过，金额计入财务公示的支出总额 |
| `REJECTED` | 已驳回 | 审核驳回，不计入公示 |

### 3.3 财务记录类型 (`FinanceRecordTypes`)

| 编码 | 中文含义 |
|------|----------|
| `FOOD` | 食物（猫粮等） |
| `MEDICAL` | 医疗（绝育、疫苗、治疗等） |
| `SUPPLIES` | 物资（猫窝、用具等） |
| `OTHER` | 其他 |

### 3.4 数据注解约束

| 字段 | 注解 | 说明 |
|------|------|------|
| `Amount` | `[Range(0.01, 79228162514264337593543950335)]` | 金额必须大于 0（最小 0.01 元） |

### 3.5 业务约束

| 约束 | 触发场景 | 说明 |
|------|----------|------|
| ProjectID 非空 | 创建 | `ProjectID` 不能为空 |
| 项目必须存在 | 创建 | `ProjectID` 对应的项目必须在 `FUND_CROWDFUNDINGPROJECTS` 表中存在 |
| Amount > 0 | 创建 | 金额必须为正数（由服务端二次校验） |
| RecordType 合法性 | 创建 | 类型必须在 `FinanceRecordTypes.Allowed` 中，自动转为大写 |
| 审核字段服务端维护 | 创建 | `AuditUserID`=null，`AuditStatus`=`PENDING`，`PublicTime`=null，客户端不可绕过 |
| 仅 PENDING 可审核 | 审核 | 只有 `PENDING` 状态的记录才能被审核 |
| 审核结果仅 APPROVED/REJECTED | 审核 | 不能设为 `PENDING` |
| 审核人必须存在 | 审核 | 审核操作者必须在 `SYS_USERS` 表中存在 |
| 通过后记录公示时间 | 审核 | `APPROVED` 时自动写入 `PublicTime` |

### 3.6 关联 API

| 方法 | 路径 | 权限 | 说明 |
|------|------|------|------|
| `GET` | `/api/expense-records` | `ADMIN` / `VOLUNTEER` | 获取所有支出记录 |
| `GET` | `/api/expense-records/{id}` | `ADMIN` / `VOLUNTEER` | 按 ID 获取单条支出记录 |
| `GET` | `/api/expense-records/by-project/{projectId}` | `ADMIN` / `VOLUNTEER` | 按项目查询支出记录 |
| `POST` | `/api/expense-records` | `ADMIN` / `VOLUNTEER` | 创建支出记录（默认待审核） |
| `PUT` | `/api/expense-records/{id}/audit` | `ADMIN` | 审核支出记录（通过/驳回） |

---

## 4. FinancialDisclosureDto — 财务公示视图对象

**源码位置:** `CampusStrayCatSystem.Models/RptStatisticsSnapshot.cs`

> 这是一个 **DTO（数据传输对象）**，不对应单张数据库表。它实时聚合 `FUND_CROWDFUNDINGPROJECTS`、`FUND_DONATIONS`、`FUND_FINANCERECORDS` 三张表的数据，用于前端展示财务透明度。

### 4.1 字段说明

| 字段 | C# 类型 | 来源 | 说明 |
|------|---------|------|------|
| `Project` | `FundCrowdfundingProject?` | `FUND_CROWDFUNDINGPROJECTS` | 项目基本信息（标题、状态、时间等） |
| `TargetAmount` | `decimal?` | 计算属性 → `Project.TargetAmount` | 目标募资金额 |
| `RaisedAmount` | `decimal?` | 计算属性 → `Project.RaisedAmount` | 已筹金额（捐赠累计） |
| `TotalExpense` | `decimal?` | 聚合查询 → `FUND_FINANCERECORDS` | 已审核通过（`APPROVED`）的支出总额 |
| `NetBalance` | `decimal?` | 计算属性 → `RaisedAmount - TotalExpense` | 净余额 = 已筹金额 − 已通过支出 |
| `DonationCount` | `int` | 聚合查询 → `FUND_DONATIONS` | 该项目收到的捐赠总笔数 |
| `Donations` | `IEnumerable<FundDonation>` | `FUND_DONATIONS` | 公开的捐赠明细列表（用于公示） |
| `Expenses` | `IEnumerable<FundExpenseRecord>` | `FUND_FINANCERECORDS` | 已审核通过的支出明细列表 |

### 4.2 关联 API

| 方法 | 路径 | 权限 | 说明 |
|------|------|------|------|
| `GET` | `/api/financial-disclosure/{projectId}` | 公开 | 获取指定项目的完整财务公示（含明细） |
| `GET` | `/api/financial-disclosure/summary` | 公开 | 获取所有进行中项目的财务公示摘要（不含明细） |

---

## 5. RptStatisticsSnapshot — 统计快照

**对应数据库表:** `RPT_STATISTICSSNAPSHOTS`
**源码位置:** `CampusStrayCatSystem.Models/RptStatisticsSnapshot.cs`

### 5.1 字段说明

| 字段 | C# 类型 | 数据库列 | 必填 | 说明 |
|------|---------|----------|------|------|
| `SnapshotID` | `string` | `SNAPSHOTID` | ✅ 主键 | 快照唯一标识 |
| `SnapshotDate` | `DateTime?` | `SNAPSHOTDATE` | ❌ | 统计快照对应的日期（业务日期，非生成时间） |
| `MetricCode` | `string?` | `METRICCODE` | ❌ | 指标代码，详见下方指标表 |
| `MetricValue` | `decimal?` | `METRICVALUE` | ❌ | 指标数值（金额或计数） |
| `DimensionType` | `string?` | `DIMENSIONTYPE` | ❌ | 维度类型，详见下方维度表 |
| `DimensionValue` | `string?` | `DIMENSIONVALUE` | ❌ | 维度值（如项目 ID、月份字符串 `2026-08`、猫咪 ID） |
| `Unit` | `string?` | `UNIT` | ❌ | 单位，详见下方单位表 |
| `GenerateTime` | `DateTime?` | `GENERATETIME` | ❌ | 快照实际生成时间戳 |
| `Remark` | `string?` | `REMARK` | ❌ | 备注说明 |

### 5.2 指标代码 (`StatisticCodes.MetricCodes`)

| 编码 | 中文含义 | 典型单位 |
|------|----------|----------|
| `TOTAL_DONATION` | 总捐赠收入 | `CNY` |
| `TOTAL_EXPENSE` | 总支出 | `CNY` |
| `NET_BALANCE` | 净余额 | `CNY` |
| `DONATION_COUNT` | 捐赠笔数 | `COUNT` |

### 5.3 维度类型 (`StatisticCodes.DimensionTypes`)

| 编码 | 中文含义 | DimensionValue 示例 |
|------|----------|---------------------|
| `PROJECT` | 按项目 | 项目 ID（如 `PRJ-001`） |
| `MONTH` | 按月 | 月份字符串（如 `2026-08`） |
| `CAT` | 按猫咪 | 猫咪 ID（如 `CAT-001`） |

### 5.4 单位 (`StatisticCodes.Units`)

| 编码 | 中文含义 |
|------|----------|
| `CNY` | 人民币元 |
| `COUNT` | 次数 |

### 5.5 业务约束

| 约束 | 触发场景 | 说明 |
|------|----------|------|
| MetricCode 合法性 | 查询 | 指标代码必须在 `{TOTAL_DONATION, TOTAL_EXPENSE, NET_BALANCE, DONATION_COUNT}` 中 |
| DimensionType 合法性 | 查询 | 维度类型必须在 `{PROJECT, MONTH, CAT}` 中 |
| DimensionValue 非空 | 查询 | 维度值不能为空 |
| 项目必须存在 | 生成 | 指定的项目 ID 必须在 `FUND_CROWDFUNDINGPROJECTS` 表中存在 |
| 事务性写入 4 条 | 生成 | 一次生成写入 4 条快照记录（TOTAL_DONATION、TOTAL_EXPENSE、NET_BALANCE、DONATION_COUNT），维度均为 `PROJECT` |

### 5.6 关联 API

| 方法 | 路径 | 权限 | 说明 |
|------|------|------|------|
| `GET` | `/api/statistics-reports` | 登录用户 | 获取所有统计快照 |
| `GET` | `/api/statistics-reports/snapshot/{id}` | 登录用户 | 按 ID 获取单条快照 |
| `GET` | `/api/statistics-reports/by-metric/{metricCode}` | 登录用户 | 按指标代码查询快照 |
| `GET` | `/api/statistics-reports/by-dimension/{dimensionType}/{dimensionValue}` | 登录用户 | 按维度查询快照 |
| `POST` | `/api/statistics-reports/generate/{projectId}` | `ADMIN` | 为指定项目生成统计报表快照 |

---

## 6. 请求 DTO 辅助模型

### 6.1 UpdateProjectStatusRequest

**源码位置:** `CampusStrayCatSystem.Core/CrowdfundingProjectsController.cs`

| 字段 | C# 类型 | 必填 | 说明 |
|------|---------|------|------|
| `NewStatus` | `string` | ✅ | 目标状态，必须为 `ACTIVE` / `COMPLETED` / `CANCELLED` 之一 |

**对应 API:** `PUT /api/crowdfunding-projects/{id}/status`

### 6.2 AuditExpenseRecordRequest

**源码位置:** `CampusStrayCatSystem.Core/ExpenseRecordsController.cs`

| 字段 | C# 类型 | 必填 | 说明 |
|------|---------|------|------|
| `AuditUserID` | `string` | ❌ | 请求体中的审核人 ID（实际以当前登录用户为准） |
| `AuditStatus` | `string` | ✅ | 审核结果，必须为 `APPROVED` 或 `REJECTED` |

**对应 API:** `PUT /api/expense-records/{id}/audit`

---

## 7. 业务编码常量汇总

> 所有编码常量定义在 `CampusStrayCatSystem.Models/DomainCodes.cs`，接口和数据库统一使用英文大写编码。

| 常量类 | 用途 | 可选值 |
|--------|------|--------|
| `ProjectStatuses` | 众筹项目状态 | `ACTIVE`, `COMPLETED`, `CANCELLED` |
| `AuditStatuses` | 支出审核状态 | `PENDING`, `APPROVED`, `REJECTED` |
| `PaymentMethods` | 捐赠支付方式 | `ALIPAY`, `WECHAT`, `BANK_TRANSFER`, `CASH`, `OTHER` |
| `FinanceRecordTypes` | 支出类型 | `FOOD`, `MEDICAL`, `SUPPLIES`, `OTHER` |
| `StatisticCodes.MetricCodes` | 统计指标 | `TOTAL_DONATION`, `TOTAL_EXPENSE`, `NET_BALANCE`, `DONATION_COUNT` |
| `StatisticCodes.DimensionTypes` | 统计维度 | `PROJECT`, `MONTH`, `CAT` |
| `StatisticCodes.Units` | 统计单位 | `CNY`, `COUNT` |

---

## 8. 数据模型关系图

```
┌─────────────────────────────────────┐
│        CAT_CATS (猫咪档案)           │
│  PK: CatID                          │
└────────────┬────────────────────────┘
             │ 1:N (可选)
             ▼
┌─────────────────────────────────────┐
│  FUND_CROWDFUNDINGPROJECTS (众筹项目) │
│  PK: ProjectID                      │
│  FK: CatID → CAT_CATS.CatID         │
│  ├─ Title                           │
│  ├─ TargetAmount                    │
│  ├─ RaisedAmount  ◄── 事务累加 ───┐ │
│  ├─ StartTime                      │ │
│  ├─ EndTime                        │ │
│  └─ ProjectStatus                  │ │
└────────┬──────────────┬────────────┘ │
         │ 1:N          │ 1:N          │
         ▼              ▼              │
┌────────────────┐  ┌──────────────────────────┐
│ FUND_DONATIONS │  │ FUND_FINANCERECORDS      │
│ (捐赠记录)      │  │ (支出记录)                 │
│ PK: DonationID │  │ PK: FinanceID            │
│ FK: ProjectID  │  │ FK: ProjectID            │
│ FK: DonorUserID│  │ FK: AuditUserID          │
│                │  │                          │
│ ├─ Amount ─────┼──┤ ├─ Amount                │
│ ├─ PayMethod   │  │ ├─ RecordType            │
│ ├─ PayTime     │  │ ├─ InvoiceUrl            │
│ └─ PublicFlag  │  │ ├─ AuditStatus           │
└───────┬────────┘  │ └─ PublicTime            │
        │           └──────────────────────────┘
        │ 1:N                    │
        ▼                        ▼
┌──────────────────────────────────────────────┐
│         FinancialDisclosureDto (视图/DTO)     │
│  聚合以上三表，提供项目财务公示                 │
│  ├─ Project (项目信息)                        │
│  ├─ RaisedAmount (已筹 = 捐赠累计)            │
│  ├─ TotalExpense (已通过支出总额)              │
│  ├─ NetBalance (净余额 = 已筹 - 已通过支出)    │
│  ├─ DonationCount (捐赠笔数)                   │
│  ├─ Donations[] (捐赠明细)                     │
│  └─ Expenses[] (支出明细，仅已通过)            │
└──────────────────────────────────────────────┘

┌──────────────────────────────────────────────┐
│   RPT_STATISTICSSNAPSHOTS (统计快照)          │
│   PK: SnapshotID                             │
│   ├─ SnapshotDate (业务日期)                  │
│   ├─ MetricCode (指标: TOTAL_DONATION 等)     │
│   ├─ MetricValue (数值)                       │
│   ├─ DimensionType (维度: PROJECT/MONTH/CAT)  │
│   ├─ DimensionValue (维度值: 项目ID/月份/猫ID) │
│   ├─ Unit (单位: CNY/COUNT)                   │
│   ├─ GenerateTime (生成时间)                   │
│   └─ Remark (备注)                            │
│                                              │
│   生成来源: 聚合 DONATIONS + FINANCERECORDS   │
│   一次生成 = 4 条记录 (按 PROJECT 维度)        │
└──────────────────────────────────────────────┘
```

---

> **最后更新:** 2026-08-06
> **对应提交:** `07dbc8b` — 统一数据库与接口字段契约
