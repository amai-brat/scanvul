// src/api/reportsApi.ts
import api from "../lib/axios";

export interface VulnerabilityScanReportResponse {
  id: number;
  createdAt: string;
}

export interface ListVulnerabilityScanReportsResponse {
  reports: VulnerabilityScanReportResponse[];
}

export const reportsApi = {
  list: () =>
    api
      .get<ListVulnerabilityScanReportsResponse>("/api/v1/admin/reports")
      .then((res) => res.data),

  // Change return type to return both URL and Filename
  downloadReport: (reportId: number) =>
    api
      .get(`/api/v1/admin/reports/${reportId}/file`, {
        responseType: "blob",
      })
      .then((response) => {
        // 1. Extract filename from Content-Disposition header
        const disposition = response.headers["content-disposition"];
        let filename = `report_${reportId}.pdf`; // Fallback

        if (disposition) {
          // Try to match filename*=UTF-8''name.ext (standard for modern browsers)
          const utf8Match = disposition.match(/filename\*=UTF-8''(.+)/);
          if (utf8Match?.[1]) {
            filename = decodeURIComponent(utf8Match[1]);
          } else {
            // Fallback to filename="name.ext"
            const filenameMatch = disposition.match(/filename="?([^";]+)"?/);
            if (filenameMatch?.[1]) {
              filename = filenameMatch[1];
            }
          }
        }

        // 2. Create Blob URL
        const url = window.URL.createObjectURL(new Blob([response.data]));
        return { url, filename };
      }),
};
