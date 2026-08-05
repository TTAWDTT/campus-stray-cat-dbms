import { Outlet } from 'react-router-dom';

// 登录接口接入前，先保留统一的路由守卫入口，避免各页面重复处理权限。
export function RouteGuard() {
  return <Outlet />;
}
