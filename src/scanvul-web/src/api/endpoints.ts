import api from "../lib/axios";
import type {
    InitAdminResponse,
    LoginResponse,
    ListAgentsResponse,
    ListPackagesResponse,
    ListVulnerablePackagesResponse,
} from "../types/api";

export const authApi = {
  init: () => api.post<InitAdminResponse>("/api/v1/auth/init"),
  login: (data: { name: string; password: string }) =>
    api.post<LoginResponse>("/api/v1/auth/login", data),
};

export const agentsApi = {
  list: () =>
    api
      .get<ListAgentsResponse>("/api/v1/admin/agents")
      .then((res) => res.data),

  getPackages: (id: string) =>
    api
      .get<ListPackagesResponse>(`/api/v1/admin/agents/${id}/packages`)
      .then((res) => res.data),

  getVulnPackages: (id: string) =>
    api
      .get<ListVulnerablePackagesResponse>(`/api/v1/admin/agents/${id}/vulnerable-packages`)
      .then((res) => res.data),

  disableAgent: (id: string) =>
    api
      .post(`/api/v1/admin/agents/${id}/commands/disable-agent`)
      .then((res) => res.data),
};
