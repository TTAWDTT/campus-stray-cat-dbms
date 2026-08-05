# 前端先行开发计划

本文档用于在其他成员熟悉 React 和 `animal-island-ui` 期间，先完成所有页面共同依赖的前端基础部分。

## 一、先确认工程可运行

```powershell
cd src/CampusStrayCatSystem.App
npm install
npm run dev
```

确认 Vite、React、`animal-island-ui` 和首页原型可以正常启动。

## 二、应用外壳

优先建立和完善：

```text
src/app/App.tsx
src/app/router.tsx
src/app/layouts/MainLayout.tsx
src/app/layouts/AuthLayout.tsx
src/app/guards/RouteGuard.tsx
```

应先实现：

- 主布局和登录布局
- 顶部/侧边导航
- 移动端导航
- 登录页与主页面切换
- 基础 404 页面
- 路由占位页面

## 三、请求和登录基础设施

建立：

```text
src/services/http.ts
src/stores/auth.store.ts
src/stores/ui.store.ts
```

实现：

- Axios `baseURL`
- 自动添加 JWT
- 统一处理 401 并清理登录状态
- 登录信息保存和恢复
- 当前用户、角色和权限范围读取
- 路由权限判断

前端请求统一通过 `services/http.ts`，页面中不得重复创建 Axios 实例。

## 四、公共组件

优先使用 `animal-island-ui` 封装以下跨页面组件：

- `PageHeader`
- `FilterBar`
- `StatusTag`
- `DataTable`
- `FormDrawer`
- `ConfirmModal`
- `EmptyState`
- `LoadingState`
- `ErrorState`

公共组件先确认 Props 和视觉样式，之后尽量保持稳定，避免页面开发过程中反复修改。

## 五、公共接口类型

建立：

```text
src/types/api.ts
src/types/auth.ts
src/types/enums.ts
```

统一整理：

- 角色枚举
- 用户和权限类型
- 通用分页类型
- 统一错误响应
- 常用状态值
- 日期、金额和 ID 类型

页面专属的请求和响应类型继续放在各自 feature 的 `types.ts` 中。

## 六、给页面开发准备模板

每个页面模块使用统一结构：

```text
features/<module>/
├── pages/
├── components/
├── api.ts
├── types.ts
└── hooks.ts
```

页面 API 必须放在本模块的 `api.ts` 中，页面组件不直接拼接 URL 或操作 Axios。

可以先用 `campus` 页面作为简单示例，展示列表、筛选、详情和新增/编辑抽屉的完整写法。

## 七、推荐开发顺序

1. 确认前端工程可以安装、启动和构建。
2. 完成 `app`、布局、导航和路由占位。
3. 完成 Axios、JWT、登录状态和路由守卫。
4. 完成公共组件和公共类型。
5. 用 mock 数据完成一个简单的校园区域页面模板。
6. 再开始接入真实接口和正式业务页面。
7. 最后统一检查权限、加载状态、异常提示和移动端布局。

## 八、暂时不要做的事情

- 不要提前实现所有业务页面。
- 不要替其他成员完成其独立页面。
- 不要让每个页面单独创建 Axios 实例。
- 不要在每个页面重复实现表格、筛选、弹窗和状态标签。
- 不要在接口格式尚未确认时大量绑定复杂表单。
- 不要随意修改全局路由、主题 token 和公共组件 Props。

## 九、完成标准

基础部分完成后，应满足：

- 新成员可以按照目录说明新增一个页面。
- 页面可以直接使用统一请求实例和登录状态。
- 角色菜单和路由权限已经可用。
- 页面可以复用公共表格、筛选、弹窗和状态组件。
- 前端可以连接本地 ASP.NET Core API。
- `npm run build` 可以成功完成。
