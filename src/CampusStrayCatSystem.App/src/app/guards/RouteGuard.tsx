import { useEffect } from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { Card, Icon } from 'animal-island-ui';
import { useAuthStore } from '../../stores/auth.store';

export function RouteGuard() {
  /*const { token, user, ready, restore } = useAuthStore();

  useEffect(() => {
    if (!ready) void restore();
  }, [ready, restore]);

  if (!ready) {
    return <div className="route-loading"><Card><Icon name="icon-critterpedia" size={32} bounce /><p>正在确认校园猫岛登录状态…</p></Card></div>;
  }

  if (!token || !user) return <Navigate to="/login" replace />;*/
  return <Outlet />;
}
