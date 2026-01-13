import { useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi, type AgentResponse } from "../../../../api/agentsApi";
import { ConfirmationModal } from "../../../../components/ConfimationModal";
import { useState } from "react";
import { Play, Power } from "lucide-react";
import { Trans, useTranslation } from "react-i18next";


export const DisableAgentCommand = ({agent, isCommandsOpen} : {agent: AgentResponse, isCommandsOpen: boolean}) => {
  const queryClient = useQueryClient();
  const { t } = useTranslation();
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
        title={t("agents.disable_agent_title")}
        confirmLabel={t("agents.disable_agent_confirm")}
        isLoading={disableAgentMutation.isPending}
        message={
          <p>
            <Trans
              i18nKey="agents.disable_agent_message"
              values={{
                host: agent?.computerName ?? t("agents.unkown_host"),
              }}
              components={{
                bold: (
                  <span className="font-semibold text-gray-900 dark:text-white" />
                ),
              }}
            />
          </p>
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
              {t("agents.disable_agent_title")}
            </div>
            <div className="text-xs text-red-500/80">
              {t("agents.disable_agent_desc")}
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