import type { ReactNode } from 'react';
import { Button, Card, Icon } from 'animal-island-ui';
import type { IconName } from 'animal-island-ui';

interface EmptyStateProps {
  title: string;
  description: string;
  icon?: IconName;
  action?: ReactNode;
}

export function EmptyState({ title, description, icon = 'icon-critterpedia', action }: EmptyStateProps) {
  return (
    <Card className="feature-empty-state">
      <span className="feature-empty-icon"><Icon name={icon} size={30} /></span>
      <h2>{title}</h2>
      <p>{description}</p>
      {action || <Button type="primary" disabled>等待接口接入</Button>}
    </Card>
  );
}
