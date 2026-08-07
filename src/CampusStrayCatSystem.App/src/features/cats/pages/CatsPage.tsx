import axios from 'axios';
import { useEffect, useMemo, useRef, useState } from 'react';
import { Button, Card, Icon, Input, Modal, Select, Table, Tag } from 'animal-island-ui';
import type { TableColumn } from 'animal-island-ui';
import { PageHeader } from '../../../shared/components/PageHeader';
import { EmptyState } from '../../../shared/components/EmptyState';
import { StatusTag } from '../../../shared/components/StatusTag';
import { useAuthStore } from '../../../stores/auth.store';
import { useNavigate } from 'react-router-dom';
import { catsService } from '../../../services/cats.service';
import { emptyCatForm } from '../../../types/cats';
import type { CampusArea, CatArchiveStatus, CatFormState, CatLifeStatus, CatSummary, CatWritePayload, BinaryFlag } from '../../../types/cats';

const lifeOptions = [
  { key: '', label: '全部生活状态' },
  { key: 'ON_CAMPUS', label: '在校园' },
  { key: 'MISSING', label: '失踪' },
  { key: 'ADOPTED', label: '已领养' },
  { key: 'DECEASED', label: '已离世' },
];

const archiveOptions = [
  { key: '', label: '全部档案状态' },
  { key: 'DRAFT', label: '草稿' },
  { key: 'PUBLISHED', label: '已发布' },
  { key: 'ARCHIVED', label: '已归档' },
];

const genderOptions = [
  { key: 'UNKNOWN', label: '未知' },
  { key: 'MALE', label: '公猫' },
  { key: 'FEMALE', label: '母猫' },
];

const statusLabel: Record<string, string> = {
  ON_CAMPUS: '在校园', MISSING: '失踪', ADOPTED: '已领养', DECEASED: '已离世',
  DRAFT: '草稿', PUBLISHED: '已发布', ARCHIVED: '已归档',
};

const PAGE_SIZE = 5;

const genderLabel: Record<string, string> = { UNKNOWN: '未知', MALE: '公猫', FEMALE: '母猫' };

const readError = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data?.message === 'string') return data.message;
    if (typeof data === 'string') return data;
    if (error.response?.status === 403) return '当前账号没有维护猫咪档案的权限。';
    if (error.response?.status === 401) return '登录状态已失效，请重新登录。';
  }
  return '暂时无法连接猫咪档案服务，请稍后重试。';
};

const toPayload = (form: CatFormState): CatWritePayload => ({
  catName: form.catName.trim(),
  gender: form.gender,
  breed: form.breed.trim(),
  colorPattern: form.colorPattern.trim(),
  sterilizedFlag: Number(form.sterilizedFlag),
  earTipFlag: Number(form.earTipFlag),
  personalityTags: form.personalityTags.trim(),
  mainAreaId: form.mainAreaId,
  lifeStatus: form.lifeStatus,
  archiveStatus: form.archiveStatus,
});

