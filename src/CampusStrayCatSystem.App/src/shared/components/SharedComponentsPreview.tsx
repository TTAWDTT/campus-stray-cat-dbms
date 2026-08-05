import { Button, Card, Divider, Input, Tag } from 'animal-island-ui';
import { EmptyState } from './EmptyState';
import { PageHeader } from './PageHeader';
import { StatusTag } from './StatusTag';

export function SharedComponentsPreview() {
  return (
    <main className="shared-preview-page">
      <PageHeader
        kicker="SHARED COMPONENTS"
        title="公共组件预览"
        description="第一阶段建立的页面标题、状态标签、空状态和操作区。"
        icon="icon-critterpedia"
        actions={<><Button type="default">导出</Button><Button type="primary">新增记录</Button></>}
      />

      <section className="shared-preview-grid">
        <Card className="shared-preview-card">
          <div className="shared-preview-label">PageHeader / 操作区</div>
          <p>页面开发者只需要传入标题、说明、图标和右侧按钮。</p>
          <div className="shared-preview-actions"><Button type="default">筛选</Button><Button type="primary">记录目击</Button></div>
        </Card>
        <Card className="shared-preview-card">
          <div className="shared-preview-label">StatusTag / 状态标签</div>
          <p>统一不同页面的状态颜色，业务模块只传状态值。</p>
          <div className="shared-preview-tags"><StatusTag value="ACTIVE" label="进行中" /><StatusTag value="PENDING" label="待审核" /><StatusTag value="COMPLETED" label="已完成" /><StatusTag value="REJECTED" label="已驳回" /></div>
        </Card>
        <Card className="shared-preview-card">
          <div className="shared-preview-label">筛选栏组合</div>
          <div className="shared-preview-form"><Input placeholder="按名称搜索" /><Tag color="app-teal" variant="soft">12 条记录</Tag><Button type="default">重置</Button></div>
          <Divider />
          <p>后续各页面的表格筛选可以沿用同一间距和排列规则。</p>
        </Card>
        <EmptyState icon="icon-map" title="暂无校园区域" description="接口接入后，这里会展示区域层级和服务点位。" action={<Button type="primary">新增区域</Button>} />
      </section>
    </main>
  );
}
