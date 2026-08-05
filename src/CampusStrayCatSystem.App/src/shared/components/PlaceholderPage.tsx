import { Button, Card, Icon, Tag } from 'animal-island-ui';
import type { IconName } from 'animal-island-ui';

type PlaceholderPageProps = { title: string; description: string; icon: IconName };

export function PlaceholderPage({ title, description, icon }: PlaceholderPageProps) {
  return <section className="placeholder-page"><Card color="app-teal" className="placeholder-card"><span className="placeholder-icon"><Icon name={icon} size={34} /></span><Tag color="app-teal" variant="soft">页面骨架</Tag><h1>{title}</h1><p>{description}</p><Button type="primary">准备接入接口</Button></Card></section>;
}
