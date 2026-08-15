import axios from 'axios';
import { useEffect, useMemo, useState } from 'react';
import { Button, Card, Icon, Input, Modal, Select, Table, Tag } from 'animal-island-ui';
import type { TableColumn } from 'animal-island-ui';
import { PageHeader } from '../../../shared/components/PageHeader';
import { EmptyState } from '../../../shared/components/EmptyState';
import { StatusTag } from '../../../shared/components/StatusTag';
import { useAuthStore } from '../../../stores/auth.store';
import { catsService } from '../../../services/cats.service';
import { campusService } from '../../campus/services/campus.service';
import { rescueService } from '../../../services/rescue.service';
import type { CampusArea } from '../../campus/types/campus';
import type { CatSummary } from '../../../types/cats';
import type { EmergencyReport, MedHealthRecord, MedReminder, MissingAlert, TnrCase, TnrStatusLog } from '../rescue.types';

type RescueTab = 'care' | 'emergency' | 'missing';
type RescueModal = 'tnr' | 'medical' | 'emergency' | 'missing' | 'status' | 'logs' | null;
type StatusTarget = { type: 'tnr'; item: TnrCase } | { type: 'emergency'; item: EmergencyReport } | { type: 'missing'; item: MissingAlert };

const tnrLabels: Record<string, string> = { DISCOVERED: '已发现', CAPTURED: '已捕捉', SURGERY: '手术中', RECOVERING: '恢复中', RELEASED: '已放归', CANCELLED: '已取消' };
const medicalLabels: Record<string, string> = { VACCINATION: '疫苗', CHECKUP: '体检', TREATMENT: '治疗', SURGERY: '手术', DEWORMING: '驱虫', EMERGENCY: '急救', OTHER: '其他' };
const reminderLabels: Record<string, string> = { VACCINATION: '疫苗提醒', CHECKUP: '体检提醒', TREATMENT: '治疗提醒', SURGERY: '手术护理', DEWORMING: '驱虫提醒', EMERGENCY: '急救跟进', OTHER: '护理提醒' };
const emergencyLabels: Record<string, string> = { SUBMITTED: '待受理', ASSIGNED: '已分配', PROCESSING: '处理中', RESOLVED: '已解决', CLOSED: '已关闭' };
const urgencyLabels: Record<string, string> = { LOW: '低', MEDIUM: '中', HIGH: '高', CRITICAL: '紧急' };
const missingLabels: Record<string, string> = { PROCESSING: '寻找中', FOUND: '已寻回', CLOSED: '已关闭' };
const tnrOptions = Object.entries(tnrLabels).map(([key, label]) => ({ key, label }));
const medicalOptions = Object.entries(medicalLabels).map(([key, label]) => ({ key, label }));
const emergencyOptions = Object.entries(emergencyLabels).map(([key, label]) => ({ key, label }));
const urgencyOptions = Object.entries(urgencyLabels).map(([key, label]) => ({ key, label: `${label}级` }));
const missingOptions = Object.entries(missingLabels).map(([key, label]) => ({ key, label }));

const messageOf = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data?.message === 'string') return data.message;
    if (typeof data === 'string') return data;
    if (error.response?.status === 401) return '登录状态已失效，请重新登录。';
    if (error.response?.status === 403) return '当前账号没有执行这项救助操作的权限。';
  }
  return '救助中心暂时无法连接，请稍后重试。';
};

const formatTime = (value?: string | null, fallback = '—') => {
  if (!value) return fallback;
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? fallback : date.toLocaleString('zh-CN', { month: 'numeric', day: 'numeric', hour: '2-digit', minute: '2-digit' });
};

const localDate = () => new Date().toISOString().slice(0, 16);

