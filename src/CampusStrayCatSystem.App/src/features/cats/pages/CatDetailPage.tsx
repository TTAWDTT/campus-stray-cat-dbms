import axios from 'axios';
import { useEffect, useRef, useState } from 'react';
import type { ChangeEvent } from 'react';
import { Button, Card, Icon, Tag } from 'animal-island-ui';
import { useNavigate, useParams } from 'react-router-dom';
import { catsService } from '../../../services/cats.service';
import { useAuthStore } from '../../../stores/auth.store';
import type { CatPhoto, CatSummary } from '../../../types/cats';
import { EmptyState } from '../../../shared/components/EmptyState';
import { PageHeader } from '../../../shared/components/PageHeader';
import { StatusTag } from '../../../shared/components/StatusTag';

const genderLabel: Record<string, string> = { UNKNOWN: '未知', MALE: '公猫', FEMALE: '母猫' };
const lifeLabel: Record<string, string> = { ON_CAMPUS: '在校园', MISSING: '失踪', ADOPTED: '已领养', DECEASED: '已离世' };

const resolvePhotoUrl = (url: string) => {
  if (/^https?:\/\//i.test(url)) return url;
  const configuredBase = import.meta.env.VITE_API_BASE_URL;
  if (configuredBase && /^https?:\/\//i.test(configuredBase)) {
    return `${configuredBase.replace(/\/api\/?$/, '')}${url}`;
  }
  return url;
};

const readError = (error: unknown) => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    if (typeof data?.message === 'string') return data.message;
    if (typeof data === 'string') return data;
    if (error.response?.status === 403) return '当前账号没有维护照片的权限。';
    if (error.response?.status === 404) return '没有找到这只猫咪或它的照片。';
  }
  return '暂时无法加载猫咪详情，请稍后重试。';
};

const displayTime = (value?: string | null) => {
  if (!value) return '时间未知';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? '时间未知' : date.toLocaleDateString('zh-CN');
};

