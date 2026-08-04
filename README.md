# Campus Stray Cat DBMS
<p align="center">
    <img src='logo.png' alt='Campus Stray Cat DBMS Logo' width='200'/>
</p>

<p align="center">
    <img src="https://img.shields.io/badge/Backend-.NET%208-512BD4" alt="Backend: .NET 8" />
    <img src="https://img.shields.io/badge/API-ASP.NET%20Core-512BD4" alt="API: ASP.NET Core" />
    <img src="https://img.shields.io/badge/Frontend-React%2018-61DAFB" alt="Frontend: React 18" />
    <img src="https://img.shields.io/badge/Language-TypeScript-3178C6" alt="Language: TypeScript" />
    <img src="https://img.shields.io/badge/Build-Vite-646CFF" alt="Build: Vite" />
    <img src="https://img.shields.io/badge/Database-Oracle%2021c-blue" alt="Database: Oracle 21c" />
    <img src="https://img.shields.io/badge/Auth-JWT-orange" alt="Auth: JWT" />
    <img src="https://img.shields.io/badge/UI-animal--island--ui-19C8B9" alt="UI: animal-island-ui" />
    <img src="https://img.shields.io/badge/License-MIT-green" alt="License: MIT" />
</p>

校园流浪猫管理系统（Campus Stray Cat DBMS）是一个面向校园流浪猫救助、TNR、医疗、领养、志愿服务和财务公示的全栈课程项目。

后端使用 ASP.NET Core Web API 连接 Oracle 21c，前端使用 React + TypeScript + Vite，并采用 `animal-island-ui` 作为统一视觉组件库。系统通过 JWT 和角色权限控制不同用户可访问的页面与操作。

此为同济大学软件工程专业数据库课程设计项目，旨在展示数据库设计、建模、SQL、Web API 和前端界面的综合实现能力。项目包含数据库结构定义、数据插入脚本、接口文档、前端页面规划以及相关测试说明。

## 技术栈

| 层次 | 技术 |
| --- | --- |
| 数据库 | Oracle 21c、PL/SQL |
| 后端 | .NET 8、ASP.NET Core Web API |
| 数据访问 | Repository、Oracle SQL |
| 鉴权 | JWT Bearer、角色权限控制 |
| 前端 | React 18、TypeScript、Vite |
| 前端状态与请求 | Zustand、Axios |
| UI | animal-island-ui、Less/CSS Modules |

## 文档入口

- [前端页面分工与开发说明](docs/frontend_page_assignments.md)
- [A 组用户与权限接口文档](docs/A组_用户与权限接口文档.md)
- [B 组猫咪档案与校园位置接口文档](docs/B组_猫咪档案与校园位置接口文档.md)
- [C 组救助、TNR 与医疗接口文档](docs/C组_救助TNR与医疗接口文档.md)
- [D 组领养、志愿者、投喂与财务接口文档](docs/D组_领养志愿者投喂财务接口文档.md)

## 项目结构

```text
.
├── assets/                      # 图片、演示素材
├── database/                    # 建表、删表、示例数据、查询脚本
├── docs/                        # 需求、数据库说明、系统设计文档
├── scripts/                     # 本地运行与环境说明
└── src/                         # C# 应用源码
    ├── CampusStrayCatSystem.App/    # React + Vite 前端界面
    ├── CampusStrayCatSystem.Core/   # 业务逻辑层
    ├── CampusStrayCatSystem.Data/   # 数据访问层
    ├── CampusStrayCatSystem.Models/ # 数据模型
    └── CampusStrayCatSystem.Tests/  # 测试代码
```

运行命令：

```powershell
dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http
```

前端开发：

```powershell
cd src/CampusStrayCatSystem.App
npm install
npm run dev
```
