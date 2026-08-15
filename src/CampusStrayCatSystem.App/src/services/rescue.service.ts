import { http } from './http';
import type { EmergencyReport, MedHealthRecord, MedReminder, MissingAlert, TnrCase, TnrStatusLog } from '../features/rescue/rescue.types';
import type { CatSighting } from '../features/campus/types/campus';

type ApiRecord = Record<string, unknown>;

const value = <T>(data: ApiRecord, camel: string, pascal: string): T | null => (data[camel] ?? data[pascal] ?? null) as T | null;
const text = (data: ApiRecord, camel: string, pascal: string) => value<string>(data, camel, pascal) || '';

const toTnr = (data: ApiRecord): TnrCase => ({
  caseID: text(data, 'caseID', 'CaseID'), catID: text(data, 'catID', 'CatID'), responsibleUserID: value<string>(data, 'responsibleUserID', 'ResponsibleUserID'),
  currentStatus: value<TnrCase['currentStatus']>(data, 'currentStatus', 'CurrentStatus'), hospitalName: value<string>(data, 'hospitalName', 'HospitalName'),
  captureTime: value<string>(data, 'captureTime', 'CaptureTime'), surgeryTime: value<string>(data, 'surgeryTime', 'SurgeryTime'), releaseTime: value<string>(data, 'releaseTime', 'ReleaseTime'), totalCost: value<number>(data, 'totalCost', 'TotalCost'),
});

const toMedical = (data: ApiRecord): MedHealthRecord => ({
  recordID: text(data, 'recordID', 'RecordID'), catID: text(data, 'catID', 'CatID'), recordType: value<MedHealthRecord['recordType']>(data, 'recordType', 'RecordType'),
  hospitalName: value<string>(data, 'hospitalName', 'HospitalName'), diagnosis: value<string>(data, 'diagnosis', 'Diagnosis'),
  recordDate: value<string>(data, 'recordDate', 'RecordDate'), nextDueDate: value<string>(data, 'nextDueDate', 'NextDueDate'), attachmentUrl: value<string>(data, 'attachmentUrl', 'AttachmentUrl'),
});

const toReminder = (data: ApiRecord): MedReminder => ({
  reminderID: text(data, 'reminderID', 'ReminderID'), recordID: value<string>(data, 'recordID', 'RecordID'), catID: value<string>(data, 'catID', 'CatID'),
  reminderType: text(data, 'reminderType', 'ReminderType') as MedReminder['reminderType'], receiverUserID: value<string>(data, 'receiverUserID', 'ReceiverUserID'),
  reminderTime: value<string>(data, 'reminderTime', 'ReminderTime'), sendStatus: text(data, 'sendStatus', 'SendStatus') as MedReminder['sendStatus'],
});

const toEmergency = (data: ApiRecord): EmergencyReport => ({
  reportID: text(data, 'reportID', 'ReportID'), reporterUserID: text(data, 'reporterUserID', 'ReporterUserID'), areaID: text(data, 'areaID', 'AreaID'),
  animalType: text(data, 'animalType', 'AnimalType') as EmergencyReport['animalType'], photoURL: value<string>(data, 'photoURL', 'PhotoURL'),
  longitude: value<number>(data, 'longitude', 'Longitude'), latitude: value<number>(data, 'latitude', 'Latitude'), reportTime: value<string>(data, 'reportTime', 'ReportTime'),
  urgencyLevel: text(data, 'urgencyLevel', 'UrgencyLevel') as EmergencyReport['urgencyLevel'], processStatus: text(data, 'processStatus', 'ProcessStatus') as EmergencyReport['processStatus'],
  handlerUserID: value<string>(data, 'handlerUserID', 'HandlerUserID'), processResult: value<string>(data, 'processResult', 'ProcessResult'),
});

const toAlert = (data: ApiRecord): MissingAlert => ({
  alertID: text(data, 'alertID', 'AlertID'), catID: text(data, 'catID', 'CatID'), lastSightingID: value<string>(data, 'lastSightingID', 'LastSightingID'),
  lastSightingTime: value<string>(data, 'lastSightingTime', 'LastSightingTime'), thresholdDays: value<number>(data, 'thresholdDays', 'ThresholdDays'),
  alertTime: value<string>(data, 'alertTime', 'AlertTime'), alertStatus: text(data, 'alertStatus', 'AlertStatus') as MissingAlert['alertStatus'],
  handlerUserID: value<string>(data, 'handlerUserID', 'HandlerUserID'), closeTime: value<string>(data, 'closeTime', 'CloseTime'), remark: value<string>(data, 'remark', 'Remark'),
});

