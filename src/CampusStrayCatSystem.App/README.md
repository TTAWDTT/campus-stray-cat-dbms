# Campus Stray Cat System 前端

本目录是校园流浪猫管理系统的 React 前端，统一使用 TypeScript、Vite 和 `animal-island-ui`。页面按业务页面拆分，避免两位开发者直接修改同一批业务文件。

## 目录结构

```text
src/
├── main.tsx                    # 前端入口，只负责挂载 React
├── app/                        # 应用骨架
│   ├── App.tsx                 # 根组件
│   ├── router.tsx              # 路由统一注册
│   ├── layouts/                # 登录布局和主布局
│   └── guards/                 # 登录及角色路由守卫
├── shared/                     # 通用组件和工具，确认后尽量冻结
│   ├── components/             # PageHeader、DataTable、FormDrawer 等
│   ├── hooks/
│   ├── utils/
│   └── constants/
├── services/                   # Axios 实例、拦截器和公共请求处理
├── stores/                     # auth、ui 等全局状态
├── types/                      # 公共 DTO、枚举和接口类型
├── features/                   # 独立页面业务模块
│   ├── auth/                   # 登录与个人中心
│   ├── dashboard/              # 首页仪表盘
│   ├── cats/                   # 猫咪档案
│   ├── rescue/                 # TNR、医疗、紧急和失踪
│   ├── adoption/               # 领养流程
│   ├── volunteer/              # 志愿者、投喂和交接
│   ├── campus/                 # 区域、服务点和猫窝
│   ├── sightings/              # 目击记录
│   ├── finance/                # 众筹、捐赠、支出和公示
│   └── system/                 # 用户、角色和黑名单
├── styles/                    # 全局样式、token 和响应式规则
└── assets/                    # 图片和本地图标
```

## 页面模块约定

每个 `features/<module>/` 只维护自己的页面、组件、接口和类型：

```text
features/<module>/
├── pages/                      # 路由页面
├── components/                 # 只服务于本模块的组件
├── api.ts                      # 本模块 API 调用
├── types.ts                    # 本模块请求/响应类型
└── hooks.ts                    # 本模块状态和业务 hooks（需要时创建）
```

页面不得直接操作 Axios，也不要直接引用其他 feature。跨页面复用的内容放入 `shared`、`services` 或 `types`。

## 协作规则

1. `app/router.tsx` 统一维护，页面开发者只提交页面并提供路由路径。
2. `shared/components` 先共同确认 Props，之后尽量不随意改动。
3. 页面 API 放在各自的 `api.ts` 中，不在 JSX 内散落请求代码。
4. 页面状态标签、错误提示、空状态和表单抽屉优先复用 `shared` 组件。
5. Git 不记录空目录，新增模块至少保留本目录的说明文件或占位文件。

## 开发顺序

1. 先完成 `app`、`services`、`stores`、`types` 和 `shared` 的基础约定。
2. 先完成登录、布局、路由、猫咪档案和救助中心。
3. 再完成校园区域、目击记录、财务公示和系统管理。
4. 最后统一注册路由和处理真实 API 联调。
5. 两人共同检查权限、加载状态、异常提示和移动端布局。

当前首页原型仍位于 `src/main.tsx`，后续正式开发时迁移到 `features/dashboard/pages/`。
