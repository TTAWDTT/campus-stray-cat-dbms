import { useState } from 'react';
import { Icon, Tag } from 'animal-island-ui';
import { NavLink, Outlet, useLocation } from 'react-router-dom';
import campusLogo from '../../assets/images/campus-stray-cat-logo.png';
import { useAuthStore } from '../../stores/auth.store';

const navItems = [
  { label: '概览', to: '/', icon: 'icon-miles' as const },
  { label: '猫咪档案', to: '/cats', icon: 'icon-critterpedia' as const },
  { label: '校园地图', to: '/campus', icon: 'icon-map' as const },
  { label: '救助中心', to: '/rescue', icon: 'icon-camera' as const },
  { label: '领养与志愿者', to: '/adoption', icon: 'icon-chat' as const },
  { label: '财务公示', to: '/finance', icon: 'icon-shopping' as const },
  { label: '系统管理', to: '/system', icon: 'icon-design' as const },
];

export function MainLayout() {
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const location = useLocation();
  const user = useAuthStore((state) => state.user);
  const profileName = user?.realName || user?.username || '用户';
  const profileRole = user?.roleName || '普通用户';
  const profileInitial = profileName.slice(0, 1);

  return (
    <div className={sidebarCollapsed ? 'shell sidebar-collapsed' : 'shell'}>
      <aside className="desktop-sidebar">
        <div className="sidebar-brand">
          <span className="brand-mark"><img src={campusLogo} alt="校园猫岛 Logo" /></span>
          <span><small>CAMPUS STRAY CAT</small><strong>校园猫岛</strong></span>
        </div>
        <button
          type="button"
          className="sidebar-toggle"
          aria-label={sidebarCollapsed ? '展开侧栏' : '收起侧栏'}
          aria-expanded={!sidebarCollapsed}
          title={sidebarCollapsed ? '展开侧栏' : '收起侧栏'}
          onClick={() => setSidebarCollapsed((collapsed) => !collapsed)}
        >
          <span className="sidebar-toggle-glyph" aria-hidden="true" />
        </button>
        <nav className="sidebar-nav" aria-label="主菜单">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.to === '/'} className={({ isActive }) => isActive ? 'shell-link active' : 'shell-link'}>
              <Icon name={item.icon} size={19} className="shell-link-icon" />
              <span className="shell-link-label">{item.label}</span>
            </NavLink>
          ))}
        </nav>
        <div className="sidebar-footnote">
          <Icon name="icon-critterpedia" size={18} />
          <span>CAMPUS STRAY CAT</span>
        </div>
      </aside>

      <div className="shell-main">
        <header className="mobile-topbar">
          <div className="sidebar-brand">
            <span className="brand-mark"><img src={campusLogo} alt="校园猫岛 Logo" /></span>
            <span><small>CAMPUS STRAY CAT</small><strong>校园猫岛</strong></span>
          </div>
          <div className="profile-chip profile-capsule"><span className="profile-dot">{profileInitial}</span><span className="profile-name">{profileName}</span><Tag className="profile-role" size="small" color="app-teal" variant="soft">{profileRole}</Tag></div>
        </header>
        <main className="shell-content">
          <div className="shell-profile-row"><div className="profile-chip profile-capsule"><span className="profile-dot">{profileInitial}</span><span className="profile-name">{profileName}</span><Tag size="small" color="app-teal" variant="soft">{profileRole}</Tag></div></div>
          <div key={location.key} className="page-transition"><Outlet /></div>
        </main>
      </div>

      <nav className="mobile-bottom-nav" aria-label="移动端主菜单">
        {navItems.slice(0, 4).map((item) => (
          <NavLink key={item.to} to={item.to} end={item.to === '/'} className={({ isActive }) => isActive ? 'mobile-shell-link active' : 'mobile-shell-link'}>
            <Icon name={item.icon} size={20} /><span>{item.label}</span>
          </NavLink>
        ))}
      </nav>
    </div>
  );
}
