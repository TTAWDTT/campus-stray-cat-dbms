export interface CatSummary {
  catID: string;
  catName?: string | null;
  gender?: string | null;
  breed?: string | null;
  colorPattern?: string | null;
  sterilizedFlag?: number | null;
  earTipFlag?: number | null;
  personalityTags?: string | null;
  mainAreaId?: string | null;
  mainAreaName?: string | null;
  lifeStatus?: string | null;
  archiveStatus?: string | null;
  primaryPhotoUrl?: string | null;
}

export interface CatPhoto {
  photoID: string;
  catID?: string | null;
  photoUrl: string;
  uploadUserID?: string | null;
  uploadTime?: string | null;
  isPrimary: number;
}

export interface CatWritePayload {
  catName: string;
  gender: string;
  breed: string;
  colorPattern: string;
  sterilizedFlag: number;
  earTipFlag: number;
  personalityTags: string;
  mainAreaId: string;
  lifeStatus: string;
  archiveStatus?: string;
}

export interface CampusArea {
  areaID: string;
  areaName?: string | null;
}

export interface CatFilters {
  mainAreaId?: string;
  lifeStatus?: string;
  archiveStatus?: string;
}

export interface CatFormState {
  catName: string;
  gender: string;
  breed: string;
  colorPattern: string;
  sterilizedFlag: string;
  earTipFlag: string;
  personalityTags: string;
  mainAreaId: string;
  lifeStatus: string;
  archiveStatus: string;
}

export const emptyCatForm: CatFormState = {
  catName: '',
  gender: 'UNKNOWN',
  breed: '',
  colorPattern: '',
  sterilizedFlag: '0',
  earTipFlag: '0',
  personalityTags: '',
  mainAreaId: '',
  lifeStatus: 'ON_CAMPUS',
  archiveStatus: 'DRAFT',
};