const toLog = (data: ApiRecord): TnrStatusLog => ({
  logID: text(data, 'logID', 'LogID'), caseID: text(data, 'caseID', 'CaseID'), fromStatus: value<string>(data, 'fromStatus', 'FromStatus'),
  toStatus: value<string>(data, 'toStatus', 'ToStatus'), operatorID: value<string>(data, 'operatorID', 'OperatorID'), opTime: value<string>(data, 'opTime', 'OpTime'), remark: value<string>(data, 'remark', 'Remark'),
});

const toSighting = (data: ApiRecord): CatSighting => ({
  sightingID: text(data, 'sightingID', 'SightingID'), catID: value<string>(data, 'catID', 'CatID'), userID: value<string>(data, 'userID', 'UserID'), areaID: value<string>(data, 'areaID', 'AreaID'),
  longitude: value<number>(data, 'longitude', 'Longitude'), latitude: value<number>(data, 'latitude', 'Latitude'), photoUrl: value<string>(data, 'photoUrl', 'PhotoUrl'), sightingTime: value<string>(data, 'sightingTime', 'SightingTime'), remark: value<string>(data, 'remark', 'Remark'),
});

export const rescueService = {
  async tnrCases() { const { data } = await http.get<ApiRecord[]>('/TnrCases'); return data.map(toTnr); },
  async createTnr(payload: Omit<TnrCase, 'caseID'>) { const { data } = await http.post<ApiRecord>('/TnrCases', payload); return toTnr(data); },
  async updateTnrStatus(caseID: string, newStatus: string, remark?: string) { await http.put(`/TnrCases/${encodeURIComponent(caseID)}/status`, { newStatus, remark }); },
  async tnrLogs(caseID: string) { const { data } = await http.get<ApiRecord[]>(`/TnrStatusLogs/case/${encodeURIComponent(caseID)}`); return data.map(toLog); },
  async medicalRecords() { const { data } = await http.get<ApiRecord[]>('/MedHealthRecords'); return data.map(toMedical); },
  async createMedical(payload: Omit<MedHealthRecord, 'recordID'>) { const { data } = await http.post<ApiRecord>('/MedHealthRecords', payload); return toMedical(data); },
  async reminders() { const { data } = await http.get<ApiRecord[]>('/MedReminder'); return data.map(toReminder); },
  async markReminderSent(reminderID: string) { await http.put(`/MedReminder/${encodeURIComponent(reminderID)}/sent`); },
  async completeReminder(reminderID: string) { await http.put(`/MedReminder/${encodeURIComponent(reminderID)}/complete`); },
  async emergencies() { const { data } = await http.get<ApiRecord[]>('/EmergencyReports'); return data.map(toEmergency); },
  async createEmergency(payload: Omit<EmergencyReport, 'reportID' | 'reporterUserID' | 'reportTime' | 'processStatus' | 'handlerUserID' | 'processResult'>) { const { data } = await http.post<ApiRecord>('/EmergencyReports', payload); return toEmergency(data); },
  async assignEmergency(reportID: string, handlerUserID: string) { await http.put(`/EmergencyReports/${encodeURIComponent(reportID)}/assign`, handlerUserID); },
  async updateEmergency(reportID: string, processStatus: string, processResult?: string) { await http.put(`/EmergencyReports/${encodeURIComponent(reportID)}/status`, { processStatus, processResult }); },
  async alerts() { const { data } = await http.get<ApiRecord[]>('/MissingAlerts'); return data.map(toAlert); },
  async createMissingSighting(payload: Omit<CatSighting, 'sightingID' | 'userID'>) { const { data } = await http.post<ApiRecord>('/MissingAlerts/sightings', payload); return toSighting(data); },
  async createAlert(payload: Omit<MissingAlert, 'alertID' | 'alertTime' | 'handlerUserID' | 'closeTime'>) { const { data } = await http.post<ApiRecord>('/MissingAlerts', payload); return toAlert(data); },
  async updateAlert(alertID: string, alertStatus: string, remark?: string) { await http.put(`/MissingAlerts/${encodeURIComponent(alertID)}/status`, { alertStatus, remark }); },
};
