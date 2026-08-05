import type { ReactNode } from 'react';
import { Icon, Tag } from 'animal-island-ui';
import type { IconName } from 'animal-island-ui';

interface PageHeaderProps {
  title: string;
  description?: string;
  kicker?: string;
  icon?: IconName;
  actions?: ReactNode;
}

export function PageHeader({ title, description, kicker, icon, actions }: PageHeaderProps) {
  return (
    <header className="feature-page-header">
      <div className="feature-page-heading">
        <div className="feature-page-title-row">
          {icon && <span className="feature-page-icon"><Icon name={icon} size={19} /></span>}
          <div>
            {kicker && <Tag color="app-teal" variant="soft" size="small">{kicker}</Tag>}
            <h1>{title}</h1>
          </div>
        </div>
        {description && <p>{description}</p>}
      </div>
      {actions && <div className="feature-page-actions">{actions}</div>}
    </header>
  );
}
