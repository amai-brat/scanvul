import { Activity, Cpu, MemoryStick } from "lucide-react";
import { Card } from "../../../components/Card";
import { InfoRow } from "./InfoRow";
import type { AgentResponse } from "../../../api/agentsApi";


export const ComputerInfoBlock = ({ agent }: 
  {
    agent: AgentResponse
  }) => (
  <Card title="Computer Info" className="h-full">
    <div className="space-y-4">
    <InfoRow icon={<Activity />} label="OS" value={agent.operatingSystem} />
    <InfoRow icon={<Cpu />} label="CPU" value={agent.cpuName ?? "N/A"} />
    <InfoRow
      icon={<MemoryStick />}
      label="Memory"
      value={
      agent.memoryInMb
          ? `${(agent.memoryInMb / 1024).toFixed(1)} GB`
          : "N/A"
      }
    />
    <div className="pt-4 border-t border-gray-100 dark:border-gray-800 text-xs text-gray-500">
        Last Ping: {new Date(agent.lastPingAt).toLocaleString()}
    </div>
    </div>
  </Card>
);