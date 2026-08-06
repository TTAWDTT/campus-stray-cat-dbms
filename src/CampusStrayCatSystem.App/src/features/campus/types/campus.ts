export interface CampusArea {
  areaID: string;
  areaName?: string | null;
  campusName?: string | null;
  parentAreaID?: string | null;
  areaType?: string | null;
  riskLevel?: string | null;
  geoBoundary?: string | null;
}

export interface ServicePoint {
  pointID: string;
  areaID?: string | null;
  areaName?: string | null;
  pointName?: string | null;
  pointType?: string | null;
  longitude?: number | null;
  latitude?: number | null;
  facilityStatus?: string | null;
}

export interface CatSighting {
  sightingID: string;
  catID?: string | null;
  catName?: string | null;
  areaID?: string | null;
  areaName?: string | null;
  longitude?: number | null;
  latitude?: number | null;
  photoUrl?: string | null;
  sightingTime?: string | null;
  remark?: string | null;
  userID?: string | null;
}

export interface SightingWritePayload {
  catID: string;
  areaID: string;
  longitude?: number;
  latitude?: number;
  photoUrl?: string;
  sightingTime?: string;
  remark?: string;
}

export interface SightingFilters {
  catId?: string;
  areaId?: string;
  from?: string;
  to?: string;
}
