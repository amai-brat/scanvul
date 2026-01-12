import { useState } from "react";
import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Card } from "../../components/Card";
import { agentsApi } from "../../api/agentsApi";
import { ComputerInfoBlock } from "./components/ComputerInfoBlock";
import { VulnerablePackagesBlock } from "./components/VulnerablePackagesBlock";
import { InstalledPackagesBlock } from "./components/InstalledPackagesBlock";
import { CommandHistoryBlock } from "./components/CommandHistoryBlock";
import { ReportPackagesCommand } from "./components/commands/ReportPackagesCommand";
import { DisableAgentCommand } from "./components/commands/DisableAgentCommand";
import { UpgradePackageCommand } from "./components/commands/UpgradePackageCommand";


export const AgentDetail = () => {
  const { id } = useParams<{ id: string }>();

  const [isCommandsOpen, setIsCommandsOpen] = useState(false);

  const { data: agentsData } = useQuery({
    queryKey: ["agents"],
    queryFn: agentsApi.list,
  });
  const agent = agentsData?.agents.find((a) => a.id === Number(id));
  
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

        <Card title="Actions" className="h-full">
          <div className="space-y-3">
            <p className="text-sm text-gray-500 mb-2">
              Send remote commands to the agent.
            </p>
            <ReportPackagesCommand agent={agent} isCommandsOpen={isCommandsOpen} />
            <UpgradePackageCommand agent={agent} isCommandsOpen={isCommandsOpen} />
            <DisableAgentCommand agent={agent} isCommandsOpen={isCommandsOpen} />
          </div>
        </Card>

        <CommandHistoryBlock agentId={id!} isCommandsOpen={isCommandsOpen} setIsCommandsOpen={setIsCommandsOpen} />
        <InstalledPackagesBlock agentId={id!} />
      </div>
    </div>
  );
};
