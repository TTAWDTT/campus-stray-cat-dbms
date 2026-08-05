import { Button, Card, Icon, Input, Tag } from 'animal-island-ui';
import { useNavigate } from 'react-router-dom';

export function LoginPage() {
  const navigate = useNavigate();

  return (
    <main className="login-page">
      <section className="login-art">
        <span className="login-orbit orbit-one" />
        <span className="login-orbit orbit-two" />
        <Icon name="icon-critterpedia" size={84} bounce />
        <p className="eyebrow">CAMPUS STRAY CAT</p>
        <h1>欢迎回到<br /><strong>校园猫岛</strong></h1>
        <p>把每一次目击、每一次救助，留在校园的温柔记录里。</p>
      </section>
      <Card className="login-card">
        <div className="login-heading"><Tag color="app-teal" variant="soft">本地演示</Tag><h2>登录系统</h2><p>使用测试账号进入运营台</p></div>
        <label htmlFor="username">账号</label>
        <Input className="login-input" id="username" placeholder="请输入用户名" />
        <label htmlFor="password">密码</label>
        <Input className="login-input" id="password" type="password" placeholder="请输入密码" />
        <Button type="primary" block onClick={() => navigate('/')}>进入校园猫岛</Button>
        <p className="login-hint"><Icon name="icon-design" size={14} /> 登录接口接入后，这里会连接真实 JWT 鉴权。</p>
      </Card>
    </main>
  );
}
