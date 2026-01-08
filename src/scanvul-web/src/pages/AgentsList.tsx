import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi } from "../api/endpoints";
import { useNavigate } from "react-router-dom";
import { Monitor, Ban } from "lucide-react";
import { Card } from "../components/Card";
import { ConnectivityIndicator } from "../components/ConnectivityIndicator";
import { ConfirmationModal } from "../components/ConfimationModal";

export const AgentsList = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  // State for the confirmation modal
  const [selectedAgentId, setSelectedAgentId] = useState<number | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ["agents"],
    queryFn: agentsApi.list,
    refetchInterval: 30000,
  });

  // Mutation to disable the agent
  const disableMutation = useMutation({
    mutationFn: (id: number) => agentsApi.disableAgent(`${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["agents"] });
      setSelectedAgentId(null);
    },
    onError: (err) => {
      console.error("Failed to disable agent:", err);
    },
  });

  const handleDisableClick = (e: React.MouseEvent, agentId: number) => {
    e.stopPropagation(); // Prevent the parent card click (navigation)
    setSelectedAgentId(agentId);
  };

  if (isLoading)
    return <div className="p-8 text-center">Loading agents...</div>;
  if (error)
    return (
      <div className="p-8 text-center text-red-500">Error loading agents</div>
    );

  const selectedAgent = data?.agents.find((a) => a.id === selectedAgentId);

  return (
    <div className="space-y-6 relative">
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
            {/* Left Side: Icon and Info */}
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

            {/* Right Side: Connectivity and Disable Button */}
            <div className="flex items-center gap-6">
              <div className="flex flex-col items-end gap-1">
                <ConnectivityIndicator lastPingAt={agent.lastPingAt} />
              </div>

              <button
                onClick={(e) => handleDisableClick(e, agent.id)}
                className="p-2 text-gray-400 hover:text-red-500 hover:bg-red-50 dark:hover:bg-red-900/20 rounded-full transition-colors"
                title="Disable agent"
              >
                <Ban className="h-5 w-5" />
              </button>
            </div>
          </div>
        ))}

        {data?.agents.length === 0 && (
          <Card title="No Agents" className="text-center py-10">
            <p className="text-gray-500">No agents have registered yet.</p>
          </Card>
        )}
      </div>

      <ConfirmationModal
        isOpen={!!selectedAgentId}
        onClose={() => setSelectedAgentId(null)}
        onConfirm={() =>
          selectedAgentId && disableMutation.mutate(selectedAgentId)
        }
        title="Disable agent?"
        confirmLabel="Yes, Disable agent"
        isLoading={disableMutation.isPending}
        message={
          <p>
            Are you sure you want to disable{" "}
            <span className="font-semibold text-gray-900 dark:text-white">
              {selectedAgent?.computerName ?? "Unknown Host"}
            </span>
            ? This will prevent the agent from communicating with the server.
          </p>
        }
      />
    </div>
  );
};
