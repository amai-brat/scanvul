import { useState } from "react";
import { useParams } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "../../components/Card";
import {
  Loader2,
  XCircle,
  Play,
  DownloadCloud,
} from "lucide-react";
import { agentsApi } from "../../api/agentsApi";
import { ComputerInfoBlock } from "./components/ComputerInfoBlock";
import { VulnerablePackagesBlock } from "./components/VulnerablePackagesBlock";
import { InstalledPackagesBlock } from "./components/InstalledPackagesBlock";
import { CommandHistoryBlock } from "./components/CommandHistoryBlock";
import { ReportPackagesCommand } from "./components/commands/ReportPackagesCommand";
import { DisableAgentCommand } from "./components/commands/DisableAgentCommand";


export const AgentDetail = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  // UI State
  const [isCommandsOpen, setIsCommandsOpen] = useState(false);

  // Modals State
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [upgradePackageName, setUpgradePackageName] = useState("");

  const { data: agentsData } = useQuery({
    queryKey: ["agents"],
    queryFn: agentsApi.list,
  });
  const agent = agentsData?.agents.find((a) => a.id === Number(id));

  // Mutations
  const upgradePackageMutation = useMutation({
    mutationFn: (pkgName: string) => agentsApi.sendUpgradePackage(id!, pkgName),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", id] });
      setShowUpgradeModal(false);
      setUpgradePackageName("");
    },
  });
  

  if (!agent)
    return <div className="p-8 text-center">404 - Not Found</div>;

  return (
    <div className="space-y-6">
      <div className="mb-6">
        <h2 className="text-3xl font-bold flex items-center gap-3">
          {agent.computerName ?? "Unknown Computer"}
          {!agent.isActive && (
            <span className="px-2 py-1 text-xs bg-red-100 text-red-700 rounded-md border border-red-200">
              Inactive
            </span>
          )}
        </h2>
        <p className="text-gray-500 dark:text-gray-400">
          ID: {agent.id} • {agent.ipAddress}
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 auto-rows-min">
        <ComputerInfoBlock agent={agent} />
        <VulnerablePackagesBlock agentId={id!} />

        {/* 2. Actions / Commands Creation Block*/}
        <Card title="Actions" className="h-full">
          <div className="space-y-3">
            <p className="text-sm text-gray-500 mb-2">
              Send remote commands to the agent.
            </p>
            <ReportPackagesCommand agent={agent} isCommandsOpen={isCommandsOpen} />

            {/* Upgrade Package */}
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

            <DisableAgentCommand agent={agent} isCommandsOpen={isCommandsOpen} />
          </div>
        </Card>

        <CommandHistoryBlock agentId={id!} isCommandsOpen={isCommandsOpen} setIsCommandsOpen={setIsCommandsOpen} />
        <InstalledPackagesBlock agentId={id!} />
      </div>

      {/* --- MODALS --- */}
      {/* 3. Upgrade Package Modal (Inline Implementation for simplicity) */}
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
    </div>
  );
};
