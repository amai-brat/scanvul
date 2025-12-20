import { useQuery } from "@tanstack/react-query";
import { agentsApi } from "../api/endpoints";
import { useNavigate } from "react-router-dom";
import { Monitor } from "lucide-react";
import { Card } from "../components/Card";
import { ConnectivityIndicator } from "../components/ConnectivityIndicator";

export const AgentsList = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ["agents"],
    queryFn: agentsApi.list,
    refetchInterval: 30000, // Refresh every 30s to update wifi status
  });
  const navigate = useNavigate();

  if (isLoading)
    return <div className="p-8 text-center">Loading agents...</div>;
  if (error)
    return (
      <div className="p-8 text-center text-red-500">Error loading agents</div>
    );

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-3xl font-bold">Agents</h2>
        <span className="bg-primary/10 text-primary px-3 py-1 rounded-full text-sm font-medium">
          Total: {data?.agents.length ?? 0}
        </span>
      </div>

      <div className="grid gap-4">
        {data?.agents.map((agent) => (
          <div
            key={agent.id}
            onClick={() => navigate(`/agents/${agent.id}`)}
            className="bg-card hover:border-primary/50 cursor-pointer border border-gray-200 dark:border-gray-800 rounded-lg p-6 shadow-sm transition-all flex items-center justify-between group"
          >
            <div className="flex items-center gap-4">
              <div className="p-3 bg-blue-50 dark:bg-blue-900/20 rounded-full text-primary">
                <Monitor className="h-6 w-6" />
              </div>
              <div>
                <h3 className="font-semibold text-lg group-hover:text-primary transition-colors">
                  {agent.computerName ?? "Unknown Host"}
                </h3>
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  {agent.ipAddress} • {agent.operatingSystem}
                </p>
              </div>
            </div>

            <div className="flex flex-col items-end gap-1">
              <ConnectivityIndicator lastPingAt={agent.lastPingAt} />
            </div>
          </div>
        ))}
        {data?.agents.length === 0 && (
          <Card title="No Agents" className="text-center py-10">
            <p className="text-gray-500">No agents have registered yet.</p>
          </Card>
        )}
      </div>
    </div>
  );
};
