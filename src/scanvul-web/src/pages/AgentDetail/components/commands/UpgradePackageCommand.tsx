import { useMemo, useState, useEffect } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Trans, useTranslation } from "react-i18next";
import { toast } from "react-toastify";
import {
  DownloadCloud,
  Loader2,
  Play,
  Search,
  AlertTriangle,
  Package,
  ExternalLink,
  Settings,
  ChevronDown,
  ChevronRight,
} from "lucide-react";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import {
  packageManagerApi,
  type PackageMetadata,
} from "../../../../api/packageManagerApi";
import {
  getPackageManagers,
  isVersionsSupported,
  type PackageManager,
} from "../../../../utils/packageManager";
import { modalEffect } from "../../../../utils/modal";

export const UpgradePackageCommand = ({
  agent,
  isCommandsOpen,
}: {
  agent: AgentResponse;
  isCommandsOpen: boolean;
}) => {
  const { t } = useTranslation();
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);

  return (
    <>
      <button
        disabled={!agent.isActive}
        onClick={() => setShowUpgradeModal(true)}
        className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50 group"
      >
        <div className="flex items-center gap-3 max-w-[90%]">
          <div className="bg-emerald-50 text-emerald-600 p-2 rounded-md">
            <DownloadCloud className="w-4 h-4" />
          </div>
          <div className="text-left">
            <div className="text-sm font-semibold text-gray-900 dark:text-gray-100">
              {t("agent_details.command_upgrade_package_title")}
            </div>
            <div className="text-xs text-gray-500">
              {t("agent_details.command_upgrade_package_desc")}
            </div>
          </div>
        </div>
        <Play className="w-4 h-4 text-gray-400 group-hover:text-gray-600 dark:group-hover:text-gray-300 transition-colors" />
      </button>

      {showUpgradeModal && (
        <UpgradePackageModal
          agent={agent}
          isCommandsOpen={isCommandsOpen}
          onClose={() => setShowUpgradeModal(false)}
        />
      )}
    </>
  );
};

