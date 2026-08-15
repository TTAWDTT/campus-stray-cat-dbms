export type TnrStatus = 'DISCOVERED' | 'CAPTURED' | 'SURGERY' | 'RECOVERING' | 'RELEASED' | 'CANCELLED';
export type MedicalRecordType = 'VACCINATION' | 'CHECKUP' | 'TREATMENT' | 'SURGERY' | 'DEWORMING' | 'EMERGENCY' | 'OTHER';
export type ReminderType = 'VACCINATION' | 'DEWORMING' | 'STERILIZATION' | 'FOLLOW_UP' | 'OTHER';
export type ReminderStatus = 'PENDING' | 'SENT' | 'COMPLETED';
export type EmergencyUrgency = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
export type EmergencyStatus = 'SUBMITTED' | 'ASSIGNED' | 'PROCESSING' | 'RESOLVED' | 'CLOSED';
export type MissingAlertStatus = 'PROCESSING' | 'FOUND' | 'CLOSED';

export interface TnrCase {
  caseID: string;
  catID: string;
  responsibleUserID?: string | null;
  currentStatus?: TnrStatus | null;
  hospitalName?: string | null;
  captureTime?: string | null;
  surgeryTime?: string | null;
  releaseTime?: string | null;
  totalCost?: number | null;
}

export interface MedHealthRecord {
  recordID: string;
  catID: string;
  recordType?: MedicalRecordType | null;
  hospitalName?: string | null;
  diagnosis?: string | null;
  recordDate?: string | null;
  nextDueDate?: string | null;
  attachmentUrl?: string | null;
}

export interface MedReminder {
  reminderID: string;
  recordID?: string | null;
  catID?: string | null;
  reminderType: ReminderType;
  receiverUserID?: string | null;
  reminderTime?: string | null;
  sendStatus: ReminderStatus;
}

export interface EmergencyReport {
  reportID: string;
  reporterUserID: string;
  areaID: string;
  animalType: 'CAT' | 'DOG' | 'OTHER';
  photoURL?: string | null;
  longitude?: number | null;
  latitude?: number | null;
  reportTime?: string | null;
  urgencyLevel: EmergencyUrgency;
  processStatus: EmergencyStatus;
  handlerUserID?: string | null;
  processResult?: string | null;
}

export interface MissingAlert {
  alertID: string;
  catID: string;
  lastSightingID?: string | null;
  lastSightingTime?: string | null;
  thresholdDays?: number | null;
  alertTime?: string | null;
  alertStatus: MissingAlertStatus;
  handlerUserID?: string | null;
  closeTime?: string | null;
  remark?: string | null;
}

export interface TnrStatusLog {
  logID: string;
  caseID: string;
  fromStatus?: string | null;
  toStatus?: string | null;
  operatorID?: string | null;
  opTime?: string | null;
  remark?: string | null;
}
