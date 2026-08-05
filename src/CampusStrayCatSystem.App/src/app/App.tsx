import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import { AuthLayout } from './layouts/AuthLayout';
import { RouteGuard } from './guards/RouteGuard';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { DashboardPage } from '../features/dashboard/pages/DashboardPage';
import { PlaceholderPage } from '../shared/components/PlaceholderPage';
import { SharedComponentsPreview } from '../shared/components/SharedComponentsPreview';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/preview/shared" element={<SharedComponentsPreview />} />
        <Route element={<AuthLayout />}>
          <Route path="/login" element={<LoginPage />} />
        </Route>
        <Route element={<RouteGuard />}>
          <Route element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="cats" element={<PlaceholderPage title="猫咪档案" icon="icon-critterpedia" description="猫咪列表、照片、特征和命名投票会在这里汇总。" />} />
            <Route path="campus" element={<PlaceholderPage title="校园地图" icon="icon-map" description="区域、服务点、猫窝和目击记录的入口。" />} />
            <Route path="rescue" element={<PlaceholderPage title="救助中心" icon="icon-camera" description="TNR、医疗提醒、紧急上报和失踪预警。" />} />
            <Route path="adoption" element={<PlaceholderPage title="领养与志愿者" icon="icon-chat" description="领养审核、排班、投喂和任务交接。" />} />
            <Route path="finance" element={<PlaceholderPage title="财务公示" icon="icon-shopping" description="众筹、捐赠、支出和统计快照。" />} />
            <Route path="system" element={<PlaceholderPage title="系统管理" icon="icon-design" description="用户、角色、权限和黑名单管理。" />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
