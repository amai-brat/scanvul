import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { agentsApi } from "../api/endpoints";
import { useNavigate } from "react-router-dom";
import { Monitor, Ban, AlertTriangle } from "lucide-react";
import { Card } from "../components/Card";
import { ConnectivityIndicator } from "../components/ConnectivityIndicator";

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

  const handleConfirmDisable = () => {
    if (selectedAgentId) {
      disableMutation.mutate(selectedAgentId);
    }
  };

  const handleCancel = () => {
    setSelectedAgentId(null);
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

      {/* Confirmation Modal */}
      {selectedAgentId && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-gray-900 rounded-lg shadow-xl max-w-md w-full border border-gray-200 dark:border-gray-800 animate-in fade-in zoom-in duration-200">
            <div className="p-6">
              <div className="flex items-center gap-4 mb-4">
                <div className="p-3 bg-red-100 dark:bg-red-900/30 rounded-full text-red-600">
                  <AlertTriangle className="h-6 w-6" />
                </div>
                <h3 className="text-xl font-bold">Disable agent?</h3>
              </div>

              <p className="text-gray-600 dark:text-gray-300 mb-6">
                Are you sure you want to disable{" "}
                <span className="font-semibold text-gray-900 dark:text-white">
                  {selectedAgent?.computerName ?? "Unknown Host"}
                </span>
                ? This will prevent the agent from communicating with the
                server.
              </p>

              <div className="flex justify-end -3">
                <button
                  onClick={handleCancel}
                  disabled={disableMutation.isPending}
                  className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 dark:bg-gray-800 dark:text-gray-300 dark:border-gray-700 dark:hover:bg-gray-700"
                >
                  Cancel
                </button>
                <button
                  onClick={handleConfirmDisable}
                  disabled={disableMutation.isPending}
                  className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700 focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                >
                  {disableMutation.isPending
                    ? "Disabling..."
                    : "Yes, Disable agent"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