export function RescuePage() {
  const user = useAuthStore((state) => state.user);
  const role = user?.roleName?.trim().toUpperCase() || 'USER';
  const canCare = ['ADMIN', 'VOLUNTEER', 'VET'].includes(role);
  const canCoordinate = ['ADMIN', 'VOLUNTEER'].includes(role);
  const [tab, setTab] = useState<RescueTab>(canCare ? 'care' : 'emergency');
  const [cats, setCats] = useState<CatSummary[]>([]);
  const [areas, setAreas] = useState<CampusArea[]>([]);
  const [tnrCases, setTnrCases] = useState<TnrCase[]>([]);
  const [medicalRecords, setMedicalRecords] = useState<MedHealthRecord[]>([]);
  const [reminders, setReminders] = useState<MedReminder[]>([]);
  const [emergencies, setEmergencies] = useState<EmergencyReport[]>([]);
  const [alerts, setAlerts] = useState<MissingAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [modal, setModal] = useState<RescueModal>(null);
  const [statusTarget, setStatusTarget] = useState<StatusTarget | null>(null);
  const [logs, setLogs] = useState<TnrStatusLog[]>([]);
  const [logCase, setLogCase] = useState<TnrCase | null>(null);
  const [tnrForm, setTnrForm] = useState({ catID: '', hospitalName: '', captureTime: localDate(), surgeryTime: '', releaseTime: '', totalCost: '' });
  const [medicalForm, setMedicalForm] = useState({ catID: '', recordType: 'CHECKUP', hospitalName: '', diagnosis: '', recordDate: localDate(), nextDueDate: '' });
  const [emergencyForm, setEmergencyForm] = useState({ areaID: '', animalType: 'CAT', urgencyLevel: 'HIGH', photoURL: '', processResult: '' });
  const [missingForm, setMissingForm] = useState({ catID: '', areaID: '', sightingTime: localDate(), thresholdDays: '14', remark: '' });
  const [statusValue, setStatusValue] = useState('');
  const [statusRemark, setStatusRemark] = useState('');

  const catName = (catID?: string | null) => cats.find((cat) => cat.catID === catID)?.catName || catID || '未命名猫咪';
  const areaName = (areaID?: string | null) => areas.find((area) => area.areaID === areaID)?.areaName || areaID || '未知区域';
  const catOptions = useMemo(() => cats.map((cat) => ({ key: cat.catID, label: cat.catName || cat.catID })), [cats]);
  const areaOptions = useMemo(() => areas.map((area) => ({ key: area.areaID, label: area.areaName || area.areaID })), [areas]);

  const reload = async () => {
    setLoading(true);
    setError('');
    try {
      const base = await Promise.all([catsService.list(), campusService.areas(), rescueService.emergencies(), rescueService.alerts()]);
      setCats(base[0]); setAreas(base[1]); setEmergencies(base[2]); setAlerts(base[3]);
      if (canCare) {
        const care = await Promise.all([rescueService.tnrCases(), rescueService.medicalRecords(), rescueService.reminders()]);
        setTnrCases(care[0]); setMedicalRecords(care[1]); setReminders(care[2]);
      } else {
        setTnrCases([]); setMedicalRecords([]); setReminders([]);
      }
    } catch (loadError) {
      setError(messageOf(loadError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void reload(); }, [canCare]);

  const openModal = (kind: RescueModal) => { setError(''); setModal(kind); };
  const closeModal = () => { if (!saving) setModal(null); };
  const resetTnr = () => { setTnrForm({ catID: '', hospitalName: '', captureTime: localDate(), surgeryTime: '', releaseTime: '', totalCost: '' }); openModal('tnr'); };
  const resetMedical = () => { setMedicalForm({ catID: '', recordType: 'CHECKUP', hospitalName: '', diagnosis: '', recordDate: localDate(), nextDueDate: '' }); openModal('medical'); };
  const resetEmergency = () => { setEmergencyForm({ areaID: '', animalType: 'CAT', urgencyLevel: 'HIGH', photoURL: '', processResult: '' }); openModal('emergency'); };
  const resetMissing = () => { setMissingForm({ catID: '', areaID: '', sightingTime: localDate(), thresholdDays: '14', remark: '' }); openModal('missing'); };

  const saveTnr = async () => {
    if (!tnrForm.catID) { setError('请选择需要救助的猫咪。'); return; }
    setSaving(true); setError('');
    try {
      await rescueService.createTnr({ catID: tnrForm.catID, responsibleUserID: user?.userId, currentStatus: 'DISCOVERED', hospitalName: tnrForm.hospitalName.trim() || undefined, captureTime: tnrForm.captureTime || undefined, surgeryTime: tnrForm.surgeryTime || undefined, releaseTime: tnrForm.releaseTime || undefined, totalCost: tnrForm.totalCost ? Number(tnrForm.totalCost) : undefined });
      setNotice('新的 TNR 案例已加入救助流程。'); setModal(null); await reload();
    } catch (saveError) { setError(messageOf(saveError)); } finally { setSaving(false); }
  };

  const saveMedical = async () => {
    if (!medicalForm.catID) { setError('请选择关联猫咪。'); return; }
    setSaving(true); setError('');
    try {
      await rescueService.createMedical({ catID: medicalForm.catID, recordType: medicalForm.recordType as MedHealthRecord['recordType'], hospitalName: medicalForm.hospitalName.trim() || undefined, diagnosis: medicalForm.diagnosis.trim() || undefined, recordDate: medicalForm.recordDate || undefined, nextDueDate: medicalForm.nextDueDate || undefined });
      setNotice('医疗记录已写入猫咪档案。'); setModal(null); await reload();
    } catch (saveError) { setError(messageOf(saveError)); } finally { setSaving(false); }
  };

  const saveEmergency = async () => {
    if (!emergencyForm.areaID) { setError('请选择发现地点。'); return; }
    setSaving(true); setError('');
    try {
      await rescueService.createEmergency({ areaID: emergencyForm.areaID, animalType: emergencyForm.animalType as EmergencyReport['animalType'], urgencyLevel: emergencyForm.urgencyLevel as EmergencyReport['urgencyLevel'], photoURL: emergencyForm.photoURL.trim() || undefined });
      setNotice('紧急救助已提交，救助伙伴会尽快处理。'); setModal(null); await reload();
    } catch (saveError) { setError(messageOf(saveError)); } finally { setSaving(false); }
  };

  const saveMissing = async () => {
    if (!missingForm.catID || !missingForm.areaID) { setError('请选择猫咪和最后目击区域。'); return; }
    setSaving(true); setError('');
    try {
      const sighting = await rescueService.createMissingSighting({ catID: missingForm.catID, areaID: missingForm.areaID, sightingTime: missingForm.sightingTime || undefined, remark: missingForm.remark.trim() || undefined });
      await rescueService.createAlert({ catID: missingForm.catID, lastSightingID: sighting.sightingID, lastSightingTime: missingForm.sightingTime || undefined, thresholdDays: Number(missingForm.thresholdDays) || 14, alertStatus: 'PROCESSING', remark: missingForm.remark.trim() || undefined });
      setNotice('最后目击已记录，失踪预警已发布。'); setModal(null); await reload();
    } catch (saveError) { setError(messageOf(saveError)); } finally { setSaving(false); }
  };

  const openStatus = (target: StatusTarget) => {
    setStatusTarget(target);
    setStatusRemark(target.type === 'emergency' ? target.item.processResult || '' : target.type === 'missing' ? target.item.remark || '' : '');
    setStatusValue(target.type === 'tnr' ? target.item.currentStatus || 'DISCOVERED' : target.type === 'emergency' ? target.item.processStatus || 'SUBMITTED' : target.item.alertStatus || 'PROCESSING');
    openModal('status');
  };

  const saveStatus = async () => {
    if (!statusTarget) return;
    setSaving(true); setError('');
    try {
      if (statusTarget.type === 'tnr') await rescueService.updateTnrStatus(statusTarget.item.caseID, statusValue, statusRemark.trim() || undefined);
      if (statusTarget.type === 'emergency') await rescueService.updateEmergency(statusTarget.item.reportID, statusValue, statusRemark.trim() || undefined);
      if (statusTarget.type === 'missing') await rescueService.updateAlert(statusTarget.item.alertID, statusValue, statusRemark.trim() || undefined);
      setNotice('处理状态已更新。'); setModal(null); await reload();
    } catch (saveError) { setError(messageOf(saveError)); } finally { setSaving(false); }
  };

  const takeEmergency = async (report: EmergencyReport) => {
    if (!user?.userId) return;
    try {
      await rescueService.assignEmergency(report.reportID, user.userId);
      setNotice('你已接手这条紧急救助。'); await reload();
    } catch (assignError) { setError(messageOf(assignError)); }
  };

  const showLogs = async (tnr: TnrCase) => {
    setSaving(true); setError('');
    try { setLogs(await rescueService.tnrLogs(tnr.caseID)); setLogCase(tnr); setModal('logs'); }
    catch (logError) { setError(messageOf(logError)); }
    finally { setSaving(false); }
  };

  const completeReminder = async (reminder: MedReminder, action: 'sent' | 'complete') => {
    try {
      if (action === 'sent') await rescueService.markReminderSent(reminder.reminderID);
      else await rescueService.completeReminder(reminder.reminderID);
      setNotice(action === 'sent' ? '提醒已标记为发送。' : '提醒已标记为完成。'); await reload();
    } catch (reminderError) { setError(messageOf(reminderError)); }
  };

  const tnrRow = (row: Record<string, unknown>) => row as unknown as TnrCase;
  const medicalRow = (row: Record<string, unknown>) => row as unknown as MedHealthRecord;
  const reminderRow = (row: Record<string, unknown>) => row as unknown as MedReminder;
  const emergencyRow = (row: Record<string, unknown>) => row as unknown as EmergencyReport;
  const alertRow = (row: Record<string, unknown>) => row as unknown as MissingAlert;
  const tnrColumns: TableColumn[] = [
    { title: '猫咪', width: 132, render: (_value, row) => <strong>{catName(tnrRow(row).catID)}</strong> },
    { title: '当前节点', width: 116, render: (_value, row) => { const value = tnrRow(row).currentStatus || 'DISCOVERED'; return <StatusTag value={value} label={tnrLabels[value] || value} />; } },
    { title: '医院', width: 150, render: (_value, row) => tnrRow(row).hospitalName || '待安排' },
    { title: '捕捉时间', width: 145, render: (_value, row) => formatTime(tnrRow(row).captureTime) },
    { title: '费用', width: 90, render: (_value, row) => tnrRow(row).totalCost == null ? '—' : `¥${tnrRow(row).totalCost}` },
    { title: '操作', width: 142, render: (_value, row) => <div className="rescue-table-actions"><Button type="text" size="small" onClick={() => void showLogs(tnrRow(row))}>轨迹</Button><Button type="text" size="small" onClick={() => openStatus({ type: 'tnr', item: tnrRow(row) })}>推进</Button></div> },
  ];
  const medicalColumns: TableColumn[] = [
    { title: '猫咪', width: 125, render: (_value, row) => <strong>{catName(medicalRow(row).catID)}</strong> },
    { title: '类型', width: 96, render: (_value, row) => medicalLabels[medicalRow(row).recordType || ''] || medicalRow(row).recordType || '未分类' },
    { title: '诊断与处理', render: (_value, row) => medicalRow(row).diagnosis || '暂未记录' },
    { title: '就诊时间', width: 145, render: (_value, row) => formatTime(medicalRow(row).recordDate) },
    { title: '下次护理', width: 135, render: (_value, row) => formatTime(medicalRow(row).nextDueDate) },
  ];
  const emergencyColumns: TableColumn[] = [
    { title: '紧急等级', width: 104, render: (_value, row) => { const item = emergencyRow(row); return <Tag color={item.urgencyLevel === 'CRITICAL' ? 'app-red' : item.urgencyLevel === 'HIGH' ? 'app-yellow' : 'app-teal'} variant="soft">{urgencyLabels[item.urgencyLevel] || item.urgencyLevel}级</Tag>; } },
    { title: '发现地点', width: 142, render: (_value, row) => <strong>{areaName(emergencyRow(row).areaID)}</strong> },
    { title: '上报时间', width: 145, render: (_value, row) => formatTime(emergencyRow(row).reportTime) },
    { title: '处理进度', width: 110, render: (_value, row) => { const value = emergencyRow(row).processStatus; return <StatusTag value={value} label={emergencyLabels[value] || value} />; } },
    { title: '处理结果', render: (_value, row) => emergencyRow(row).processResult || '等待救助伙伴响应' },
    ...(canCoordinate ? [{ title: '操作', width: 150, render: (_value: unknown, row: Record<string, unknown>) => { const item = emergencyRow(row); return <div className="rescue-table-actions">{!item.handlerUserID && <Button type="text" size="small" onClick={() => void takeEmergency(item)}>接手</Button>}<Button type="text" size="small" onClick={() => openStatus({ type: 'emergency', item })}>更新</Button></div>; } }] : []),
  ];
  const missingColumns: TableColumn[] = [
    { title: '猫咪', width: 136, render: (_value, row) => <strong>{catName(alertRow(row).catID)}</strong> },
    { title: '最后目击', width: 145, render: (_value, row) => formatTime(alertRow(row).lastSightingTime) },
    { title: '预警阈值', width: 100, render: (_value, row) => `${alertRow(row).thresholdDays || '—'} 天` },
    { title: '状态', width: 105, render: (_value, row) => { const value = alertRow(row).alertStatus; return <StatusTag value={value} label={missingLabels[value] || value} />; } },
    { title: '说明', render: (_value, row) => alertRow(row).remark || '等待新的目击信息' },
    ...(canCoordinate ? [{ title: '操作', width: 100, render: (_value: unknown, row: Record<string, unknown>) => <Button type="text" size="small" onClick={() => openStatus({ type: 'missing', item: alertRow(row) })}>更新</Button> }] : []),
  ];

  const careTabs = canCare ? [{ key: 'care', label: 'TNR 与医疗', note: '救助流程与护理提醒', icon: 'icon-camera' as const }, { key: 'emergency', label: '紧急救助', note: '发现即上报、及时接手', icon: 'icon-miles' as const }, { key: 'missing', label: '失踪预警', note: '记录最后目击与寻回', icon: 'icon-map' as const }] : [{ key: 'emergency', label: '紧急救助', note: '发现即上报、及时接手', icon: 'icon-miles' as const }, { key: 'missing', label: '失踪预警', note: '记录最后目击与寻回', icon: 'icon-map' as const }];
  const pendingReminders = reminders.filter((item) => item.sendStatus !== 'COMPLETED');
  const urgentReports = emergencies.filter((item) => ['HIGH', 'CRITICAL'].includes(item.urgencyLevel) && !['RESOLVED', 'CLOSED'].includes(item.processStatus));
  const activeAlerts = alerts.filter((item) => item.alertStatus === 'PROCESSING');

  return (
    <section className="feature-page rescue-page">
      <PageHeader kicker="RESCUE DESK" title="救助中心" icon="icon-camera" actions={<div className="rescue-header-actions">{tab === 'emergency' && <Button type="primary" icon={<Icon name="icon-miles" size={16} />} onClick={resetEmergency}>紧急上报</Button>}{tab === 'missing' && <Button type="primary" icon={<Icon name="icon-map" size={16} />} onClick={resetMissing}>发布预警</Button>}{tab === 'care' && <Button type="primary" icon={<Icon name="icon-camera" size={16} />} onClick={resetTnr}>新建 TNR</Button>}</div>} />
      {error && <div className="cats-alert rescue-alert" role="alert"><Icon name="icon-camera" size={17} /><span>{error}</span><Button type="text" size="small" onClick={() => setError('')}>知道了</Button></div>}
      {notice && <div className="rescue-notice" role="status"><Icon name="icon-miles" size={16} /><span>{notice}</span><Button type="text" size="small" onClick={() => setNotice('')}>知道了</Button></div>}
      <div className="rescue-summary-row"><Card className="rescue-summary-card urgent"><span><Icon name="icon-miles" size={21} /></span><div><small>高优先级救助</small><strong>{urgentReports.length}</strong><em>条等待处理</em></div></Card><Card className="rescue-summary-card"><span><Icon name="icon-camera" size={21} /></span><div><small>待完成护理</small><strong>{pendingReminders.length}</strong><em>项提醒</em></div></Card><Card className="rescue-summary-card"><span><Icon name="icon-map" size={21} /></span><div><small>寻找中的猫咪</small><strong>{activeAlerts.length}</strong><em>条预警</em></div></Card></div>
      <Card className="rescue-workspace-card"><div className="rescue-tabs">{careTabs.map((item) => <button key={item.key} className={tab === item.key ? 'active' : ''} onClick={() => setTab(item.key as RescueTab)}><Icon name={item.icon} size={18} /><span><strong>{item.label}</strong><small>{item.note}</small></span></button>)}</div>
        {tab === 'care' && <div className="rescue-panel"><div className="rescue-panel-heading"><div><h2>TNR 救助流程</h2></div><Button type="default" icon={<Icon name="icon-camera" size={15} />} onClick={resetTnr}>新建案例</Button></div><Table className="rescue-table-wide" columns={tnrColumns} dataSource={tnrCases as unknown as Record<string, unknown>[]} rowKey="caseID" loading={loading} emptyText="还没有 TNR 救助案例" scroll={{ x: 760 }} /><div className="rescue-care-lower"><div><div className="rescue-panel-heading small"><div><h2>医疗历史</h2></div><Button type="default" icon={<Icon name="icon-camera" size={15} />} onClick={resetMedical}>新增记录</Button></div><Table columns={medicalColumns} dataSource={medicalRecords.slice(0, 5) as unknown as Record<string, unknown>[]} rowKey="recordID" loading={loading} emptyText="暂无医疗记录" scroll={{ x: 620 }} /></div><div className="rescue-reminder-card"><div className="rescue-panel-heading small"><div><h2>护理提醒</h2></div><Tag color="app-yellow" variant="soft">{pendingReminders.length} 项</Tag></div>{pendingReminders.length ? <div className="rescue-reminder-list">{pendingReminders.slice(0, 5).map((reminder) => <div key={reminder.reminderID}><span><strong>{catName(reminder.catID)} · {reminderLabels[reminder.reminderType] || reminder.reminderType}</strong><small>{formatTime(reminder.reminderTime, '暂未安排时间')}</small></span><div>{reminder.sendStatus === 'PENDING' && <Button type="text" size="small" onClick={() => void completeReminder(reminder, 'sent')}>已发送</Button>}<Button type="text" size="small" onClick={() => void completeReminder(reminder, 'complete')}>完成</Button></div></div>)}</div> : <EmptyState icon="icon-camera" title="没有待处理提醒" description="新的疫苗、驱虫或复诊提醒会出现在这里。" />}</div></div></div>}
        {tab === 'emergency' && <div className="rescue-panel"><div className="rescue-panel-heading"><div><h2>紧急救助上报</h2></div><Button type="primary" icon={<Icon name="icon-miles" size={15} />} onClick={resetEmergency}>紧急上报</Button></div><Table columns={emergencyColumns} dataSource={emergencies as unknown as Record<string, unknown>[]} rowKey="reportID" loading={loading} emptyText="暂时没有紧急救助上报" scroll={{ x: 760 }} /></div>}
        {tab === 'missing' && <div className="rescue-panel"><div className="rescue-panel-heading"><div><h2>猫咪失踪预警</h2></div><Button type="primary" icon={<Icon name="icon-map" size={15} />} onClick={resetMissing}>发布预警</Button></div><Table columns={missingColumns} dataSource={alerts as unknown as Record<string, unknown>[]} rowKey="alertID" loading={loading} emptyText="当前没有失踪预警" scroll={{ x: 720 }} /></div>}
      </Card>
      <Modal open={modal === 'tnr'} title="新建 TNR 救助案例" width={650} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" onClick={() => void saveTnr()} loading={saving}>加入救助流程</Button></div>}><div className="rescue-form"><div className="rescue-form-intro"><Icon name="icon-camera" size={22} /><span><strong>从发现开始记录这条救助线</strong><small>案例创建后可在表格中持续推进状态并查看完整日志。</small></span></div><div className="rescue-form-grid"><label className="rescue-cat-select"><span>猫咪 *</span><Select options={catOptions} value={tnrForm.catID} onChange={(value) => setTnrForm((form) => ({ ...form, catID: value }))} /></label><label><span>就诊医院</span><Input value={tnrForm.hospitalName} onChange={(event) => setTnrForm((form) => ({ ...form, hospitalName: event.target.value }))} placeholder="例如：校动物医院" /></label><label><span>计划捕捉时间</span><Input type="datetime-local" value={tnrForm.captureTime} onChange={(event) => setTnrForm((form) => ({ ...form, captureTime: event.target.value }))} /></label><label><span>计划手术时间</span><Input type="datetime-local" value={tnrForm.surgeryTime} onChange={(event) => setTnrForm((form) => ({ ...form, surgeryTime: event.target.value }))} /></label><label><span>计划放归时间</span><Input type="datetime-local" value={tnrForm.releaseTime} onChange={(event) => setTnrForm((form) => ({ ...form, releaseTime: event.target.value }))} /></label><label><span>预计费用</span><Input type="number" value={tnrForm.totalCost} onChange={(event) => setTnrForm((form) => ({ ...form, totalCost: event.target.value }))} placeholder="元" /></label></div></div></Modal>
      <Modal open={modal === 'medical'} title="新增医疗记录" width={620} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" onClick={() => void saveMedical()} loading={saving}>保存记录</Button></div>}><div className="rescue-form"><div className="rescue-form-intro"><Icon name="icon-camera" size={22} /><span><strong>把护理过程留在档案里</strong><small>后续疫苗、驱虫与复诊提醒都能以此为依据。</small></span></div><div className="rescue-form-grid"><label><span>猫咪 *</span><Select options={catOptions} value={medicalForm.catID} onChange={(value) => setMedicalForm((form) => ({ ...form, catID: value }))} /></label><label><span>医疗类型</span><Select options={medicalOptions} value={medicalForm.recordType} onChange={(value) => setMedicalForm((form) => ({ ...form, recordType: value }))} /></label><label><span>医院</span><Input value={medicalForm.hospitalName} onChange={(event) => setMedicalForm((form) => ({ ...form, hospitalName: event.target.value }))} placeholder="可选" /></label><label><span>记录时间</span><Input type="datetime-local" value={medicalForm.recordDate} onChange={(event) => setMedicalForm((form) => ({ ...form, recordDate: event.target.value }))} /></label><label><span>下次护理时间</span><Input type="datetime-local" value={medicalForm.nextDueDate} onChange={(event) => setMedicalForm((form) => ({ ...form, nextDueDate: event.target.value }))} /></label><label className="rescue-form-wide"><span>诊断与处理</span><textarea value={medicalForm.diagnosis} onChange={(event) => setMedicalForm((form) => ({ ...form, diagnosis: event.target.value }))} placeholder="例如：完成驱虫，建议两周后复查。" /></label></div></div></Modal>
      <Modal open={modal === 'emergency'} title="提交紧急救助" width={590} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" onClick={() => void saveEmergency()} loading={saving}>提交上报</Button></div>}><div className="rescue-form"><div className="rescue-form-intro emergency"><Icon name="icon-miles" size={22} /><span><strong>请优先记录地点与紧急程度</strong><small>如遇明显伤病、被困或交通风险，请选择“紧急”并尽快联系现场志愿者。</small></span></div><div className="rescue-form-grid"><label><span>发现地点 *</span><Select options={areaOptions} value={emergencyForm.areaID} onChange={(value) => setEmergencyForm((form) => ({ ...form, areaID: value }))} /></label><label><span>紧急等级 *</span><Select options={urgencyOptions} value={emergencyForm.urgencyLevel} onChange={(value) => setEmergencyForm((form) => ({ ...form, urgencyLevel: value }))} /></label><label className="rescue-form-wide"><span>现场照片 URL（可选）</span><Input value={emergencyForm.photoURL} onChange={(event) => setEmergencyForm((form) => ({ ...form, photoURL: event.target.value }))} placeholder="可粘贴公开图片地址" /></label></div></div></Modal>
      <Modal open={modal === 'missing'} title="记录最后目击并发布预警" width={630} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" onClick={() => void saveMissing()} loading={saving}>记录并发布</Button></div>}><div className="rescue-form"><div className="rescue-form-intro"><Icon name="icon-map" size={22} /><span><strong>先锁定最后一次相遇</strong><small>提交后会先生成目击记录，再自动关联到这条失踪预警。</small></span></div><div className="rescue-form-grid"><label><span>猫咪 *</span><Select options={catOptions} value={missingForm.catID} onChange={(value) => setMissingForm((form) => ({ ...form, catID: value }))} /></label><label><span>最后目击区域 *</span><Select options={areaOptions} value={missingForm.areaID} onChange={(value) => setMissingForm((form) => ({ ...form, areaID: value }))} /></label><label><span>最后目击时间</span><Input type="datetime-local" value={missingForm.sightingTime} onChange={(event) => setMissingForm((form) => ({ ...form, sightingTime: event.target.value }))} /></label><label><span>预警阈值（天）</span><Input type="number" min="1" value={missingForm.thresholdDays} onChange={(event) => setMissingForm((form) => ({ ...form, thresholdDays: event.target.value }))} /></label><label className="rescue-form-wide"><span>补充说明</span><textarea value={missingForm.remark} onChange={(event) => setMissingForm((form) => ({ ...form, remark: event.target.value }))} placeholder="例如：连续两周未在原活动区域出现。" /></label></div></div></Modal>
      <Modal open={modal === 'status'} title="更新救助状态" width={560} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="default" onClick={closeModal} disabled={saving}>取消</Button><Button type="primary" onClick={() => void saveStatus()} loading={saving}>更新状态</Button></div>}><div className="rescue-form"><div className="rescue-status-target"><Icon name={statusTarget?.type === 'missing' ? 'icon-map' : statusTarget?.type === 'emergency' ? 'icon-miles' : 'icon-camera'} size={21} /><span><strong>{statusTarget?.type === 'tnr' ? catName(statusTarget.item.catID) : statusTarget?.type === 'emergency' ? areaName(statusTarget.item.areaID) : statusTarget ? catName(statusTarget.item.catID) : ''}</strong><small>{statusTarget?.type === 'tnr' ? '推进 TNR 流程后会自动留下状态日志。' : '状态与处理说明会同步到救助工作台。'}</small></span></div><div className="rescue-form-grid"><label className="rescue-form-wide"><span>新的状态</span><Select options={statusTarget?.type === 'tnr' ? tnrOptions : statusTarget?.type === 'emergency' ? emergencyOptions : missingOptions} value={statusValue} onChange={setStatusValue} /></label><label className="rescue-form-wide"><span>{statusTarget?.type === 'emergency' ? '处理结果' : '处理备注'}</span><textarea value={statusRemark} onChange={(event) => setStatusRemark(event.target.value)} placeholder="补充这次处理的实际情况" /></label></div></div></Modal>
      <Modal open={modal === 'logs'} title={`${logCase ? catName(logCase.catID) : 'TNR'} · 状态轨迹`} width={590} typewriter={false} onClose={closeModal} footer={<div className="rescue-modal-footer"><Button type="primary" onClick={closeModal}>知道了</Button></div>}><div className="rescue-log-list">{logs.length ? logs.map((log) => <div key={log.logID}><span className="rescue-log-dot" /><div><strong>{tnrLabels[log.fromStatus || ''] || log.fromStatus || '初始'} <i>→</i> {tnrLabels[log.toStatus || ''] || log.toStatus || '未知'}</strong><small>{formatTime(log.opTime)}{log.remark ? ` · ${log.remark}` : ''}</small></div></div>) : <EmptyState icon="icon-camera" title="还没有状态流转记录" description="推进 TNR 案例后，状态轨迹会出现在这里。" />}</div></Modal>
    </section>
  );
}
