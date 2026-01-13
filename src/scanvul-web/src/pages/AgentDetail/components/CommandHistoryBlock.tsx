import { useQuery } from "@tanstack/react-query";
import { agentsApi } from "../../../api/agentsApi";
import { ChevronDown, ChevronUp, Clock, ExternalLink, Loader2, Terminal } from "lucide-react";
import { useTranslation } from "react-i18next";

export const CommandHistoryBlock = ({ agentId, isCommandsOpen, setIsCommandsOpen }: { 
  agentId: string, 
  isCommandsOpen: boolean, 
  setIsCommandsOpen: (isOpen: boolean) => void 
}) => {
  const { t } = useTranslation();
  const { data: commandsData, isLoading: commandsLoading } = useQuery({
    queryKey: ["commands", agentId],
    queryFn: () => agentsApi.getCommands(agentId!),
    enabled: !!agentId && isCommandsOpen,
  });

  return (
    <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
      <button
        onClick={() => setIsCommandsOpen(!isCommandsOpen)}
        className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
      >
        <div className="space-y-1">
          <h3 className="font-bold text-lg flex items-center gap-2">
            <Terminal className="w-5 h-5 text-gray-500" />
            {t("agent_details.command_history")}
          </h3>
          <p className="text-sm text-gray-500">
            {commandsData
              ? t("agent_details.command_history_total", {
                  amount: commandsData.commands.length,
                })
              : t("agent_details.command_history_click")}
          </p>
        </div>
        {isCommandsOpen ? (
          <ChevronUp className="w-5 h-5 text-gray-400" />
        ) : (
          <ChevronDown className="w-5 h-5 text-gray-400" />
        )}
      </button>

      {isCommandsOpen && (
        <div className="border-t border-gray-100 dark:border-gray-800">
          {commandsLoading ? (
            <div className="p-8 flex justify-center items-center text-gray-500 gap-2">
              <Loader2 className="w-5 h-5 animate-spin" />
              <span>{t("agent_details.command_history_fetching")}</span>
            </div>
          ) : (
            <div className="overflow-x-auto max-h-96 overflow-y-auto">
              <table className="w-full text-sm text-left">
                <thead className="text-xs uppercase bg-gray-50 dark:bg-gray-800/50 text-gray-500 sticky top-0 backdrop-blur-sm">
                  <tr>
                    <th className="px-6 py-3">
                      {t("agent_details.command_type")}
                    </th>
                    <th className="px-6 py-3">
                      {t("agent_details.command_status")}
                    </th>
                    <th className="px-6 py-3">
                      {t("agent_details.command_parameters")}
                    </th>
                    <th className="px-6 py-3 text-right">
                      {t("agent_details.command_timestamps")}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {commandsData?.commands.map((cmd) => (
                    <tr
                      key={cmd.id}
                      className="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30"
                    >
                      <td className="px-6 py-4 font-medium">
                        <span className="bg-slate-100 dark:bg-slate-800 px-2 py-1 rounded text-xs font-mono border border-slate-200 dark:border-slate-700">
                          {cmd.type}
                        </span>
                      </td>
                      <td className="px-6 py-4 max-w-50">
                        {cmd.agentResponse ? (
                          <div className="flex flex-col gap-1.5">
                            <div className="flex items-center gap-2 text-gray-600 dark:text-gray-300">
                              <span
                                className="font-mono text-xs truncate"
                                title={cmd.agentResponse}
                              >
                                {cmd.agentResponse}
                              </span>
                            </div>
                            <button
                              onClick={() => {
                                const blob = new Blob([cmd.agentResponse!], {
                                  type: "text/plain",
                                });
                                const url = URL.createObjectURL(blob);
                                window.open(url, "_blank");
                              }}
                              className="text-xs text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 flex items-center gap-1 font-medium cursor-pointer"
                            >
                              <ExternalLink className="w-3 h-3" />
                              {t("agent_details.command_full_resp")}
                            </button>
                          </div>
                        ) : cmd.sentAt ? (
                          <div className="flex items-center gap-2 text-blue-600">
                            <Clock className="w-4 h-4" />
                            <span>{t("agent_details.command_sent")}</span>
                          </div>
                        ) : (
                          <div className="flex items-center gap-2 text-gray-400">
                            <Loader2 className="w-4 h-4 animate-spin" />
                            <span>{t("agent_details.command_pending")}</span>
                          </div>
                        )}
                      </td>
                      <td className="px-6 py-4 text-gray-500 font-mono text-xs">
                        {cmd.commandParams
                          ? JSON.stringify(cmd.commandParams)
                          : "-"}
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="text-xs text-gray-900 dark:text-gray-100">
                          {t("agent_details.command_created_at", {
                            time: new Date(cmd.createdAt).toLocaleString(),
                          })}
                        </div>
                        {cmd.sentAt && (
                          <div className="text-[10px] text-gray-400">
                            {t("agent_details.command_sent_at", {
                              time: new Date(cmd.sentAt).toLocaleString(),
                            })}
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                  {commandsData?.commands.length === 0 && (
                    <tr>
                      <td
                        colSpan={4}
                        className="px-6 py-8 text-center text-gray-400"
                      >
                        {t("agent_details.no_commands")}
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
};