const UpgradePackageModal = ({
  agent,
  isCommandsOpen,
  onClose,
}: {
  agent: AgentResponse;
  isCommandsOpen: boolean;
  onClose: () => void;
}) => {
  const queryClient = useQueryClient();
  const { t } = useTranslation();

  const [upgradePackageName, setUpgradePackageName] = useState("");
  const [upgradePackageVersion, setUpgradePackageVersion] = useState<
    string | undefined
  >();
  const [expandedPackage, setExpandedPackage] = useState<string | null>(null);
  const [searchResults, setSearchResults] = useState<PackageMetadata[]>([]);
  const [packageManager, setPackageManager] = useState<
    PackageManager | string
  >();

  useEffect(() => {
    modalEffect(onClose);
  }, [onClose]);

  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns", agent.id],
    queryFn: () => agentsApi.getVulnPackages(agent.id.toString()),
  });

  const uniqueVulnPackages = useMemo(() => {
    if (!vulnData?.packages) return [];
    return Array.from(
      new Set(vulnData.packages.map((p) => p.packageName)),
    ).sort();
  }, [vulnData]);

  const availablePackageManagers = useMemo(() => {
    return getPackageManagers(agent.operatingSystem || "unknown");
  }, [agent.operatingSystem]);

  const activePackageManager = useMemo(() => {
    if (
      packageManager &&
      availablePackageManagers.includes(packageManager as PackageManager)
    ) {
      return packageManager;
    }
    return availablePackageManagers.length > 0
      ? availablePackageManagers[0]
      : undefined;
  }, [packageManager, availablePackageManagers]);

  const versionsSupported = useMemo(() => {
    return activePackageManager
      ? isVersionsSupported(activePackageManager as PackageManager)
      : false;
  }, [activePackageManager]);

  const searchPackageMutation = useMutation({
    mutationFn: async () => {
      if (!activePackageManager) {
        throw new Error(t("agent_details.err_no_package_manager"));
      }
      return packageManagerApi.search(
        upgradePackageName,
        activePackageManager as PackageManager,
      );
    },
    onSuccess: (data) => {
      setSearchResults(data.packages);
      setExpandedPackage(null);
    },
    onError: (err) => {
      console.error("Failed to search packages", err);
      toast.error(
        t("app.err", {
          msg: err.message,
        }),
      );
    },
  });

  const upgradePackageMutation = useMutation({
    mutationFn: (pkgName: string) =>
      agentsApi.sendUpgradePackage(
        agent.id.toString(),
        pkgName,
        activePackageManager!,
        upgradePackageVersion,
      ),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({
          queryKey: ["commands", agent.id.toString()],
        });
      toast.info(t("agent_details.command_upgrade_package_toast_msg"));
      onClose();
    },
  });

  const handleVulnClick = (name: string) => {
    setUpgradePackageName(name);
    setUpgradePackageVersion(undefined);
    setTimeout(() => searchPackageMutation.mutate(), 0);
  };

  const handleResultClick = (name: string) => {
    if (upgradePackageName !== name) {
      setUpgradePackageName(name);
      setUpgradePackageVersion(undefined);
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-in fade-in m-0"
      onClick={onClose}
    >
      <div
        className="bg-white dark:bg-gray-900 rounded-lg shadow-xl w-full max-w-lg border border-gray-200 dark:border-gray-800 overflow-hidden flex flex-col max-h-[90vh]"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex justify-between items-center shrink-0">
          <h3 className="font-semibold text-lg text-gray-900 dark:text-gray-100">
            {t("agent_details.command_upgrade_package_title")}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors p-1"
          >
            ✕
          </button>
        </div>

        {/* Scrollable Content */}
        <div className="p-6 space-y-6 overflow-y-auto custom-scrollbar">
          <p className="text-sm text-gray-600 dark:text-gray-300">
            {t("agent_details.search_package_desc")}
          </p>

          {/* 1. Vulnerable Packages Section */}
          {!vulnLoading && uniqueVulnPackages.length > 0 && (
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-xs font-semibold text-amber-600 dark:text-amber-500">
                <AlertTriangle className="w-3 h-3" />
                <span>{t("agent_details.detected_vuln_pkgs")}</span>
              </div>
              <div className="flex flex-wrap gap-2">
                {uniqueVulnPackages.map((pkg) => (
                  <button
                    key={pkg}
                    onClick={() => handleVulnClick(pkg)}
                    className="px-2 py-1 text-xs rounded-md border border-amber-200 bg-amber-50 text-amber-700 hover:bg-amber-100 hover:border-amber-300 transition-colors"
                  >
                    {pkg}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* 2. Package Manager Selection */}
          {availablePackageManagers.length > 0 && (
            <div className="space-y-2">
              <div className="flex items-center gap-2 text-xs font-semibold text-gray-700 dark:text-gray-300">
                <Settings className="w-3 h-3" />
                <span>{t("agent_details.package_manager")}</span>
              </div>
              <div className="flex flex-wrap gap-2">
                {availablePackageManagers.map((pm) => (
                  <button
                    key={pm}
                    onClick={() => {
                      setPackageManager(pm);
                      setSearchResults([]);
                      setUpgradePackageName("");
                      setUpgradePackageVersion(undefined);
                      setExpandedPackage(null);
                    }}
                    className={`px-3 py-1.5 text-xs font-medium rounded-md border transition-all ${
                      activePackageManager === pm
                        ? "bg-slate-800 text-white border-slate-800 dark:bg-blue-600 dark:border-blue-600 shadow-sm"
                        : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700 dark:hover:bg-gray-700"
                    }`}
                  >
                    {pm}
                  </button>
                ))}
              </div>
            </div>
          )}

          {/* 2.1. Pacman Warning */}
          {activePackageManager === "pacman" && (
            <div className="">
              <div className="flex items-center gap-2 text-xs font-semibold border border-amber-200 bg-amber-50 text-amber-700 p-2 rounded-md">
                <span>
                  <Trans
                    i18nKey="agent_details.pacman_warning"
                    components={{
                      1: (
                        <a
                          href="https://wiki.archlinux.org/title/System_maintenance#Partial_upgrades_are_unsupported"
                          target="_blank"
                          rel="noopener noreferrer"
                          className="underline hover:text-amber-900"
                        />
                      ),
                    }}
                  />
                </span>
              </div>
            </div>
          )}

          {/* 3. Search Input */}
          <div className="space-y-2">
            <label className="block text-xs font-medium text-gray-700 dark:text-gray-300">
              {t("agent_details.package_name")}
            </label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <input
                  type="text"
                  value={upgradePackageName}
                  onChange={(e) => {
                    setUpgradePackageName(e.target.value);
                    setUpgradePackageVersion(undefined);
                  }}
                  onKeyDown={(e) => {
                    if (e.key === "Enter") searchPackageMutation.mutate();
                  }}
                  placeholder="e.g. 7zip"
                  className="w-full pl-3 pr-3 py-2 border rounded-md dark:bg-gray-800 dark:border-gray-700 dark:text-white focus:ring-2 focus:ring-blue-500 outline-none transition-colors"
                />
              </div>
              <button
                onClick={() => searchPackageMutation.mutate()}
                disabled={
                  !upgradePackageName.trim() ||
                  searchPackageMutation.isPending ||
                  !activePackageManager
                }
                className="px-3 py-2 bg-gray-100 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-md hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors disabled:opacity-50"
                title="Search Package Manager"
              >
                {searchPackageMutation.isPending ? (
                  <Loader2 className="w-4 h-4 animate-spin text-gray-600 dark:text-gray-300" />
                ) : (
                  <Search className="w-4 h-4 text-gray-600 dark:text-gray-300" />
                )}
              </button>
            </div>
          </div>

          {/* 4. Search Results */}
          {searchResults.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
                {t("agent_details.search_results")}
              </h4>
              <div className="space-y-2 max-h-72 overflow-y-auto pr-1 custom-scrollbar">
                {searchResults.map((pkg) => (
                  <div
                    key={`${pkg.name}`}
                    onClick={() => handleResultClick(pkg.name)}
                    className={`p-3 rounded-md border cursor-pointer transition-all group ${
                      upgradePackageName === pkg.name
                        ? "border-blue-500 bg-blue-50 dark:bg-blue-900/20"
                        : "border-gray-200 dark:border-gray-700 hover:border-blue-300 hover:shadow-sm"
                    }`}
                  >
                    <div className="flex justify-between items-start">
                      <div className="flex items-center gap-2">
                        <Package className="w-4 h-4 text-gray-400" />
                        <span className="font-medium text-sm text-gray-900 dark:text-gray-100">
                          {pkg.name}
                        </span>
                        <span className="text-xs bg-gray-100 dark:bg-gray-800 px-1.5 py-0.5 rounded text-gray-500">
                          {pkg.lastVersion}
                        </span>
                      </div>
                      {pkg.url && (
                        <a
                          href={pkg.url}
                          target="_blank"
                          rel="noreferrer"
                          onClick={(e) => e.stopPropagation()}
                          className="text-gray-400 hover:text-blue-500"
                        >
                          <ExternalLink className="w-3 h-3" />
                        </a>
                      )}
                    </div>
                    {pkg.summary && (
                      <p className="mt-1 text-xs text-gray-500 dark:text-gray-400 line-clamp-2">
                        {pkg.summary}
                      </p>
                    )}

                    {/* Versions Accordion */}
                    {versionsSupported &&
                      pkg.versions &&
                      pkg.versions.length > 0 && (
                        <div
                          className="mt-3 border-t border-gray-200 dark:border-gray-700 pt-2"
                          onClick={(e) => e.stopPropagation()}
                        >
                          <button
                            type="button"
                            onClick={(e) => {
                              e.stopPropagation();
                              setExpandedPackage(
                                expandedPackage === pkg.name ? null : pkg.name,
                              );
                            }}
                            className="flex items-center gap-1 text-xs font-medium text-gray-600 dark:text-gray-400 hover:text-gray-900 dark:hover:text-gray-200 transition-colors"
                          >
                            {expandedPackage === pkg.name ? (
                              <ChevronDown className="w-3 h-3" />
                            ) : (
                              <ChevronRight className="w-3 h-3" />
                            )}
                            {t("agent_details.versions", "Versions")}
                          </button>

                          {expandedPackage === pkg.name && (
                            <div className="mt-2 flex flex-wrap gap-2 max-h-40 overflow-y-auto custom-scrollbar p-1">
                              {pkg.versions.map((v) => (
                                <button
                                  key={v}
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    setUpgradePackageName(pkg.name);
                                    setUpgradePackageVersion(
                                      upgradePackageVersion === v
                                        ? undefined
                                        : v,
                                    );
                                  }}
                                  className={`px-2 py-1 text-xs rounded border transition-colors ${
                                    upgradePackageName === pkg.name &&
                                    upgradePackageVersion === v
                                      ? "bg-blue-600 border-blue-600 text-white"
                                      : "bg-white dark:bg-gray-800 border-gray-300 dark:border-gray-600 text-gray-700 dark:text-gray-300 hover:bg-gray-50 dark:hover:bg-gray-700"
                                  }`}
                                >
                                  {v}
                                </button>
                              ))}
                            </div>
                          )}
                        </div>
                      )}
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* No Results State */}
          {searchPackageMutation.isSuccess &&
            searchResults.length === 0 &&
            upgradePackageName && (
              <p className="text-xs text-center text-gray-500 py-2">
                {t("agent_details.no_packages_matching", {
                  pattern: upgradePackageName,
                })}
              </p>
            )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 bg-gray-50 dark:bg-gray-800/50 flex justify-end gap-3 shrink-0 border-t border-gray-100 dark:border-gray-800">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-md hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
          >
            {t("components.confirmation_modal.cancel")}
          </button>
          <button
            onClick={() => upgradePackageMutation.mutate(upgradePackageName)}
            disabled={
              !upgradePackageName.trim() || upgradePackageMutation.isPending
            }
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2 transition-colors"
          >
            {upgradePackageMutation.isPending && (
              <Loader2 className="w-4 h-4 animate-spin" />
            )}
            {t("agent_details.send_command")}
          </button>
        </div>
      </div>
    </div>
  );
};