export function CatDetailPage() {
  const { catId } = useParams();
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);
  const canManage = ['ADMIN', 'VOLUNTEER'].includes((user?.roleName || '').toUpperCase());
  const fileInput = useRef<HTMLInputElement>(null);
  const [cat, setCat] = useState<CatSummary | null>(null);
  const [photos, setPhotos] = useState<CatPhoto[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');

  const loadDetail = async () => {
    if (!catId) return;
    setLoading(true);
    setError('');
    try {
      const [catResult, photoResult] = await Promise.all([catsService.get(catId), catsService.photos(catId)]);
      setCat(catResult);
      setPhotos(photoResult);
    } catch (loadError) {
      setError(readError(loadError));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { void loadDetail(); }, [catId]);

  const primaryPhoto = photos.find((photo) => photo.isPrimary === 1) || photos[0];

  const upload = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = '';
    if (!file || !catId) return;
    setUploading(true);
    setError('');
    try {
      await catsService.uploadPhoto(catId, file, photos.length === 0);
      setNotice('照片已加入档案。');
      await loadDetail();
    } catch (uploadError) {
      setError(readError(uploadError));
    } finally {
      setUploading(false);
    }
  };

  const setPrimary = async (photo: CatPhoto) => {
    if (!catId || photo.isPrimary === 1) return;
    try {
      await catsService.setPrimaryPhoto(catId, photo.photoID);
      setNotice('主图已更新。');
      await loadDetail();
    } catch (primaryError) {
      setError(readError(primaryError));
    }
  };

  const deletePhoto = async (photo: CatPhoto) => {
    if (!catId || !window.confirm('确定删除这张照片吗？删除后无法从档案中恢复。')) return;
    try {
      await catsService.deletePhoto(catId, photo.photoID);
      setNotice('照片已删除。');
      await loadDetail();
    } catch (deleteError) {
      setError(readError(deleteError));
    }
  };

  if (loading) {
    return <section className="feature-page cat-detail-page"><Card className="cat-detail-loading"><Icon name="icon-critterpedia" size={32} bounce /><p>正在打开猫咪档案…</p></Card></section>;
  }

  if (!cat) {
    return <section className="feature-page cat-detail-page"><EmptyState icon="icon-critterpedia" title="没有找到这只猫咪" description={error || '这份档案可能已经被移除，或链接已失效。'} action={<Button type="primary" onClick={() => navigate('/cats')}>返回猫咪档案</Button>} /></section>;
  }

  return (
    <section className="feature-page cat-detail-page">
      <PageHeader kicker="CAT PROFILE" title={cat.catName || '未命名猫咪'} icon="icon-critterpedia" actions={<Button type="default" onClick={() => navigate('/cats')}><Icon name="icon-map" size={15} />返回档案</Button>} />
      {error && <div className="cats-alert" role="alert"><Icon name="icon-camera" size={17} /><span>{error}</span><Button type="text" size="small" onClick={() => setError('')}>知道了</Button></div>}
      <div className="cat-detail-top"><Card className="cat-detail-cover">{primaryPhoto ? <img src={resolvePhotoUrl(primaryPhoto.photoUrl)} alt={`${cat.catName || '猫咪'}主图`} /> : <div className="cat-detail-no-cover"><Icon name="icon-critterpedia" size={42} /><span>暂时还没有照片</span></div>}{primaryPhoto && <Tag className="cat-detail-cover-tag" color="app-yellow" variant="soft">主图</Tag>}</Card><Card className="cat-detail-facts"><div className="cat-detail-facts-heading"><div><small>PROFILE NOTES</small><h2>{cat.catName || '未命名猫咪'}</h2></div><StatusTag value={cat.lifeStatus || 'ON_CAMPUS'} label={lifeLabel[cat.lifeStatus || 'ON_CAMPUS'] || cat.lifeStatus || '未知'} /></div><div className="cat-detail-fact-grid"><div><span>性别</span><strong>{genderLabel[cat.gender || 'UNKNOWN'] || '未知'}</strong></div><div><span>花色</span><strong>{cat.colorPattern || '未记录'}</strong></div><div><span>品种</span><strong>{cat.breed || '未记录'}</strong></div><div><span>活动区域</span><strong>{cat.mainAreaName || cat.mainAreaId || '暂未关联'}</strong></div><div><span>绝育</span><strong>{cat.sterilizedFlag === 1 ? '已绝育' : '未绝育'}</strong></div><div><span>剪耳</span><strong>{cat.earTipFlag === 1 ? '已剪耳' : '未剪耳'}</strong></div></div>{cat.personalityTags && <div className="cat-detail-personality"><span>性格标签</span><div>{cat.personalityTags.split(',').map((tag) => <Tag key={tag} color="app-teal" variant="soft" size="small">{tag.trim()}</Tag>)}</div></div>}</Card></div>
      <Card className="cat-photos-card"><div className="cat-photos-heading"><div><h2>猫咪照片</h2><p>{photos.length ? `共 ${photos.length} 张照片 · 最近上传于 ${displayTime(photos[photos.length - 1]?.uploadTime)}` : '照片会在这里形成这只猫咪的识别记录。'}</p></div>{canManage && <><input ref={fileInput} className="cat-photo-input" type="file" accept="image/jpeg,image/png" onChange={(event) => void upload(event)} /><Button type="primary" loading={uploading} onClick={() => fileInput.current?.click()}><Icon name="icon-camera" size={16} />上传照片</Button></>}</div>{notice && <div className="cat-detail-notice"><Tag color="app-green" variant="soft">{notice}</Tag></div>}{photos.length ? <div className="cat-photo-grid">{photos.map((photo) => <div className={photo.isPrimary === 1 ? 'cat-photo-item primary' : 'cat-photo-item'} key={photo.photoID}><img src={resolvePhotoUrl(photo.photoUrl)} alt={`${cat.catName || '猫咪'}照片`} /><div className="cat-photo-overlay"><span>{photo.isPrimary === 1 ? '主图' : displayTime(photo.uploadTime)}</span>{canManage && <div>{photo.isPrimary !== 1 && <Button type="default" size="small" onClick={() => void setPrimary(photo)}>设为主图</Button>}<Button type="default" size="small" onClick={() => void deletePhoto(photo)}>删除</Button></div>}</div></div>)}</div> : <div className="cat-photo-empty"><Icon name="icon-camera" size={30} /><strong>还没有照片</strong><p>{canManage ? '上传一张清晰的照片，帮助大家在校园里认出它。' : '管理员或志愿者上传照片后，这里会展示识别记录。'}</p></div>}</Card>
    </section>
  );
}
