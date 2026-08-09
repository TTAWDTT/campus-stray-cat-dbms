import axios from 'axios';
import { useEffect, useMemo, useRef, useState } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { Button, Card, Icon, Input, Modal, Select, Table, Tag } from 'animal-island-ui';
import type { TableColumn } from 'animal-island-ui';
import { PageHeader } from '../../../shared/components/PageHeader';
import { useAuthStore } from '../../../stores/auth.store';
import { campusService } from '../services/campus.service';
import type { CampusArea, CatSighting, ServicePoint, SightingWritePayload } from '../types/campus';
import { catsService } from '../../../services/cats.service';
import type { CatSummary } from '../../../types/cats';

const areaTypeLabel: Record<string, string> = { CAMPUS: '校区', PUBLIC_AREA: '公共区域', ACTIVITY_AREA: '活动区域', GREENBELT: '绿地', GATE: '出入口' };
const pointLabel: Record<string, string> = { FEEDING: '投喂点', NEST: '猫窝', ACTIVITY: '活动点' };
const statusLabel: Record<string, string> = { ACTIVE: '正常', INACTIVE: '暂停维护', MAINTENANCE: '维护中' };
const tongjiCenter: L.LatLngExpression = [31.2848, 121.5064];
const knownMarkerPositions: Record<string, [number, number]> = {
  '东门测试区域': [31.2852, 121.5118],
  '图书馆北侧': [31.2866, 121.5054],
  '西北小树林': [31.2872, 121.5019],
  '食堂后门': [31.2842, 121.5057],
  '体育场看台': [31.2834, 121.5092],
  '教学楼东侧': [31.2854, 121.5094],
  '宿舍区南门': [31.2819, 121.5062],
  '实验楼草坪': [31.2847, 121.5028],
  '湖心亭': [31.2829, 121.5037],
  '创新中心广场': [31.2864, 121.5090],
};

const markerPosition = (area: CampusArea, index: number): [number, number] => {
  const name = area.areaName || '';
  if (knownMarkerPositions[name]) return knownMarkerPositions[name];
  return [31.282 + (index % 4) * 0.0015, 121.502 + (index % 3) * 0.0024];
};

const escapeHtml = (value: string) => value.replace(/[&<>'"]/g, (character) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[character] || character);

const errorMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data?.message === 'string') return data.message;
    if (error.response?.status === 401) return '请先登录后再记录目击。';
    if (error.response?.status === 403) return '当前账号没有修改目击记录的权限。';
  }
  return '校园位置服务暂时不可用，请稍后重试。';
};

