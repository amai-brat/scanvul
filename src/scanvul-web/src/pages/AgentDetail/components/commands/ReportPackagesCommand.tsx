import { useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { Loader2, Play, RefreshCw } from "lucide-react";


export const ReportPackagesCommand = ({agent, isCommandsOpen} : {agent: AgentResponse, isCommandsOpen: boolean}) => {
  const queryClient = useQueryClient();

  const reportPackagesMutation = useMutation({
    mutationFn: () => agentsApi.sendReportPackages(agent.id.toString()),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", agent.id] });
      // TODO: Add toast notification "Scan command sent"
    },
  });

  return (
    <button
      disabled={reportPackagesMutation.isPending || !agent.isActive}
      className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
    >
      <div className="flex items-center gap-3">
        <div className="bg-blue-50 text-blue-600 p-2 rounded-md">
          {reportPackagesMutation.isPending ? (
            <Loader2 className="w-4 h-4 animate-spin" />
          ) : (
            <RefreshCw className="w-4 h-4" />
          )}
        </div>
        <div className="text-left">
          <div className="text-sm font-semibold">Scan Packages</div>
          <div className="text-xs text-gray-500">
            Request package list update
          </div>
        </div>
      </div>
      <Play
        className="w-4 h-4 text-gray-400 cursor-pointer"
        onClick={() => reportPackagesMutation.mutate()}
      />
    </button>
  );
}