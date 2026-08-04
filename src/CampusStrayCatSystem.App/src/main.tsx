import React from 'react';
import ReactDOM from 'react-dom/client';
import { Button, Card, Icon, Progress, Tag } from 'animal-island-ui';
import 'animal-island-ui/style';
import './styles.css';

type Cat = {
  name: string;
  area: string;
  status: string;
  statusColor: 'app-green' | 'app-yellow' | 'app-teal';
  image: string;
};

const cats: Cat[] = [
  {
    name: '芝麻',
    area: '图书馆东侧',
    status: '今日已目击',
    statusColor: 'app-green',
    image: 'https://images.unsplash.com/photo-1518791841217-8f162f1e1131?auto=format&fit=crop&w=800&q=85',
  },
  {
    name: '小岛',
    area: '北门猫窝',
    status: '等待回访',
    statusColor: 'app-yellow',
    image: 'https://images.unsplash.com/photo-1573865526739-10659fec78a5?auto=format&fit=crop&w=800&q=85',
  },
  {
    name: '花生',
    area: '食堂后门',
    status: '已绝育',
    statusColor: 'app-teal',
    image: 'https://images.unsplash.com/photo-1533745922647-10e936b3f9f8?auto=format&fit=crop&w=800&q=85',
  },
];

const stats = [
  { label: '校园猫咪', value: '38', note: '较上月 +4', icon: 'icon-critterpedia', color: 'app-teal' as const },
  { label: '本周目击', value: '126', note: '覆盖 12 个区域', icon: 'icon-map', color: 'app-green' as const },
  { label: '进行中救助', value: '7', note: '其中 2 件需关注', icon: 'icon-camera', color: 'app-orange' as const },
];

function App() {
  const [activeTab, setActiveTab] = React.useState('概览');

  return (
    <div className="island-app">
      <header className="topbar">
        <div className="brand-lockup">
          <div className="brand-mark"><Icon name="icon-critterpedia" size={25} /></div>
          <div>
            <span className="eyebrow">CAMPUS STRAY CAT</span>
            <h1>校园猫岛</h1>
          </div>
        </div>
        <nav className="nav-pills" aria-label="主导航">
          {['概览', '猫咪档案', '目击打卡', '救助中心'].map((item) => (
            <button
              key={item}
              className={activeTab === item ? 'nav-pill active' : 'nav-pill'}
              onClick={() => setActiveTab(item)}
            >
              {item}
            </button>
          ))}
        </nav>
        <div className="profile-chip">
          <span className="profile-dot">罗</span>
          <span className="profile-name">罗臻</span>
          <Tag className="profile-role" size="small" color="app-teal" variant="soft">管理员</Tag>
        </div>
      </header>

      <main className="page-shell">
        <section className="welcome-row">
          <div>
            <p className="kicker">星期四 · 10 月 24 日 · 晴</p>
            <h2>早上好，罗臻 <Icon name="icon-design" size={24} /></h2>
            <p className="lede">今天也一起照顾校园里的小邻居吧。</p>
          </div>
          <div className="hero-actions">
            <Button className="hero-button" type="default" icon={<Icon name="icon-map" size={17} />}>查看校园地图</Button>
            <Button className="hero-button" type="primary" icon={<Icon name="icon-camera" size={17} />}>记录一次目击</Button>
          </div>
        </section>

        <section className="stats-grid" aria-label="校园概览统计">
          {stats.map((stat) => (
            <Card key={stat.label} color={stat.color} pattern={stat.color} className="stat-card">
              <div className="stat-icon"><Icon name={stat.icon as 'icon-map'} size={24} /></div>
              <div className="stat-copy">
                <span>{stat.label}</span>
                <strong>{stat.value}</strong>
                <small>{stat.note}</small>
              </div>
            </Card>
          ))}
          <Card color="app-yellow" className="progress-card">
            <div className="progress-head"><span>本月绝育目标</span><strong>16 / 24</strong></div>
            <Progress percent={67} showInfo={false} size="middle" aria-label="本月绝育目标完成度" />
            <small>还差 8 只猫咪，一起加油。</small>
          </Card>
        </section>

        <section className="content-grid">
          <div className="cats-column">
            <div className="section-heading">
              <div>
                <p className="kicker">最近更新</p>
                <h3>猫咪小档案</h3>
              </div>
              <Button type="link">查看全部</Button>
            </div>
            <div className="cat-grid">
              {cats.map((cat) => (
                <Card key={cat.name} hoverable className="cat-card">
                  <div className="cat-photo-wrap">
                    <img src={cat.image} alt={`${cat.name}的照片`} />
                    <Tag className="cat-status" size="small" color={cat.statusColor} variant="solid">{cat.status}</Tag>
                  </div>
                  <div className="cat-info">
                    <div>
                      <h4>{cat.name}</h4>
                      <p><Icon name="icon-map" size={14} /> {cat.area}</p>
                    </div>
                    <Button type="text" size="small" aria-label={`查看${cat.name}详情`}>详情</Button>
                  </div>
                </Card>
              ))}
            </div>
          </div>

          <aside className="notice-column">
            <div className="section-heading compact-heading">
              <div><p className="kicker">待处理事项</p><h3>岛上便签</h3></div>
              <span className="notice-count">3</span>
            </div>
            <Card className="notice-card">
              <div className="notice-item urgent">
                <span className="notice-badge"><Icon name="icon-camera" size={17} /></span>
                <div><strong>北门猫窝需要检查</strong><p>猫窝维护记录 · 今天 14:00 前</p></div>
              </div>
              <div className="notice-item">
                <span className="notice-badge mint"><Icon name="icon-chat" size={17} /></span>
                <div><strong>2 个领养申请待审核</strong><p>领养中心 · 昨天 18:32</p></div>
              </div>
              <div className="notice-item">
                <span className="notice-badge yellow"><Icon name="icon-diy" size={17} /></span>
                <div><strong>芝麻的疫苗提醒</strong><p>医疗记录 · 10 月 28 日</p></div>
              </div>
              <Button block type="default">打开待办清单</Button>
            </Card>

            <Card color="app-blue" className="quote-card">
              <span className="quote-mark">“</span>
              <p>每一次目击，都是给它们留下的一盏小灯。</p>
              <span className="quote-sign">— 校园志愿者手册</span>
            </Card>
          </aside>
        </section>
      </main>

      <footer className="footer-note">校园猫岛 · 让每一只猫咪都有被看见的记录</footer>
    </div>
  );
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode><App /></React.StrictMode>,
);