export function CatsPage() {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const canManage = ['ADMIN', 'VOLUNTEER'].includes((user?.roleName || '').toUpperCase());
  const [cats, setCats] = useState<CatSummary[]>([]);
  const [areas, setAreas] = useState<CampusArea[]>([]);
  const [search, setSearch] = useState('');
  const [lifeStatus, setLifeStatus] = useState<CatLifeStatus | ''>('');
  const [archiveStatus, setArchiveStatus] = useState<CatArchiveStatus | ''>('');
  const [areaID, setAreaID] = useState('');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [modalClosing, setModalClosing] = useState(false);
  const [editing, setEditing] = useState<CatSummary | null>(null);
  const [form, setForm] = useState<CatFormState>(emptyCatForm);
  const closeTimer = useRef<number | null>(null);
  const previousBodyPadding = useRef('');

  const loadCats = async () => {
    setLoading(true);
    setError('');
    try {
      setCats(await catsService.list({ mainAreaId: areaID, lifeStatus: lifeStatus || undefined, archiveStatus: archiveStatus || undefined }));
    } catch (loadError) {
      setError(readError(loadError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void catsService.areas().then(setAreas).catch(() => setAreas([]));
  }, []);

  useEffect(() => {
    void loadCats();
    // 筛选项变化时刷新真实数据。
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [areaID, lifeStatus, archiveStatus]);

  const visibleCats = useMemo(() => {
    const keyword = search.trim().toLowerCase();
    if (!keyword) return cats;
    return cats.filter((cat) => [cat.catName, cat.catID, cat.colorPattern, cat.breed].some((value) => value?.toLowerCase().includes(keyword)));
  }, [cats, search]);

  const pageCount = Math.max(1, Math.ceil(visibleCats.length / PAGE_SIZE));
  const pageCats = visibleCats.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE);

  useEffect(() => {
    setPage(1);
  }, [search, areaID, lifeStatus, archiveStatus]);

  useEffect(() => {
    if (page > pageCount) setPage(pageCount);
  }, [page, pageCount]);

  useEffect(() => () => {
    if (closeTimer.current !== null) window.clearTimeout(closeTimer.current);
    document.body.style.paddingRight = previousBodyPadding.current;
  }, []);

  const areaOptions = useMemo(() => [
    { key: '', label: '全部校园区域' },
    ...areas.map((area) => ({ key: area.areaID, label: area.areaName || area.areaID })),
  ], [areas]);

  const openModal = () => {
    if (closeTimer.current !== null) window.clearTimeout(closeTimer.current);
    if (!modalOpen) {
      previousBodyPadding.current = document.body.style.paddingRight;
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      document.body.style.paddingRight = scrollbarWidth > 0 ? `${scrollbarWidth}px` : previousBodyPadding.current;
    }
    setModalClosing(false);
    setModalOpen(true);
  };

  const closeModal = (force = false) => {
    if (saving && !force) return;
    if (!modalOpen || modalClosing) return;
    setModalClosing(true);
    closeTimer.current = window.setTimeout(() => {
      setModalOpen(false);
      setModalClosing(false);
      document.body.style.paddingRight = previousBodyPadding.current;
      closeTimer.current = null;
    }, 220);
  };

  const openCreate = () => {
    setEditing(null);
    setForm({ ...emptyCatForm });
    setError('');
    openModal();
  };

  const openEdit = (cat: CatSummary) => {
    setEditing(cat);
    setForm({
      catName: cat.catName || '', gender: cat.gender || 'UNKNOWN', breed: cat.breed || '', colorPattern: cat.colorPattern || '',
      sterilizedFlag: String(cat.sterilizedFlag ?? 0) as BinaryFlag, earTipFlag: String(cat.earTipFlag ?? 0) as BinaryFlag, personalityTags: cat.personalityTags || '',
      mainAreaId: cat.mainAreaId || '', lifeStatus: cat.lifeStatus || 'ON_CAMPUS', archiveStatus: cat.archiveStatus || 'DRAFT',
    });
    setError('');
    openModal();
  };

  const updateField = (field: keyof CatFormState, value: string) => setForm((current) => ({ ...current, [field]: value }));

  const submit = async () => {
    if (!form.catName.trim() || !form.colorPattern.trim()) {
      setError('请至少填写猫咪名称和花色。');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const payload = toPayload(form);
      if (editing) {
        await catsService.update(editing.catID, payload);
        setNotice(`“${form.catName.trim()}”的档案已更新。`);
      } else {
        await catsService.create(payload);
        setNotice(`“${form.catName.trim()}”已加入猫咪档案。`);
      }
      closeModal(true);
      await loadCats();
    } catch (saveError) {
      setError(readError(saveError));
    } finally {
      setSaving(false);
    }
  };

  const archive = async (cat: CatSummary) => {
    if (!window.confirm(`确定要归档“${cat.catName || '未命名猫咪'}”吗？归档后不会物理删除记录。`)) return;
    setError('');
    try {
      await catsService.archive(cat.catID);
      setNotice(`“${cat.catName || '未命名猫咪'}”已归档。`);
      await loadCats();
    } catch (archiveError) {
      setError(readError(archiveError));
    }
  };

  const rowCat = (row: Record<string, unknown>) => row as unknown as CatSummary;
  const columns: TableColumn[] = [
    { title: '猫咪', width: 150, render: (_value, rawRow) => { const row = rowCat(rawRow); return <strong className="cat-table-name">{row.catName || '未命名猫咪'}</strong>; } },
    { title: '性别', width: 82, render: (_value, rawRow) => { const row = rowCat(rawRow); return genderLabel[row.gender || 'UNKNOWN'] || '未知'; } },
    { title: '花色', width: 118, render: (_value, rawRow) => rowCat(rawRow).colorPattern || '未记录' },
    { title: '品种', width: 148, render: (_value, rawRow) => rowCat(rawRow).breed || '未记录' },
    { title: '活动区域', width: 136, dataIndex: 'mainAreaName', render: (value, rawRow) => { const row = rowCat(rawRow); return (typeof value === 'string' && value) || row.mainAreaId || '暂未关联'; } },
    { title: '生活状态', width: 108, render: (_value, rawRow) => { const row = rowCat(rawRow); const value = row.lifeStatus || 'ON_CAMPUS'; return <StatusTag value={value} label={statusLabel[value] || value} />; } },
    { title: '档案', width: 96, render: (_value, rawRow) => { const row = rowCat(rawRow); const value = row.archiveStatus || 'DRAFT'; return <StatusTag value={value} label={statusLabel[value] || value} />; } },
    { title: '操作', width: 196, align: 'right', render: (_value, rawRow) => { const row = rowCat(rawRow); return <div className="cat-table-actions"><Button type="text" size="small" onClick={() => navigate(`/cats/${encodeURIComponent(row.catID)}`)}>查看</Button>{canManage && <Button type="text" size="small" onClick={() => openEdit(row)}>编辑</Button>}{canManage && row.archiveStatus !== 'ARCHIVED' && <Button type="text" size="small" onClick={() => void archive(row)}>归档</Button>}</div>; } },
  ];

  return (
    <section className="feature-page cats-page">
      <PageHeader kicker="CAT ARCHIVE" title="猫咪档案" icon="icon-critterpedia" actions={canManage && <Button type="primary" icon={<Icon name="icon-diy" size={16} />} onClick={openCreate}>新增猫咪</Button>} />
      <div className="cats-summary-row"><Card color="app-teal" className="cats-summary-card"><Icon name="icon-critterpedia" size={23} /><span><small>当前筛选结果</small><strong>{visibleCats.length} <em>只</em></strong></span></Card><Card color="app-yellow" className="cats-summary-card"><Icon name="icon-camera" size={23} /><span><small>已完成绝育</small><strong>{cats.filter((cat) => cat.sterilizedFlag === 1).length} <em>只</em></strong></span></Card><Card color="app-green" className="cats-summary-card"><Icon name="icon-map" size={23} /><span><small>已关联区域</small><strong>{cats.filter((cat) => cat.mainAreaId).length} <em>只</em></strong></span></Card></div>
      <Card className="cats-filter-card"><div className="cats-filter-heading"><div><strong>档案索引</strong></div>{notice && <Tag color="app-green" variant="soft">{notice}</Tag>}</div><div className="cats-filter-grid"><Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="搜索名称、编号或花色" allowClear prefix={<Icon name="icon-critterpedia" size={15} />} /><Select options={areaOptions} value={areaID} onChange={setAreaID} aria-label="按区域筛选" /><Select options={lifeOptions} value={lifeStatus} onChange={(value) => setLifeStatus(value as CatLifeStatus | '')} aria-label="按生活状态筛选" /><Select options={archiveOptions} value={archiveStatus} onChange={(value) => setArchiveStatus(value as CatArchiveStatus | '')} aria-label="按档案状态筛选" /><Button type="default" onClick={() => { setSearch(''); setAreaID(''); setLifeStatus(''); setArchiveStatus(''); }}>重置</Button></div></Card>
      {error && <div className="cats-alert" role="alert"><Icon name="icon-camera" size={17} /><span>{error}</span><Button type="text" size="small" onClick={() => setError('')}>知道了</Button></div>}
      {loading || visibleCats.length > 0 ? <Card className="cats-table-card"><div key={page} className="cats-table-page"><Table columns={columns} dataSource={pageCats as unknown as Record<string, unknown>[]} rowKey="catID" loading={loading} emptyText="没有符合条件的猫咪档案" scroll={{ x: 900 }} /></div><div className="cats-pagination"><span>第 {page} / {pageCount} 页 · 共 {visibleCats.length} 条</span><div><Button className="cats-page-button" type="default" size="small" disabled={page <= 1} onClick={() => setPage((current) => Math.max(1, current - 1))}>上一页</Button><Button className="cats-page-button" type="default" size="small" disabled={page >= pageCount} onClick={() => setPage((current) => Math.min(pageCount, current + 1))}>下一页</Button></div></div></Card> : <EmptyState icon="icon-critterpedia" title="岛上还没有猫咪档案" description={canManage ? '先录入第一只校园小邻居，后续照片、目击和医疗记录都可以从这里延伸。' : '管理员或志愿者录入档案后，这里会展示校园猫咪。'} action={canManage ? <Button type="primary" onClick={openCreate}>新增第一只猫咪</Button> : undefined} />}
      <Modal open={modalOpen} className={modalClosing ? 'cat-modal-closing' : 'cat-modal-opening'} maskStyle={{ animation: modalClosing ? 'cats-modal-mask-out .22s var(--animal-motion-ease) both' : undefined }} title={editing ? '编辑猫咪档案' : '新增猫咪档案'} width={680} typewriter={false} onClose={closeModal} footer={<div className="cat-modal-footer"><Button type="default" onClick={() => closeModal()} disabled={saving}>取消</Button><Button type="primary" onClick={() => void submit()} loading={saving}>{editing ? '保存修改' : '加入档案'}</Button></div>}>
        <div className="cat-form"><div className="cat-form-intro"><Icon name="icon-critterpedia" size={22} /><span><strong>{editing ? '更新这只小邻居的档案' : '记录一只新的校园小邻居'}</strong><small>带 * 的字段会直接参与档案检索和后续业务。</small></span></div><div className="cat-form-grid"><label><span>猫咪名称 *</span><Input value={form.catName} onChange={(event) => updateField('catName', event.target.value)} placeholder="例如：芝麻" /></label><label><span>性别</span><Select options={genderOptions} value={form.gender} onChange={(value) => updateField('gender', value)} /></label><label><span>花色 *</span><Input value={form.colorPattern} onChange={(event) => updateField('colorPattern', event.target.value)} placeholder="例如：狸花、黑白" /></label><label><span>品种</span><Input value={form.breed} onChange={(event) => updateField('breed', event.target.value)} placeholder="例如：中华田园猫" /></label><label><span>活动区域</span><Select options={[{ key: '', label: '暂不关联区域' }, ...areas.map((area) => ({ key: area.areaID, label: area.areaName || area.areaID }))]} value={form.mainAreaId} onChange={(value) => updateField('mainAreaId', value)} /></label><label><span>生活状态</span><Select options={lifeOptions.slice(1)} value={form.lifeStatus} onChange={(value) => updateField('lifeStatus', value)} /></label><label><span>是否绝育</span><Select options={[{ key: '0', label: '未绝育' }, { key: '1', label: '已绝育' }]} value={form.sterilizedFlag} onChange={(value) => updateField('sterilizedFlag', value)} /></label><label><span>是否剪耳</span><Select options={[{ key: '0', label: '未剪耳' }, { key: '1', label: '已剪耳' }]} value={form.earTipFlag} onChange={(value) => updateField('earTipFlag', value)} /></label><label className="cat-form-wide"><span>性格标签</span><Input value={form.personalityTags} onChange={(event) => updateField('personalityTags', event.target.value)} placeholder="例如：亲人，胆小，爱叫" /></label>{editing && <label><span>档案状态</span><Select options={archiveOptions.slice(1)} value={form.archiveStatus} onChange={(value) => updateField('archiveStatus', value)} /></label>}</div></div>
      </Modal>
    </section>
  );
}
