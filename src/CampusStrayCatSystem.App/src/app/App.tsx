import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import { MainLayout } from './layouts/MainLayout';
import { AuthLayout } from './layouts/AuthLayout';
import { RouteGuard } from './guards/RouteGuard';
import { LoginPage } from '../features/auth/pages/LoginPage';
import { DashboardPage } from '../features/dashboard/pages/DashboardPage';
import { PlaceholderPage } from '../shared/components/PlaceholderPage';
import { SharedComponentsPreview } from '../shared/components/SharedComponentsPreview';
import { CatsPage } from '../features/cats/pages/CatsPage';
import { CatDetailPage } from '../features/cats/pages/CatDetailPage';
import { CampusPage } from '../features/campus/pages/CampusPage';
import { useAuthStore } from '../stores/auth.store';
import { FinancePage } from '../features/finance/pages/FinancePage'
import { ProjectPage } from '../features/finance/pages/ProjectPage'
import { RecordsPage } from '../features/finance/pages/RecordsPage'
import { StatisticsPage } from '../features/finance/pages/StatisticsPage'
function AdminOnly({ children }: { children: ReactNode }) {
  const role = useAuthStore((state) => state.user?.roleName?.toUpperCase());
  return role === 'ADMIN' ? children : <Navigate to="/" replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/preview/shared" element={<SharedComponentsPreview />} />
         {/* <Route element={<AuthLayout />}>
            <Route path="/login" element={<LoginPage />} />
          </Route> */}
        <Route element={<RouteGuard />}>
          <Route element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="cats" element={<CatsPage />} />
            <Route path="cats/:catId" element={<CatDetailPage />} />
            <Route path="campus" element={<CampusPage />} />
            <Route path="rescue" element={<PlaceholderPage title="救助中心" icon="icon-camera" description="TNR、医疗提醒、紧急上报和失踪预警。" />} />
            <Route path="adoption" element={<PlaceholderPage title="领养与志愿者" icon="icon-chat" description="领养审核、排班、投喂和任务交接。" />} />
            <Route path="finance" element={<FinancePage />} />
            <Route path="finance/projects" element={<ProjectPage />} />
            <Route path="finance/records" element={<RecordsPage />} />
            <Route path="finance/statistics" element={<StatisticsPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
