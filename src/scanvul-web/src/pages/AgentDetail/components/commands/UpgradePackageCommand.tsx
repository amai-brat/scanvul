// UpgradePackageCommand.tsx
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { useMemo, useState } from "react";
import {
  DownloadCloud,
  Loader2,
  Play,
  XCircle,
  Search,
  AlertTriangle,
  Package,
  ExternalLink,
} from "lucide-react";
import { packageManagerApi, type PackageMetadata } from "../../../../api/packageManagerApi";
import { getPackageManager } from "../../../../utils/packageManager";


export const UpgradePackageCommand = ({
  agent,
  isCommandsOpen,
}: {
  agent: AgentResponse;
  isCommandsOpen: boolean;
}) => {
  const queryClient = useQueryClient();
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [upgradePackageName, setUpgradePackageName] = useState("");
  const [searchResults, setSearchResults] = useState<PackageMetadata[]>([]);

  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns", agent.id],
    queryFn: () => agentsApi.getVulnPackages(agent.id.toString()),
  });

  const uniqueVulnPackages = useMemo(() => {
    if (!vulnData?.packages) return [];
    return Array.from(new Set(vulnData.packages.map((p) => p.packageName))).sort();
  }, [vulnData]);

  const searchPackageMutation = useMutation({
    mutationFn: async () => {
      const pm = getPackageManager(agent.operatingSystem || "unknown");
      return packageManagerApi.search(upgradePackageName, pm);
    },
    onSuccess: (data) => {
      setSearchResults(data.packages);
    },
    onError: (err) => {
      console.error("Failed to search packages", err);
    },
  });

  const upgradePackageMutation = useMutation({
    mutationFn: (pkgName: string) =>
      agentsApi.sendUpgradePackage(agent.id.toString(), pkgName),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", agent.id.toString()] });
      setShowUpgradeModal(false);
      setUpgradePackageName("");
      setSearchResults([]);
    },
  });

  const handleVulnClick = (name: string) => {
    setUpgradePackageName(name);
    setTimeout(() => searchPackageMutation.mutate(), 0);
  };

  const handleResultClick = (name: string) => {
    setUpgradePackageName(name);
  };

  return (
    <>
      <button
        disabled={!agent.isActive}
        className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
      >
        <div className="flex items-center gap-3">
          <div className="bg-emerald-50 text-emerald-600 p-2 rounded-md">
            <DownloadCloud className="w-4 h-4" />
          </div>
          <div className="text-left">
            <div className="text-sm font-semibold">Upgrade Package</div>
            <div className="text-xs text-gray-500">
              Update specific software
            </div>
          </div>
        </div>
        <Play
          className="w-4 h-4 text-gray-400 cursor-pointer"
          onClick={() => setShowUpgradeModal(true)}
        />
      </button>

      {showUpgradeModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-in fade-in">
          <div className="bg-white dark:bg-gray-900 rounded-lg shadow-xl w-full max-w-lg border border-gray-200 dark:border-gray-800 overflow-hidden flex flex-col max-h-[90vh]">
            {/* Header */}
            <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex justify-between items-center shrink-0">
              <h3 className="font-semibold text-lg">Upgrade Package</h3>
              <button onClick={() => setShowUpgradeModal(false)}>
                <XCircle className="w-5 h-5 text-gray-400 hover:text-gray-600" />
              </button>
            </div>

            {/* Scrollable Content */}
            <div className="p-6 space-y-6 overflow-y-auto">
              <p className="text-sm text-gray-600 dark:text-gray-300">
                Search for a package to upgrade or select from detected
                vulnerabilities.
              </p>

              {/* 1. Vulnerable Packages Section */}
              {!vulnLoading && uniqueVulnPackages.length > 0 && (
                <div className="space-y-2">
                  <div className="flex items-center gap-2 text-xs font-semibold text-amber-600 dark:text-amber-500">
                    <AlertTriangle className="w-3 h-3" />
                    <span>Detected Vulnerable Packages</span>
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

              {/* 2. Search Input */}
              <div className="space-y-2">
                <label className="block text-xs font-medium text-gray-700 dark:text-gray-300">
                  Package Name
                </label>
                <div className="flex gap-2">
                  <div className="relative flex-1">
                    <input
                      type="text"
                      value={upgradePackageName}
                      onChange={(e) => setUpgradePackageName(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === "Enter") searchPackageMutation.mutate();
                      }}
                      placeholder="e.g. 7zip"
                      className="w-full pl-3 pr-3 py-2 border rounded-md dark:bg-gray-800 dark:border-gray-700 focus:ring-2 focus:ring-blue-500 outline-none"
                    />
                  </div>
                  <button
                    onClick={() => searchPackageMutation.mutate()}
                    disabled={
                      !upgradePackageName.trim() ||
                      searchPackageMutation.isPending
                    }
                    className="px-3 py-2 bg-gray-100 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-md hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors disabled:opacity-50"
                    title="Search Package Manager"
                  >
                    {searchPackageMutation.isPending ? (
                      <Loader2 className="w-4 h-4 animate-spin text-gray-600" />
                    ) : (
                      <Search className="w-4 h-4 text-gray-600 dark:text-gray-300" />
                    )}
                  </button>
                </div>
              </div>

              {/* 3. Search Results */}
              {searchResults.length > 0 && (
                <div className="space-y-2">
                  <h4 className="text-xs font-semibold text-gray-500 uppercase tracking-wider">
                    Search Results
                  </h4>
                  <div className="space-y-2 max-h-60 pr-1">
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
                    No packages found matching "{upgradePackageName}"
                  </p>
                )}
            </div>

            {/* Footer */}
            <div className="px-6 py-4 bg-gray-50 dark:bg-gray-800/50 flex justify-end gap-3 shrink-0 border-t border-gray-100 dark:border-gray-800">
              <button
                onClick={() => setShowUpgradeModal(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={() =>
                  upgradePackageMutation.mutate(upgradePackageName)
                }
                disabled={
                  !upgradePackageName.trim() || upgradePackageMutation.isPending
                }
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
              >
                {upgradePackageMutation.isPending && (
                  <Loader2 className="w-4 h-4 animate-spin" />
                )}
                Send Command
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};
