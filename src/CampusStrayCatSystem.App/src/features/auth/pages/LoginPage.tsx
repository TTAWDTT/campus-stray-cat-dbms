import { Button, Card, Icon, Input, Tag } from 'animal-island-ui';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../../stores/auth.store';
import campusLogo from '../../../assets/images/campus-stray-cat-logo.png';

export function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((state) => state.login);
  const loading = useAuthStore((state) => state.loading);
  const error = useAuthStore((state) => state.error);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const submit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!username.trim() || !password) return;
    try {
      await login({ username: username.trim(), password });
      navigate('/', { replace: true });
    } catch {
      // 登录错误已经由 store 转换成页面提示。
    }
  };

  return (
    <main className="login-page">
      <section className="login-art">
        <div className="login-logo-wrap">
          <img className="login-logo" src={campusLogo} alt="校园猫岛 Logo" />
        </div>
        <p className="eyebrow">CAMPUS STRAY CAT</p>
        <h1>欢迎回到<br /><strong>校园猫岛</strong></h1>
        <p>把每一次目击、每一次救助，留在校园的温柔记录里。</p>
      </section>
      <Card className="login-card">
        <div className="login-heading"><Tag color="app-teal" variant="soft">本地演示</Tag><h2>登录系统</h2><p>使用测试账号进入运营台</p></div>
        <form onSubmit={submit}>
          <label htmlFor="username">账号</label>
          <Input className="login-input" id="username" value={username} onChange={(event) => setUsername(event.target.value)} placeholder="请输入用户名" autoComplete="username" />
          <label htmlFor="password">密码</label>
          <Input className="login-input" id="password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} placeholder="请输入密码" autoComplete="current-password" />
          <p className="login-error" role="alert" aria-live="polite">{error || ''}</p>
          <Button className="login-submit" htmlType="submit" type="primary" block loading={loading}>进入校园猫岛</Button>
        </form>
        <p className="login-hint"><Icon name="icon-design" size={14} /> 演示账号可使用 A 组管理员账号，密码为 Passw0rd!。</p>
      </Card>
    </main>
  );
}
