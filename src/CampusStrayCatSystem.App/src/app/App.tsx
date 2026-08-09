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
import { SystemPage } from '../features/system/pages/SystemPage';
import { useAuthStore } from '../stores/auth.store';
import { FinancePage } from '../features/finance/pages/FinancePage'
import { ProjectPage } from '../features/finance/pages/ProjectPage'
import { RecordsPage } from '../features/finance/pages/RecordsPage'
import { StatisticsPage } from '../features/finance/pages/StatisticsPage'
import { AdoptionPage} from '../features/adoption/pages/AdoptionPage'
import { VolunteerPage } from '../features/volunteer/pages/VolunteerPage'
import { VisitPage } from '../features/volunteer/pages/VisitPage'
import { AdoptionCheckPage } from '../features/volunteer/pages/AdoptionCheckPage'
import { ActivityPage } from '../features/volunteer/pages/ActivityPage'

function AdminOnly({ children }: { children: ReactNode }) {
  const user = useAuthStore((state) => state.user);
  const role = user?.roleName?.trim().toUpperCase();
  const permissions = (user?.permissionScope || '').split(',').map((permission) => permission.trim().toUpperCase());
  const canManageSystem = role === 'ADMIN' || permissions.some((permission) => ['USER_MANAGE', 'ROLE_MANAGE', 'BLACKLIST_MANAGE'].includes(permission));
  return canManageSystem ? children : <Navigate to="/" replace />;
}

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/preview/shared" element={<SharedComponentsPreview />} />
         { /*
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<LoginPage />} />
            </Route>
*/}
        <Route element={<RouteGuard />}>
          <Route element={<MainLayout />}>
            <Route index element={<DashboardPage />} />
            <Route path="cats" element={<CatsPage />} />
            <Route path="cats/:catId" element={<CatDetailPage />} />
            <Route path="campus" element={<CampusPage />} />
            <Route path="rescue" element={<PlaceholderPage title="救助中心" icon="icon-camera" description="TNR、医疗提醒、紧急上报和失踪预警。" />} />
            <Route path="adoption" element={<AdoptionPage/>} />
            <Route path="finance" element={<FinancePage />} />
            <Route path="finance/projects" element={<ProjectPage />} />
            <Route path="finance/records" element={<RecordsPage />} />
            <Route path="finance/statistics" element={<StatisticsPage />} />
            <Route path='volunteer' element={<VolunteerPage/>}/>
            <Route path="volunteer/visits" element={<VisitPage/>} />
            <Route path="volunteer/adoptions" element={<AdoptionCheckPage/>} />
            <Route path="volunteer/activity" element={<ActivityPage/>} />
            <Route path="system" element={<AdminOnly><SystemPage /></AdminOnly>} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
