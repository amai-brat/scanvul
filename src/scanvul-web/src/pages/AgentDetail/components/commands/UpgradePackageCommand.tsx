import { useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { useState } from "react";
import { DownloadCloud, Loader2, Play, XCircle } from "lucide-react";

export const UpgradePackageCommand = ({agent, isCommandsOpen} : {agent: AgentResponse, isCommandsOpen: boolean}) => {
  const queryClient = useQueryClient();
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [upgradePackageName, setUpgradePackageName] = useState("");

  const upgradePackageMutation = useMutation({
    mutationFn: (pkgName: string) => agentsApi.sendUpgradePackage(agent.id.toString(), pkgName),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", agent.id] });
      setShowUpgradeModal(false);
      setUpgradePackageName("");
    },
  });

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
          <div className="bg-white dark:bg-gray-900 rounded-lg shadow-xl w-full max-w-md border border-gray-200 dark:border-gray-800 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex justify-between items-center">
              <h3 className="font-semibold text-lg">Upgrade Package</h3>
              <button onClick={() => setShowUpgradeModal(false)}>
                <XCircle className="w-5 h-5 text-gray-400 hover:text-gray-600" />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <p className="text-sm text-gray-600 dark:text-gray-300">
                Enter the exact name of the package you wish to upgrade via the
                package manager.
              </p>
              <div>
                <label className="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Package Name
                </label>
                <input
                  type="text"
                  value={upgradePackageName}
                  onChange={(e) => setUpgradePackageName(e.target.value)}
                  placeholder="e.g. 7zip"
                  className="w-full px-3 py-2 border rounded-md dark:bg-gray-800 dark:border-gray-700 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
            </div>
            <div className="px-6 py-4 bg-gray-50 dark:bg-gray-800/50 flex justify-end gap-3">
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
}