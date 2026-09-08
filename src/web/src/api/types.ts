export type StatusTone = "success" | "warning" | "error" | "info";

export interface Capabilities {
  canViewReports: boolean;
  canGenerate: boolean;
  canImport: boolean;
  canGenerateAll: boolean;
  canAsk: boolean;
}

export interface MeResponse {
  userName: string;
  roles: string[];
  capabilities: Capabilities;
}

export interface DashboardYearMetric {
  year?: number | null;
  key: string;
  label: string;
  schoolCount: number;
  graduateCount: number;
  sharePercent: number;
  graduateChange?: number | null;
}

export interface DashboardSchoolRow {
  id: number;
  code: string;
  name?: string;
  label: string;
  graduateCount: number;
  yearCounts: Record<string, number>;
}

export interface DashboardResponse {
  databaseStatus: string;
  databaseDetail: string;
  generatedAtUtc: string;
  schoolCount: number;
  graduateCount: number;
  years: DashboardYearMetric[];
  schools: DashboardSchoolRow[];
  topSchools: { code: string; label: string; graduateCount: number }[];
  lastImport: {
    fileName?: string;
    status: string;
    statusTone: StatusTone;
    startedUtc: string;
    importedRowCount: number;
    invalidRowCount: number;
    blankRowCount: number;
  } | null;
  lastRun: {
    id: number;
    status: string;
    statusTone: StatusTone;
    mode: string;
    startedUtc: string;
    totalCount: number;
    successfulCount: number;
    failedCount: number;
    duration: string;
  } | null;
}

export interface SchoolOption {
  id: number;
  code: string;
  name?: string;
  graduateCount: number;
  label: string;
  classYears: string[];
}

export interface GenerateResult {
  runId: number;
  reportRunItemId?: number;
  schoolId: number;
  schoolCode?: string;
  schoolName?: string;
  schoolLabel: string;
  status: string;
  statusTone: StatusTone;
  graduateCount: number;
  duration: string;
  message?: string;
  downloadUrl?: string;
}

export interface RunItem {
  id: number;
  schoolLabel: string;
  schoolCode: string;
  status: string;
  statusTone: StatusTone;
  message?: string;
  downloadUrl?: string;
  downloadName?: string;
}

export interface RunSummary {
  id: number;
  status: string;
  statusTone: StatusTone;
  mode: string;
  startedUtc: string;
  totalCount: number;
  successfulCount: number;
  failedCount: number;
  duration: string;
  items: RunItem[];
}

export interface ReportDetails {
  id: number;
  schoolCode: string;
  schoolName?: string;
  schoolLabel: string;
  status: string;
  statusTone: StatusTone;
  reportYear?: number;
  startedUtc: string;
  completedUtc: string;
  downloadUrl?: string;
  downloadName?: string;
}

export interface SuggestionGroup {
  title: string;
  hint: string;
  questions: string[];
}

export interface AssistantSource {
  documentName: string;
  documentKind: string;
  ruleId: string;
  sourceLocation: string;
  schoolCode?: string;
  reportYear?: number;
}

export interface AssistantAnswer {
  insufficient: boolean;
  answer?: string | null;
  message: string;
  sources: AssistantSource[];
}
