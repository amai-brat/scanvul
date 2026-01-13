import { Activity, Cpu, MemoryStick } from "lucide-react";
import { Card } from "../../../components/Card";
import { InfoRow } from "./InfoRow";
import type { AgentResponse } from "../../../api/agentsApi";
import { useTranslation } from "react-i18next";


export const ComputerInfoBlock = ({ agent }: 
  {
    agent: AgentResponse
  }) => {
    const { t } = useTranslation();

    return (
      <Card title={t("agent_details.computer_info")} className="h-full">
        <div className="space-y-4">
          <InfoRow
            icon={<Activity />}
            label={t("agent_details.os")}
            value={agent.operatingSystem}
          />
          <InfoRow
            icon={<Cpu />}
            label={t("agent_details.cpu")}
            value={agent.cpuName ?? "N/A"}
          />
          <InfoRow
            icon={<MemoryStick />}
            label={t("agent_details.memory")}
            value={
              agent.memoryInMb
                ? `${(agent.memoryInMb / 1024).toFixed(1)} GB`
                : "N/A"
            }
          />
          <div className="pt-4 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-500">
            {t("agent_details.last_ping", {
              time: new Date(agent.lastPingAt).toLocaleString(),
            })}
          </div>
        </div>
      </Card>
    );
  };