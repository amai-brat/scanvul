import api from "../lib/axios";
import type { PackageManager } from "../utils/packageManager";

export interface PackageMetadata {
  name: string;
  url: string;
  lastVersion: string;
  summary: string;
}

export interface SearchPackageResponse {
  packages: PackageMetadata[]
}

export const packageManagerApi = {
  search: (packageName: string, packageManager: PackageManager) =>
    api
      .get(`/api/v1/admin/package-managers/search`, {
        params: {
          packageName,
          packageManager,
        },
      })
      .then((res) => res.data),
};