const formatTime = (value?: string | null) => {
  if (!value) return '时间未知';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '时间未知' : date.toLocaleString('zh-CN', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' });
};

const initialForm: SightingWritePayload = { catID: '', areaID: '', sightingTime: '', remark: '' };
const SIGHTING_PAGE_SIZE = 5;

export function CampusPage() {
  const user = useAuthStore((state) => state.user);
  const canManage = ['ADMIN', 'VOLUNTEER'].includes((user?.roleName || '').toUpperCase());
  const [areas, setAreas] = useState<CampusArea[]>([]);
  const [points, setPoints] = useState<ServicePoint[]>([]);
  const [cats, setCats] = useState<CatSummary[]>([]);
  const [sightings, setSightings] = useState<CatSighting[]>([]);
  const [areaId, setAreaId] = useState('');
  const [catId, setCatId] = useState('');
  const [search, setSearch] = useState('');
  const [sightingPage, setSightingPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [modalClosing, setModalClosing] = useState(false);
  const [form, setForm] = useState<SightingWritePayload>(initialForm);
  const [editingSighting, setEditingSighting] = useState<CatSighting | null>(null);
  const timer = useRef<number | null>(null);
  const mapElement = useRef<HTMLDivElement>(null);
  const map = useRef<L.Map | null>(null);
  const markerLayer = useRef<L.LayerGroup | null>(null);

  const loadSightings = async () => {
    setLoading(true);
    setError('');
    try {
      setSightings(await campusService.sightings({ areaId, catId }));
    } catch (loadError) {
      setError(errorMessage(loadError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void Promise.all([campusService.areas().then(setAreas), campusService.servicePoints().then(setPoints), catsService.list().then(setCats)])
      .catch((loadError) => setError(errorMessage(loadError)));
  }, []);

  useEffect(() => { void loadSightings(); }, [areaId, catId]);
  useEffect(() => () => { if (timer.current !== null) window.clearTimeout(timer.current); }, []);

  useEffect(() => {
    if (!mapElement.current || map.current) return;
    map.current = L.map(mapElement.current, { zoomControl: true, attributionControl: true, minZoom: 16, maxZoom: 19 }).setView(tongjiCenter, 16);
    const tileSources = [
      { url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', attribution: '&copy; OpenStreetMap contributors' },
      { url: 'https://{s}.tile.openstreetmap.de/{z}/{x}/{y}.png', attribution: '&copy; OpenStreetMap contributors' },
      { url: 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', attribution: '&copy; OpenStreetMap contributors &copy; CARTO' },
    ];
    let sourceIndex = 0;
    let tileErrors = 0;
    mapElement.current.classList.add('campus-map-loading');
    const attachTiles = () => {
      if (!map.current || !mapElement.current) return;
      const source = tileSources[sourceIndex];
      const layer = L.tileLayer(source.url, { maxZoom: 19, attribution: source.attribution }).addTo(map.current);
      layer.on('tileload', () => {
        tileErrors = 0;
        mapElement.current?.classList.remove('campus-map-loading', 'campus-map-offline');
      });
      layer.on('tileerror', () => {
        tileErrors += 1;
        if (tileErrors < 2 || sourceIndex >= tileSources.length - 1 || !map.current) return;
        map.current.removeLayer(layer);
        sourceIndex += 1;
        tileErrors = 0;
        attachTiles();
      });
      if (sourceIndex >= tileSources.length - 1) {
        window.setTimeout(() => {
          if (tileErrors > 0) mapElement.current?.classList.add('campus-map-offline');
        }, 2200);
      }
    };
    attachTiles();
    return () => {
      map.current?.remove();
      map.current = null;
      markerLayer.current = null;
    };
  }, []);

  const resolvedSightings = useMemo(() => sightings.map((item) => ({
    ...item,
    catName: item.catName || cats.find((cat) => cat.catID === item.catID)?.catName || item.catID,
    areaName: item.areaName || areas.find((area) => area.areaID === item.areaID)?.areaName || item.areaID,
  })), [sightings, cats, areas]);

  const filteredSightings = useMemo(() => {
    const keyword = search.trim().toLowerCase();
    if (!keyword) return resolvedSightings;
    return resolvedSightings.filter((item) => [item.catName, item.areaName, item.remark].some((value) => value?.toLowerCase().includes(keyword)));
  }, [resolvedSightings, search]);

  const sightingPageCount = Math.max(1, Math.ceil(filteredSightings.length / SIGHTING_PAGE_SIZE));
  const pageSightings = filteredSightings.slice((sightingPage - 1) * SIGHTING_PAGE_SIZE, sightingPage * SIGHTING_PAGE_SIZE);
  const hasFilters = Boolean(search.trim() || catId || areaId);

  useEffect(() => {
    setSightingPage(1);
  }, [search, areaId, catId]);

  useEffect(() => {
    if (sightingPage > sightingPageCount) setSightingPage(sightingPageCount);
  }, [sightingPage, sightingPageCount]);

  const areaOptions = useMemo(() => [{ key: '', label: '全部活动区域' }, ...areas.map((item) => ({ key: item.areaID, label: item.areaName || item.areaID }))], [areas]);
  const catOptions = useMemo(() => [{ key: '', label: '全部猫咪' }, ...cats.map((item) => ({ key: item.catID, label: item.catName || item.catID }))], [cats]);
  const selectedArea = areas.find((item) => item.areaID === areaId);
  const selectedPoints = points.filter((point) => !areaId || point.areaID === areaId);
  const sightingRow = (row: Record<string, unknown>) => row as unknown as CatSighting;

  useEffect(() => {
    if (!map.current) return;
    markerLayer.current?.clearLayers();
    const layer = L.layerGroup().addTo(map.current);
    markerLayer.current = layer;
    areas.forEach((area, index) => {
      const name = area.areaName || area.areaID;
      const marker = L.marker(markerPosition(area, index), {
        icon: L.divIcon({ className: area.areaID === areaId ? 'campus-map-marker selected' : 'campus-map-marker', html: `<span>${index + 1}</span>`, iconSize: [28, 28], iconAnchor: [14, 14] }),
        title: name,
      });
      marker.bindTooltip(`<strong>${escapeHtml(name)}</strong><small>${escapeHtml(areaTypeLabel[area.areaType || ''] || '活动区域')}</small>`, { direction: 'top', offset: [0, -12], className: 'campus-map-tooltip' });
      marker.on('click', () => setAreaId(area.areaID));
      marker.addTo(layer);
    });
  }, [areas, areaId]);

  const openModal = () => { setEditingSighting(null); setForm({ ...initialForm, areaID: areaId }); setError(''); setModalClosing(false); setModalOpen(true); };
  const openEdit = (sighting: CatSighting) => {
    setEditingSighting(sighting);
    setForm({ catID: sighting.catID || '', areaID: sighting.areaID || '', sightingTime: sighting.sightingTime || '', photoUrl: sighting.photoUrl || '', remark: sighting.remark || '' });
    setError('');
    setModalClosing(false);
    setModalOpen(true);
  };
  const closeModal = (force = false) => {
    if (saving && !force) return;
    if (!modalOpen || modalClosing) return;
    setModalClosing(true);
    timer.current = window.setTimeout(() => { setModalOpen(false); setModalClosing(false); timer.current = null; }, 220);
  };
  const updateForm = (key: keyof SightingWritePayload, value: string) => setForm((current) => ({ ...current, [key]: value }));

  const submit = async () => {
    if (!form.catID || !form.areaID) { setError('请选择猫咪和目击区域。'); return; }
    setSaving(true);
    setError('');
    try {
      const payload = { ...form, sightingTime: form.sightingTime || undefined, remark: form.remark?.trim() || undefined };
      if (editingSighting) {
        await campusService.updateSighting(editingSighting.sightingID, payload);
        setNotice('目击记录已更新。');
      } else {
        await campusService.createSighting(payload);
        setNotice('目击记录已加入校园轨迹。');
      }
      closeModal(true);
      await loadSightings();
    } catch (saveError) {
      setError(errorMessage(saveError));
    } finally {
      setSaving(false);
    }
  };

  const removeSighting = async (sighting: CatSighting) => {
    if (!window.confirm(`确定删除${sighting.catName || '这条'}目击记录吗？`)) return;
    setError('');
    try {
      await campusService.deleteSighting(sighting.sightingID);
      setNotice('目击记录已删除。');
      await loadSightings();
    } catch (deleteError) {
      setError(errorMessage(deleteError));
    }
  };

  const columns: TableColumn[] = [
    { title: '猫咪', width: 150, render: (_value, row) => <strong className="campus-sighting-cat">{sightingRow(row).catName || '未命名猫咪'}</strong> },
    { title: '活动区域', width: 150, render: (_value, row) => { const item = sightingRow(row); const areaName = item.areaName || item.areaID || '未知区域'; return <span className="campus-sighting-area" title={areaName}>{areaName}</span>; } },
    { title: '目击时间', width: 150, render: (_value, row) => formatTime(sightingRow(row).sightingTime) },
    { title: '备注', render: (_value, row) => { const remark = sightingRow(row).remark || '—'; return <span className="campus-sighting-remark" title={remark}>{remark}</span>; } },
    { title: '位置', width: 125, render: (_value, row) => { const item = sightingRow(row); return item.latitude != null && item.longitude != null ? `${item.latitude.toFixed(4)}, ${item.longitude.toFixed(4)}` : '未记录'; } },
    ...(canManage ? [{ title: '操作', width: 116, render: (_value: unknown, row: Record<string, unknown>) => { const item = sightingRow(row); return <div className="cat-table-actions"><Button type="text" size="small" onClick={() => openEdit(item)}>编辑</Button><Button type="text" size="small" onClick={() => void removeSighting(item)}>删除</Button></div>; } }] : []),
  ];

  return (
    <section className="feature-page campus-page">
      <PageHeader kicker="CAMPUS TRACK" title="校园地图与目击" icon="icon-map" actions={<Button type="primary" icon={<Icon name="icon-camera" size={16} />} onClick={openModal}>记录目击</Button>} />
      {error && <div className="cats-alert" role="alert"><Icon name="icon-camera" size={17} /><span>{error}</span><Button type="text" size="small" onClick={() => setError('')}>知道了</Button></div>}
      {notice && <div className="campus-notice" role="status" aria-live="polite"><Icon name="icon-miles" size={15} /><span>{notice}</span><Button type="text" size="small" onClick={() => setNotice('')}>知道了</Button></div>}
      <div className="campus-overview-grid">
        <Card className="campus-map-card"><div className="campus-card-heading"><div><h2>{selectedArea?.areaName || '校园区域地图'}</h2></div><Tag color="app-teal" variant="soft">{areas.length} 个标点</Tag></div><div ref={mapElement} className="campus-map" aria-label="同济大学四平路校区地图" /><div className="campus-map-caption"><span><i className="campus-map-legend-dot" />点击标点筛选该区域目击记录</span><span>地图数据 © OpenStreetMap</span></div></Card>
        <Card className="campus-point-card"><div className="campus-card-heading"><div><h2>服务点位</h2></div><Tag color="app-yellow" variant="soft">{selectedPoints.length} 个点位</Tag></div><div className="campus-point-list">{selectedPoints.length ? selectedPoints.map((point) => <div className="campus-point-item" key={point.pointID}><span className="campus-point-icon"><Icon name={point.pointType === 'NEST' ? 'icon-critterpedia' : 'icon-shopping'} size={16} /></span><span><strong>{point.pointName || '未命名点位'}</strong><small>{pointLabel[point.pointType || ''] || point.pointType || '服务点'} · {statusLabel[point.facilityStatus || ''] || point.facilityStatus || '状态未知'}</small></span></div>) : <div className="campus-no-points"><Icon name="icon-map" size={25} /><p>该区域暂时没有服务点或猫窝。</p></div>}</div></Card>
      </div>
      <Card className="campus-sightings-card"><div className="campus-sightings-heading"><div><h2>目击轨迹</h2><p>每页展示 5 条，点击地图标点可快速筛选。</p></div><Tag color="app-green" variant="soft">{filteredSightings.length} 条记录</Tag></div><div className="campus-filter-grid"><Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="搜索猫咪、区域或备注" allowClear prefix={<Icon name="icon-critterpedia" size={15} />} /><Select options={catOptions} value={catId} onChange={setCatId} aria-label="按猫咪筛选" /><Select options={areaOptions} value={areaId} onChange={setAreaId} aria-label="按区域筛选" /><Button type="default" disabled={!hasFilters} onClick={() => { setSearch(''); setCatId(''); setAreaId(''); }}>重置</Button></div><div className="campus-table-wrap"><div key={sightingPage} className="cats-table-page"><Table columns={columns} dataSource={pageSightings as unknown as Record<string, unknown>[]} rowKey="sightingID" loading={loading} emptyText="还没有符合条件的目击记录" scroll={{ x: 760 }} /></div><div className="cats-pagination"><span>第 {sightingPage} / {sightingPageCount} 页 · 共 {filteredSightings.length} 条</span><div><Button className="cats-page-button" type="default" size="small" disabled={sightingPage <= 1} onClick={() => setSightingPage((current) => Math.max(1, current - 1))}>上一页</Button><Button className="cats-page-button" type="default" size="small" disabled={sightingPage >= sightingPageCount} onClick={() => setSightingPage((current) => Math.min(sightingPageCount, current + 1))}>下一页</Button></div></div></div></Card>
      <Modal open={modalOpen} className={modalClosing ? 'campus-modal-closing' : 'campus-modal-opening'} title={editingSighting ? '编辑目击记录' : '记录一次目击'} width={560} typewriter={false} onClose={closeModal} footer={<div className="cat-modal-footer"><Button type="default" onClick={() => closeModal()} disabled={saving}>取消</Button><Button type="primary" onClick={() => void submit()} loading={saving}>{editingSighting ? '保存修改' : '保存记录'}</Button></div>}>
        <div className="campus-form"><div className="cat-form-intro"><Icon name="icon-map" size={22} /><span><strong>把相遇留在校园轨迹里</strong><small>记录的信息会用于猫咪活动追踪和后续救助。</small></span></div><div className="cat-form-grid"><label><span>猫咪 *</span><Select options={catOptions.slice(1)} value={form.catID} onChange={(value) => updateForm('catID', value)} /></label><label><span>目击区域 *</span><Select options={areaOptions.slice(1)} value={form.areaID} onChange={(value) => updateForm('areaID', value)} /></label><label><span>目击时间</span><Input className="campus-datetime-input" type="datetime-local" prefix={<Icon name="icon-miles" size={15} />} value={form.sightingTime || ''} onChange={(event) => updateForm('sightingTime', event.target.value)} /></label><label><span>备注</span><Input value={form.remark || ''} onChange={(event) => updateForm('remark', event.target.value)} placeholder="例如：在树下晒太阳" /></label><label className="cat-form-wide"><span>现场照片 URL（可选）</span><Input value={form.photoUrl || ''} onChange={(event) => updateForm('photoUrl', event.target.value)} placeholder="暂时可粘贴一张公开图片地址" /></label></div></div>
      </Modal>
      {!canManage && <p className="campus-permission-note"><Icon name="icon-chat" size={14} />普通用户可以记录目击；编辑和删除功能将由管理员或志愿者维护。</p>}
    </section>
  );
}
