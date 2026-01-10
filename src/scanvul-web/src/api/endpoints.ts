import api from "../lib/axios";
import type {
    InitAdminResponse,
    LoginResponse,
    ListAgentsResponse,
    ListPackagesResponse,
    ListVulnerablePackagesResponse,
    ListCommandsResponse,
} from "../types/api";

export const authApi = {
  init: () => api.post<InitAdminResponse>("/api/v1/auth/init"),
  login: (data: { name: string; password: string }) =>
    api.post<LoginResponse>("/api/v1/auth/login", data),
};

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
