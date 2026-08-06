import { http } from './http';
import type { CampusArea, CatFilters, CatPhoto, CatSummary, CatWritePayload, CatGender, CatLifeStatus, CatArchiveStatus } from '../types/cats';

type ApiRecord = Record<string, unknown>;

const value = <T>(data: ApiRecord, camel: string, pascal: string): T | undefined =>
  (data[camel] ?? data[pascal]) as T | undefined;

const toCat = (data: ApiRecord): CatSummary => ({
  catID: value<string>(data, 'catID', 'CatID') || '',
  catName: value<string | null>(data, 'catName', 'CatName'),
  gender: value<CatGender | null>(data, 'gender', 'Gender'),
  breed: value<string | null>(data, 'breed', 'Breed'),
  colorPattern: value<string | null>(data, 'colorPattern', 'ColorPattern'),
  sterilizedFlag: value<number | null>(data, 'sterilizedFlag', 'SterilizedFlag'),
  earTipFlag: value<number | null>(data, 'earTipFlag', 'EarTipFlag'),
  personalityTags: value<string | null>(data, 'personalityTags', 'PersonalityTags'),
  mainAreaId: value<string | null>(data, 'mainAreaId', 'MainAreaId'),
  mainAreaName: value<string | null>(data, 'mainAreaName', 'MainAreaName'),
  lifeStatus: value<CatLifeStatus | null>(data, 'lifeStatus', 'LifeStatus'),
  archiveStatus: value<CatArchiveStatus | null>(data, 'archiveStatus', 'ArchiveStatus'),
  primaryPhotoUrl: value<string | null>(data, 'primaryPhotoUrl', 'PrimaryPhotoUrl'),
});

const toArea = (data: ApiRecord): CampusArea => ({
  areaID: value<string>(data, 'areaID', 'AreaID') || value<string>(data, 'areaId', 'AreaId') || '',
  areaName: value<string | null>(data, 'areaName', 'AreaName'),
});

const toPhoto = (data: ApiRecord): CatPhoto => ({
  photoID: value<string>(data, 'photoID', 'PhotoID') || '',
  catID: value<string | null>(data, 'catID', 'CatID'),
  photoUrl: value<string>(data, 'photoUrl', 'PhotoUrl') || '',
  uploadUserID: value<string | null>(data, 'uploadUserID', 'UploadUserID'),
  uploadTime: value<string | null>(data, 'uploadTime', 'UploadTime'),
  isPrimary: value<number>(data, 'isPrimary', 'IsPrimary') || 0,
});

export const catsService = {
  async list(filters: CatFilters = {}) {
    const { data } = await http.get<ApiRecord[]>('/cats', { params: filters });
    return data.map(toCat);
  },

  async get(catID: string) {
    const { data } = await http.get<ApiRecord>(`/cats/${encodeURIComponent(catID)}`);
    return toCat(data);
  },

  async areas() {
    const { data } = await http.get<ApiRecord[]>('/areas');
    return data.map(toArea);
  },

  async create(payload: CatWritePayload) {
    const { data } = await http.post<ApiRecord>('/cats', payload);
    return toCat(data);
  },

  async update(catID: string, payload: CatWritePayload) {
    await http.put(`/cats/${encodeURIComponent(catID)}`, payload);
  },

  async archive(catID: string) {
    await http.delete(`/cats/${encodeURIComponent(catID)}`);
  },

  async photos(catID: string) {
    const { data } = await http.get<ApiRecord[]>(`/cats/${encodeURIComponent(catID)}/photos`);
    return data.map(toPhoto);
  },

  async uploadPhoto(catID: string, file: File, isPrimary = false) {
    const body = new FormData();
    body.append('File', file);
    body.append('IsPrimary', isPrimary ? '1' : '0');
    const { data } = await http.post<ApiRecord>(`/cats/${encodeURIComponent(catID)}/photos`, body, { headers: { 'Content-Type': 'multipart/form-data' } });
    return toPhoto(data);
  },

  async setPrimaryPhoto(catID: string, photoID: string) {
    await http.put(`/cats/${encodeURIComponent(catID)}/photos/${encodeURIComponent(photoID)}/primary`);
  },

  async deletePhoto(catID: string, photoID: string) {
    await http.delete(`/cats/${encodeURIComponent(catID)}/photos/${encodeURIComponent(photoID)}`);
  },
};
