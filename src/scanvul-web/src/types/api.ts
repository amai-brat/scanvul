export interface LoginResponse {
  token: string;
}

export interface InitAdminResponse {
  name: string;
  password?: string;
}

export interface AgentResponse {
  id: number;
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
