import { Button, Card, Icon, Progress, Tag } from 'animal-island-ui';
import { useAuthStore } from '../../../stores/auth.store';

const statCards = [
  { label: '校园猫咪', value: '38', note: '较上月增加 4 只', icon: 'icon-critterpedia' as const, color: 'app-teal' as const },
  { label: '本周目击', value: '126', note: '覆盖 12 个区域', icon: 'icon-map' as const, color: 'app-green' as const },
  { label: '进行中救助', value: '7', note: '其中 2 件需关注', icon: 'icon-camera' as const, color: 'app-orange' as const },
];

export function DashboardPage() {
  const profileName = useAuthStore((state) => state.user?.realName || state.user?.username || '校园伙伴');

  return (
    <div className="dashboard-page">
      <section className="dashboard-welcome">
        <div><p className="kicker">星期四 · 10 月 24 日 · 晴</p><h1>早上好，{profileName} <Icon name="icon-design" size={25} /></h1><p>今天也一起照顾校园里的小邻居吧。</p></div>
        <div className="dashboard-actions"><Button className="dashboard-action" type="default" icon={<Icon name="icon-map" size={17} />}>查看地图</Button><Button className="dashboard-action" type="primary" icon={<Icon name="icon-camera" size={17} />}>记录目击</Button></div>
      </section>
      <section className="stat-grid" aria-label="校园概览统计">
        {statCards.map((stat) => <Card key={stat.label} color={stat.color} pattern={stat.color} className="stat-card"><span className="stat-icon"><Icon name={stat.icon} size={23} /></span><span className="stat-copy"><small>{stat.label}</small><strong>{stat.value}</strong><em>{stat.note}</em></span></Card>)}
        <Card color="app-yellow" className="target-card"><div><small>本月绝育目标</small><strong>16 / 24</strong></div><Progress percent={67} showInfo={false} size="middle" aria-label="本月绝育目标完成度" /><em>还差 8 只猫咪</em></Card>
      </section>
      <section className="dashboard-columns">
        <Card className="today-card"><div className="section-heading"><div><h2>今天的岛屿便签</h2></div><Tag color="app-red" variant="soft">3 件待办</Tag></div><div className="todo-list"><div><Icon name="icon-camera" size={19} /><span><strong>北门猫窝需要检查</strong><small>今天 14:00 前完成维护记录</small></span></div><div><Icon name="icon-chat" size={19} /><span><strong>2 个领养申请待审核</strong><small>领养中心 · 昨天 18:32</small></span></div><div><Icon name="icon-diy" size={19} /><span><strong>芝麻的疫苗提醒</strong><small>医疗记录 · 10 月 28 日</small></span></div></div><Button block type="default">打开待办清单</Button></Card>
        <Card color="app-blue" className="insight-card"><Icon name="icon-map" size={29} /><p className="kicker">今日运营提醒</p><h2>已有 12 个校园区域更新目击记录。</h2><p>继续记录每一次发现，帮助志愿者掌握猫咪动态。</p></Card>
      </section>
    </div>
  );
}
