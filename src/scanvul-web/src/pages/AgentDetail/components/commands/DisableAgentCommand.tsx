import { useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { ConfirmationModal } from "../../../../components/ConfimationModal";
import { useState } from "react";
import { Play, Power } from "lucide-react";


export const DisableAgentCommand = ({agent, isCommandsOpen} : {agent: AgentResponse, isCommandsOpen: boolean}) => {
  const queryClient = useQueryClient();
  const [showDisableConfirm, setShowDisableConfirm] = useState(false);
  
  const disableAgentMutation = useMutation({
    mutationFn: () => agentsApi.disableAgent(agent.id.toString()),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["agents"] });
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", agent.id] });
      setShowDisableConfirm(false);
    },
  });

  return (
    <>
      <ConfirmationModal
        isOpen={showDisableConfirm}
        onClose={() => setShowDisableConfirm(false)}
        onConfirm={() => disableAgentMutation.mutate()}
        title="Disable Agent?"
        confirmLabel="Yes, Disable Agent"
        isLoading={disableAgentMutation.isPending}
        message={
          <div className="space-y-2">
            <p>
              Are you sure you want to disable <b>{agent.computerName}</b>?
            </p>
            <p className="text-sm text-red-600 bg-red-50 p-3 rounded border border-red-200">
              ⚠️ The agent will be stopped and removed from services. This
              action may interrupt active scans.
            </p>
          </div>
        }
      />
      <button
        disabled={disableAgentMutation.isPending || !agent.isActive}
        className="w-full flex items-center justify-between p-3 rounded-lg border border-red-200 dark:border-red-900/50 hover:bg-red-50 dark:hover:bg-red-900/20 transition-colors disabled:opacity-50"
      >
        <div className="flex items-center gap-3">
          <div className="bg-red-50 text-red-600 p-2 rounded-md">
            <Power className="w-4 h-4" />
          </div>
          <div className="text-left">
            <div className="text-sm font-semibold text-red-700 dark:text-red-400">
              Disable Agent
            </div>
            <div className="text-xs text-red-500/80">
              Stop and remove from services
            </div>
          </div>
        </div>
        <Play
          className="w-4 h-4 text-red-400 cursor-pointer"
          onClick={() => setShowDisableConfirm(true)}
        />
      </button>
    </>
  );
}