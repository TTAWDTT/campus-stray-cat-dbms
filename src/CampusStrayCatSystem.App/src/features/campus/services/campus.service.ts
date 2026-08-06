import { http } from '../../../services/http';
import type { CampusArea, CatSighting, ServicePoint, SightingFilters, SightingWritePayload } from '../types/campus';

type ApiRecord = Record<string, unknown>;

const value = <T>(data: ApiRecord, camel: string, pascal: string): T | undefined =>
  (data[camel] ?? data[pascal]) as T | undefined;

const toArea = (data: ApiRecord): CampusArea => ({
  areaID: value<string>(data, 'areaID', 'AreaID') || value<string>(data, 'areaId', 'AreaId') || '',
  areaName: value<string | null>(data, 'areaName', 'AreaName'),
  campusName: value<string | null>(data, 'campusName', 'CampusName'),
  parentAreaID: value<string | null>(data, 'parentAreaID', 'ParentAreaID') || value<string | null>(data, 'parentAreaId', 'ParentAreaId'),
  areaType: value<string | null>(data, 'areaType', 'AreaType'),
  riskLevel: value<string | null>(data, 'riskLevel', 'RiskLevel'),
  geoBoundary: value<string | null>(data, 'geoBoundary', 'GeoBoundary'),
});

const toPoint = (data: ApiRecord): ServicePoint => ({
  pointID: value<string>(data, 'pointID', 'PointID') || value<string>(data, 'pointId', 'PointId') || '',
  areaID: value<string | null>(data, 'areaID', 'AreaID') || value<string | null>(data, 'areaId', 'AreaId'),
  areaName: value<string | null>(data, 'areaName', 'AreaName'),
  pointName: value<string | null>(data, 'pointName', 'PointName'),
  pointType: value<string | null>(data, 'pointType', 'PointType'),
  longitude: value<number | null>(data, 'longitude', 'Longitude'),
  latitude: value<number | null>(data, 'latitude', 'Latitude'),
  facilityStatus: value<string | null>(data, 'facilityStatus', 'FacilityStatus'),
});

const toSighting = (data: ApiRecord): CatSighting => ({
  sightingID: value<string>(data, 'sightingID', 'SightingID') || value<string>(data, 'sightingId', 'SightingId') || '',
  catID: value<string | null>(data, 'catID', 'CatID') || value<string | null>(data, 'catId', 'CatId'),
  catName: value<string | null>(data, 'catName', 'CatName'),
  areaID: value<string | null>(data, 'areaID', 'AreaID') || value<string | null>(data, 'areaId', 'AreaId'),
  areaName: value<string | null>(data, 'areaName', 'AreaName'),
  longitude: value<number | null>(data, 'longitude', 'Longitude'),
  latitude: value<number | null>(data, 'latitude', 'Latitude'),
  photoUrl: value<string | null>(data, 'photoUrl', 'PhotoUrl'),
  sightingTime: value<string | null>(data, 'sightingTime', 'SightingTime'),
  remark: value<string | null>(data, 'remark', 'Remark'),
  userID: value<string | null>(data, 'userID', 'UserID') || value<string | null>(data, 'userId', 'UserId'),
});

export const campusService = {
  async areas() {
    const { data } = await http.get<ApiRecord[]>('/campus-areas');
    return data.map(toArea);
  },

  async servicePoints(areaId = '') {
    const { data } = await http.get<ApiRecord[]>('/service-points', { params: areaId ? { areaId } : undefined });
    return data.map(toPoint);
  },

  async sightings(filters: SightingFilters = {}) {
    const { data } = await http.get<ApiRecord[]>('/cat-sightings', { params: filters });
    return data.map(toSighting);
  },

  async recentSightings(catId: string, limit = 5) {
    const { data } = await http.get<ApiRecord[]>(`/cat-sightings/recent/by-cat/${encodeURIComponent(catId)}`, { params: { limit } });
    return data.map(toSighting);
  },

  async createSighting(payload: SightingWritePayload) {
    const { data } = await http.post<ApiRecord>('/cat-sightings', payload);
    return toSighting(data);
  },

  async updateSighting(sightingID: string, payload: SightingWritePayload) {
    await http.put(`/cat-sightings/${encodeURIComponent(sightingID)}`, payload);
  },

  async deleteSighting(sightingID: string) {
    await http.delete(`/cat-sightings/${encodeURIComponent(sightingID)}`);
  },
};
