import { useQuery } from "@tanstack/react-query";
import { Book, Download } from "lucide-react";
import { Card } from "../../../components/Card";
import { useTranslation } from "react-i18next";
import { reportsApi } from "../../../api/reportsApi";
import { useMemo } from "react";
import { toast } from "react-toastify"; // 1. Import toast
import type { AxiosError } from "axios";

export const ReportsList = () => {
  const { t } = useTranslation();

  const { data, isLoading, error } = useQuery({
    queryKey: ["reports"],
    queryFn: reportsApi.list,
    refetchInterval: 30000,
  });

  const sortedReports = useMemo(() => {
    if (!data?.reports) return [];
    return [...data.reports].sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    );
  }, [data]);

   const handleDownload = async (reportId: number) => {
     try {
       const { url, filename } = await reportsApi.downloadReport(reportId);

       // Create temporary link element
       const link = document.createElement("a");
       link.href = url;
       link.setAttribute("download", filename);
       document.body.appendChild(link);

       // Trigger click
       link.click();

       // Cleanup
       link.remove();
       window.URL.revokeObjectURL(url);
     } catch (err) {
       console.error("Failed to download report", err);
       toast.error(t("reports.download_error", { status: (err as AxiosError)?.response?.status }));
     }
   };

  if (isLoading)
    return (
      <div className="p-8 text-center">{t("reports.loading_reports")}</div>
    );
  if (error)
    return (
      <div className="p-8 text-center text-red-500">
        {t("reports.loading_reports_error")}
      </div>
    );

  return (
    <div className="space-y-6 relative">
      <div className="flex items-center justify-between">
        <h2 className="text-3xl font-bold">{t("reports.title")}</h2>
        <span className="bg-primary/10 text-primary px-3 py-1 rounded-full text-sm font-medium">
          {t("reports.total")}: {sortedReports.length}
        </span>
      </div>

      <div className="grid gap-2 max-h-150 overflow-y-auto border border-gray-200 dark:border-gray-800 rounded-lg p-2 bg-gray-50/50 dark:bg-gray-900/20">
        {sortedReports.map((report) => (
          <div
            key={report.id}
            className="bg-card hover:border-primary/50 border border-gray-200 dark:border-gray-700 rounded-md p-3 shadow-sm transition-all flex items-center justify-between group"
          >
            {/* Left Side: Icon and Info */}
            <div className="flex items-center gap-3">
              <div className="p-2 bg-blue-50 dark:bg-blue-900/20 rounded-full text-primary">
                <Book className="h-4 w-4" />
              </div>
              <div>
                <h3 className="font-medium text-sm group-hover:text-primary transition-colors">
                  {t("reports.report_at", {
                    time: new Date(report.createdAt).toLocaleString(navigator.language),
                  })}
                </h3>
              </div>
            </div>

            {/* Right Side: Download Button */}
            <div className="flex items-center gap-4">
              <button
                className="text-gray-500 hover:text-primary transition-colors p-1 cursor-pointer"
                title="Download"
                onClick={(e) => {
                  e.stopPropagation();
                  handleDownload(report.id);
                }}
              >
                <Download className="h-5 w-5" />
              </button>
            </div>
          </div>
        ))}

        {sortedReports.length === 0 && (
          <Card title="No reports" className="text-center py-6">
            <p className="text-gray-500 text-sm">{t("reports.no_reports")}</p>
          </Card>
        )}
      </div>
    </div>
  );
};
