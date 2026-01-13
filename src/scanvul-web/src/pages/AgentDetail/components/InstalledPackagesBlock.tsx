import { useQuery } from "@tanstack/react-query";
import { agentsApi } from "../../../api/agentsApi";
import { useState } from "react";
import { ChevronDown, ChevronUp, Loader2, Package } from "lucide-react";
import { useTranslation } from "react-i18next";

export const InstalledPackagesBlock = ({ agentId }: { agentId: string }) => {
  const { t } = useTranslation();
  const [isPackagesOpen, setIsPackagesOpen] = useState(false);

  const { data: pkgData, isLoading: pkgLoading } = useQuery({
    queryKey: ["packages", agentId],
    queryFn: () => agentsApi.getPackages(agentId!),
    enabled: !!agentId && isPackagesOpen,
  });

  return (
    <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
      <button
        onClick={() => setIsPackagesOpen(!isPackagesOpen)}
        className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
      >
        <div className="space-y-1">
          <h3 className="font-bold text-lg">
            {t("agent_details.installed_packages")}
          </h3>
          <p className="text-sm text-gray-500">
            {pkgData
              ? t("agent_details.installed_packages_total", {
                  amount: pkgData.packages.length,
                })
              : t("agent_details.installed_packages_click")}
          </p>
        </div>
        {isPackagesOpen ? (
          <ChevronUp className="w-5 h-5 text-gray-400" />
        ) : (
          <ChevronDown className="w-5 h-5 text-gray-400" />
        )}
      </button>

      {isPackagesOpen && (
        <div className="border-t border-gray-100 dark:border-gray-800">
          {pkgLoading ? (
            <div className="p-8 flex justify-center items-center text-gray-500 gap-2">
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>{t("agent_details.installed_packages_fetching")}</span>
            </div>
          ) : (
            <div className="overflow-x-auto max-h-96 overflow-y-auto">
              <table className="w-full text-sm text-left">
                <thead className="text-xs uppercase bg-gray-50 dark:bg-gray-800/50 text-gray-500 sticky top-0 backdrop-blur-sm">
                  <tr>
                    <th className="px-6 py-3">
                      {t("agent_details.package_name")}
                    </th>
                    <th className="px-6 py-3">{t("agent_details.version")}</th>
                    <th className="px-6 py-3 text-right">
                      {t("agent_details.id")}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {pkgData?.packages.map((pkg) => (
                    <tr
                      key={pkg.id}
                      className="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30"
                    >
                      <td className="px-6 py-3 font-medium flex items-center gap-2">
                        <Package className="w-4 h-4 text-gray-400" />
                        {pkg.name}
                      </td>
                      <td className="px-6 py-3 font-mono text-gray-600 dark:text-gray-400">
                        {pkg.version}
                      </td>
                      <td className="px-6 py-3 text-right text-gray-400">
                        #{pkg.id}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}