export type CatGender = 'UNKNOWN' | 'MALE' | 'FEMALE';
export type CatLifeStatus = 'ON_CAMPUS' | 'MISSING' | 'ADOPTED' | 'DECEASED';
export type CatArchiveStatus = 'DRAFT' | 'PUBLISHED' | 'ARCHIVED';
export type BinaryFlag = '0' | '1';

export interface CatSummary {
  catID: string;
  catName?: string | null;
  gender?: CatGender | null;
  breed?: string | null;
  colorPattern?: string | null;
  sterilizedFlag?: number | null;
  earTipFlag?: number | null;
  personalityTags?: string | null;
  mainAreaId?: string | null;
  mainAreaName?: string | null;
  lifeStatus?: CatLifeStatus | null;
  archiveStatus?: CatArchiveStatus | null;
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
  gender: CatGender;
  breed: string;
  colorPattern: string;
  sterilizedFlag: number;
  earTipFlag: number;
  personalityTags: string;
  mainAreaId: string;
  lifeStatus: CatLifeStatus;
  archiveStatus?: CatArchiveStatus;
}

export interface CampusArea {
  areaID: string;
  areaName?: string | null;
}

export interface CatFilters {
  mainAreaId?: string;
  lifeStatus?: CatLifeStatus;
  archiveStatus?: CatArchiveStatus;
}

export interface CatFormState {
  catName: string;
  gender: CatGender;
  breed: string;
  colorPattern: string;
  sterilizedFlag: BinaryFlag;
  earTipFlag: BinaryFlag;
  personalityTags: string;
  mainAreaId: string;
  lifeStatus: CatLifeStatus;
  archiveStatus: CatArchiveStatus;
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
