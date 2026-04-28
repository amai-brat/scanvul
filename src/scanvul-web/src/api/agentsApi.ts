import api from "../lib/axios";

export interface AgentResponse {
  id: number;
  isActive: boolean;
  lastPingAt: string;
  lastPackagesScrapingAt: string;
  ipAddress: string;
  operatingSystem: string;
  computerName: string | null;
  memoryInMb: number | null;
  cpuName: string | null;
}

export interface PackageResponse {
  id: number;
  name: string;
  version: string;
}

export type VulnerablePackageStatus =
  | "unknown"
  | "vulnerable"
  | "falsePositive"
  | "patchless"
  | "fixed";

export interface VulnerablePackageResponse {
  id: number;
  cveId: string;
  packageId: number;
  packageName: string;
  packageVersion: string;
  cvssV3_1: number | null;
  cvssV3_0: number | null;
  cvssV2_0: number | null;
  description: string | null;
  status: VulnerablePackageStatus;
}


export interface Identifier {
  type: string;
  link: string | null;
  value: string;
}

export interface Cwe {
  id: string;
  name: string;
}

export interface VulnerableSoftware {
  name: string;
  platform: string;
  vendor: string;
  version: string;
}

export interface BduVulnerablePackageResponse {
  id: number;
  bduId: string;
  packageId: number;
  packageName: string;
  packageVersion: string;
  description: string;
  severity: string;
  identifiers: Identifier[];
  cwes: Cwe[];
  cvss: number | null; // v2
  cvss3: number | null; // v3.0 / v3.1
  cvss4: number | null; // v4.0
  software: VulnerableSoftware[];
  status: VulnerablePackageStatus;
}

export interface ListBduVulnerablePackagesResponse {
  packages: BduVulnerablePackageResponse[];
}

export interface ListAgentsResponse {
  agents: AgentResponse[];
}

export interface ListPackagesResponse {
  packages: PackageResponse[];
}

export interface ListVulnerablePackagesResponse {
  packages: VulnerablePackageResponse[];
}

export interface CommandResponse {
  id: string;
  type: string;
  createdAt: string;
  sentAt: string | null;
  agentResponse: string | null;
  commandParams: object;
}

export interface ListCommandsResponse {
  commands: CommandResponse[];
}

export interface PackageInfo {
  id: number;
  name: string;
  version: string;
}

export interface VulnerablePackage {
  id: number;
  vulnerabilityId: string;
  packageInfoId: number;
  packageName: string;
  packageVersion: string;
  status: VulnerablePackageStatus;
}

export interface ScanSnapshotPayloadSummary {
  packages: number;
  vulnerablePackages: number;
  bduVulnerablePackages: number;
}

export interface ScanSnapshotDiffSummary {
  addedPackages: number;
  removedPackages: number;
  addedVulnerablePackages: number;
  removedVulnerablePackages: number;
  addedBduVulnerablePackages: number;
  removedBduVulnerablePackages: number;
}

export interface ScanSnapshotSummary {
  snapshotId: string;
  createdAt: string;
  payload: ScanSnapshotPayloadSummary;
  diff: ScanSnapshotDiffSummary | null;
}

export interface ListScanSnapshotSummariesResponse {
  summaries: ScanSnapshotSummary[];
}

export interface ScanSnapshotPayloadResponse {
  packages: PackageInfo[];
  vulnerablePackages: VulnerablePackage[];
  bduVulnerablePackages: VulnerablePackage[];
}

export interface ScanSnapshotDiffPayloadResponse {
  addedPackages: PackageInfo[];
  removedPackages: PackageInfo[];
  addedVulnerablePackages: VulnerablePackage[];
  removedVulnerablePackages: VulnerablePackage[];
  addedBduVulnerablePackages: VulnerablePackage[];
  removedBduVulnerablePackages: VulnerablePackage[];
}

export interface GetScanSnapshotPayloadResponse {
  payload: ScanSnapshotPayloadResponse | null;
}

export interface GetScanSnapshotDiffResponse {
  diff: ScanSnapshotDiffPayloadResponse | null;
}

export const agentsApi = {
  list: () =>
    api.get<ListAgentsResponse>("/api/v1/admin/agents").then((res) => res.data),

  getPackages: (id: string) =>
    api
      .get<ListPackagesResponse>(`/api/v1/admin/agents/${id}/packages`)
      .then((res) => res.data),

  getVulnPackages: (id: string, status?: VulnerablePackageStatus) =>
    api
      .get<ListVulnerablePackagesResponse>(
        `/api/v1/admin/agents/${id}/vulnerable-packages`,
        { params: { status } },
      )
      .then((res) => res.data),

  getBduVulnPackages: (id: string, status?: VulnerablePackageStatus) =>
      api
          .get<ListBduVulnerablePackagesResponse>(
              `/api/v1/admin/agents/${id}/bdu-vulnerable-packages`,
              { params: { status } }
          )
          .then((res) => res.data),

  changeVulnStatus: (
    vulnerablePackageId: number,
    status: VulnerablePackageStatus,
  ) =>
    api
      .patch(
        `/api/v1/admin/agents/vulnerable-packages/${vulnerablePackageId}`,
        { status },
      )
      .then((res) => res.data),

  changeVulnStatusBdu: (
    vulnerablePackageId: number,
    status: VulnerablePackageStatus,
  ) =>
    api
      .patch(
        `/api/v1/admin/agents/bdu-vulnerable-packages/${vulnerablePackageId}`,
        { status },
      )
      .then((res) => res.data),

  getCommands: (id: string) =>
    api
      .get<ListCommandsResponse>(`/api/v1/admin/agents/${id}/commands`)
      .then((res) => res.data),

  sendReportPackages: (id: string) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/report-packages`)
      .then((res) => res.data),

  sendUpgradePackage: (
    id: string,
    packageName: string,
    packageManager: string,
  ) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/upgrade-package`, {
        packageName,
        packageManager,
      })
      .then((res) => res.data),

  disableAgent: (id: string) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/disable-agent`)
      .then((res) => res.data),

  getSnapshotSummaries: (agentId: string) =>
    api
      .get<ListScanSnapshotSummariesResponse>(
        `/api/v1/admin/agents/${agentId}/snapshots/summary`,
      )
      .then((res) => res.data),

  getSnapshotPayload: (snapshotId: string) =>
    api
      .get<GetScanSnapshotPayloadResponse>(
        `/api/v1/admin/agents/snapshots/${snapshotId}/payload`,
      )
      .then((res) => res.data),

  getSnapshotLastDiff: (snapshotId: string) =>
    api
      .get<GetScanSnapshotDiffResponse>(
        `/api/v1/admin/agents/snapshots/${snapshotId}/diff`,
      )
      .then((res) => res.data),
};
