import api from "../lib/axios";

export interface LoginResponse {
  token: string;
}

export interface InitAdminResponse {
  name: string;
  password?: string;
}

export const authApi = {
  init: () => api.post<InitAdminResponse>("/api/v1/auth/init"),
  login: (data: { name: string; password: string }) =>
    api.post<LoginResponse>("/api/v1/auth/login", data),
};
