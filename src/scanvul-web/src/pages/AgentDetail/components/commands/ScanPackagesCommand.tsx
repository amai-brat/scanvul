import { useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { Loader2, Play, Search } from "lucide-react";
import { useTranslation } from "react-i18next";
import { toast } from "react-toastify";

export const ScanPackagesCommand = ({
  agent,
  isCommandsOpen,
}: {
  agent: AgentResponse;
  isCommandsOpen: boolean;
}) => {
  const queryClient = useQueryClient();
  const { t } = useTranslation();

  const scanPackagesMutation = useMutation({
    mutationFn: () => agentsApi.scanPackages(agent.id.toString()),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({
          queryKey: ["commands", agent.id.toString()],
        });
      toast.info(t("agent_details.command_scan_packages_toast_msg"));
    },
  });

  return (
    <button
      disabled={scanPackagesMutation.isPending || !agent.isActive}
      className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
    >
      <div className="flex items-center gap-3 max-w-[90%]">
        <div className="bg-blue-50 text-blue-600 p-2 rounded-md">
          {scanPackagesMutation.isPending ? (
            <Loader2 className="w-4 h-4 animate-spin" />
          ) : (
            <Search className="w-4 h-4" />
          )}
        </div>
        <div className="text-left">
          <div className="text-sm font-semibold">
            {t("agent_details.command_scan_packages_title")}
          </div>
          <div className="text-xs text-gray-500">
            {t("agent_details.command_scan_packages_desc")}
          </div>
        </div>
      </div>
      <Play
        className="w-4 h-4 text-gray-400 cursor-pointer"
        onClick={() => scanPackagesMutation.mutate()}
      />
    </button>
  );
};
