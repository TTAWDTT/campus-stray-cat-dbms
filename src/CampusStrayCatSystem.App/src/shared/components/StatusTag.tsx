import { Tag } from 'animal-island-ui';
import type { TagColor } from 'animal-island-ui';

const colors: Record<string, TagColor> = {
  ACTIVE: 'app-green',
  COMPLETED: 'app-green',
  VERIFIED: 'app-green',
  PENDING: 'app-yellow',
  PROCESSING: 'app-yellow',
  DISABLED: 'app-red',
  REJECTED: 'app-red',
  CLOSED: 'default',
  ON_CAMPUS: 'app-green',
  MISSING: 'app-yellow',
  ADOPTED: 'app-teal',
  DECEASED: 'default',
  DRAFT: 'default',
  PUBLISHED: 'app-green',
  ARCHIVED: 'default',
};

interface StatusTagProps {
  value: string;
  label?: string;
}

export function StatusTag({ value, label = value }: StatusTagProps) {
  return <Tag color={colors[value.toUpperCase()] ?? 'app-teal'} variant="soft" size="small">{label}</Tag>;
}
