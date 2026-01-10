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

export const agentsApi = {
  list: () =>
    api.get<ListAgentsResponse>("/api/v1/admin/agents").then((res) => res.data),

  getPackages: (id: string) =>
    api
      .get<ListPackagesResponse>(`/api/v1/admin/agents/${id}/packages`)
      .then((res) => res.data),

  getVulnPackages: (id: string) =>
    api
      .get<ListVulnerablePackagesResponse>(
        `/api/v1/admin/agents/${id}/vulnerable-packages`
      )
      .then((res) => res.data),

  markFalsePositive: (vulnerablePackageId: number) =>
    api
      .patch(
        `/api/v1/admin/agents/vulnerable-packages/${vulnerablePackageId}/false-positive`
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

  sendUpgradePackage: (id: string, packageName: string) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/upgrade-package`, {
        packageName,
      })
      .then((res) => res.data),

  disableAgent: (id: string) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/disable-agent`)
      .then((res) => res.data),
};
