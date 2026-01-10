import React, { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Card } from "../components/Card";
import {
  Cpu,
  MemoryStick,
  Activity,
  Package,
  ChevronDown,
  ChevronUp,
  Loader2,
  ExternalLink,
  ShieldAlert,
  Ban,
  XCircle,
  RefreshCw,
  Play,
  DownloadCloud,
  Terminal,
  Clock,
  Power,
} from "lucide-react";
import { ConfirmationModal } from "../components/ConfimationModal";
import { agentsApi, type VulnerablePackageResponse } from "../api/agentsApi";


type SeverityLevel = "CRITICAL" | "HIGH" | "MEDIUM" | "LOW";

const getCvssScore = (v: VulnerablePackageResponse): number => {
  return v.cvssV3_1 ?? v.cvssV3_0 ?? v.cvssV2_0 ?? 0;
};

const getSeverityLevel = (score: number): SeverityLevel => {
  if (score >= 9.0) return "CRITICAL";
  if (score >= 7.0) return "HIGH";
  if (score >= 4.0) return "MEDIUM";
  return "LOW";
};

const SEVERITY_CONFIG: Record<SeverityLevel, { 
  bg: string; 
  border: string; 
  text: string; 
  badge: string;
}> = {
  CRITICAL: {
    bg: "bg-rose-50 dark:bg-rose-950/20",
    border: "border-rose-200 dark:border-rose-900",
    text: "text-rose-700 dark:text-rose-300",
    badge: "bg-rose-600 text-white",
  },
  HIGH: {
    bg: "bg-orange-50 dark:bg-orange-950/20",
    border: "border-orange-200 dark:border-orange-900",
    text: "text-orange-700 dark:text-orange-300",
    badge: "bg-orange-500 text-white",
  },
  MEDIUM: {
    bg: "bg-yellow-50 dark:bg-yellow-950/20",
    border: "border-yellow-200 dark:border-yellow-900",
    text: "text-yellow-700 dark:text-yellow-300",
    badge: "bg-yellow-500 text-white",
  },
  LOW: {
    bg: "bg-slate-50 dark:bg-slate-900/40",
    border: "border-slate-200 dark:border-slate-800",
    text: "text-slate-600 dark:text-slate-400",
    badge: "bg-slate-500 text-white",
  },
};

const InfoRow = ({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
}) => (
  <div className="flex items-center gap-3">
    <div className="text-gray-400 w-5 flex justify-center">{icon}</div>
    <div>
      <p className="text-xs text-gray-500 uppercase font-semibold">{label}</p>
      <p className="font-medium text-sm">{value}</p>
    </div>
  </div>
);

const ShieldCheckIcon = (props: React.SVGProps<SVGSVGElement>) => {
  return (
    <svg
      {...props}
      xmlns="http://www.w3.org/2000/svg"
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10" />
      <path d="m9 12 2 2 4-4" />
    </svg>
  );
}

export const AgentDetail = () => {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();

  // UI State
  const [isPackagesOpen, setIsPackagesOpen] = useState(false);
  const [isCommandsOpen, setIsCommandsOpen] = useState(false);
  const [expandedCveId, setExpandedCveId] = useState<number | null>(null);
  const [expandedPackageId, setExpandedPackageId] = useState<number | null>(
    null
  );

  // Modals State
  const [vulnIdToMark, setVulnIdToMark] = useState<number | null>(null);
  const [showDisableConfirm, setShowDisableConfirm] = useState(false);
  const [showUpgradeModal, setShowUpgradeModal] = useState(false);
  const [upgradePackageName, setUpgradePackageName] = useState("");

  // 1. Fetch Agent Info
  const { data: agentsData } = useQuery({
    queryKey: ["agents"],
    queryFn: agentsApi.list,
  });
  const agent = agentsData?.agents.find((a) => a.id === Number(id));

  // 2. Fetch Packages
  const { data: pkgData, isLoading: pkgLoading } = useQuery({
    queryKey: ["packages", id],
    queryFn: () => agentsApi.getPackages(id!),
    enabled: !!id && isPackagesOpen,
  });

  // 3. Fetch Vulnerabilities
  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns", id],
    queryFn: () => agentsApi.getVulnPackages(id!),
    enabled: !!id,
  });

  // 4. Fetch Commands
  const { data: commandsData, isLoading: commandsLoading } = useQuery({
    queryKey: ["commands", id],
    queryFn: () => agentsApi.getCommands(id!),
    enabled: !!id && isCommandsOpen,
  });

  // Mutations
  const markFalsePositiveMutation = useMutation({
    mutationFn: (vulnerablePackageId: number) =>
      agentsApi.markFalsePositive(vulnerablePackageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vulns", id] });
      setVulnIdToMark(null);
    },
  });

  const reportPackagesMutation = useMutation({
    mutationFn: () => agentsApi.sendReportPackages(id!),
    onSuccess: () => {
      // Refresh command list to show the new pending command
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", id] });
      // Optional: Add toast notification "Scan command sent"
    },
  });

  const disableAgentMutation = useMutation({
    mutationFn: () => agentsApi.disableAgent(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["agents"] });
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", id] });
      setShowDisableConfirm(false);
    },
  });

  const upgradePackageMutation = useMutation({
    mutationFn: (pkgName: string) => agentsApi.sendUpgradePackage(id!, pkgName),
    onSuccess: () => {
      if (isCommandsOpen)
        queryClient.invalidateQueries({ queryKey: ["commands", id] });
      setShowUpgradeModal(false);
      setUpgradePackageName("");
    },
  });

  // 5. Data Processing: Group by Package -> Then by Severity -> Sort Packages by Max Score
  const organizedVulns = useMemo(() => {
    if (!vulnData?.packages) return [];

    const groups = new Map<
      number,
      {
        packageId: number;
        name: string;
        version: string;
        maxScore: number;
        buckets: Record<SeverityLevel, VulnerablePackageResponse[]>;
      }
    >();

    vulnData.packages.forEach((v) => {
      const score = getCvssScore(v);
      const severity = getSeverityLevel(score);

      if (!groups.has(v.packageId)) {
        groups.set(v.packageId, {
          packageId: v.packageId,
          name: v.packageName,
          version: v.packageVersion,
          maxScore: 0,
          buckets: {
            CRITICAL: [],
            HIGH: [],
            MEDIUM: [],
            LOW: [],
          },
        });
      }

      const group = groups.get(v.packageId)!;

      // Add detailed object to the specific severity bucket
      group.buckets[severity].push(v);

      // Update Max Score
      if (score > group.maxScore) {
        group.maxScore = score;
      }
    });

    // Convert Map to Array and Sort by Max Score Descending
    return Array.from(groups.values()).sort((a, b) => b.maxScore - a.maxScore);
  }, [vulnData]);

  if (!agent)
    return <div className="p-8 text-center">Loading agent info...</div>;

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
        {/* 1. Computer Info Block (Existing) */}
        <Card title="Computer Info" className="h-full">
          {/* ... Existing InfoRow content ... */}
          <div className="space-y-4">
            <InfoRow
              icon={<Activity />}
              label="OS"
              value={agent.operatingSystem}
            />
            <InfoRow
              icon={<Cpu />}
              label="CPU"
              value={agent.cpuName ?? "N/A"}
            />
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

        {/* 2. Actions / Commands Creation Block*/}
        <Card title="Actions" className="h-full">
          <div className="space-y-3">
            <p className="text-sm text-gray-500 mb-2">
              Send remote commands to the agent.
            </p>

            {/* Scan Packages */}
            <button
              disabled={reportPackagesMutation.isPending || !agent.isActive}
              className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
            >
              <div className="flex items-center gap-3">
                <div className="bg-blue-50 text-blue-600 p-2 rounded-md">
                  {reportPackagesMutation.isPending ? (
                    <Loader2 className="w-4 h-4 animate-spin" />
                  ) : (
                    <RefreshCw className="w-4 h-4" />
                  )}
                </div>
                <div className="text-left">
                  <div className="text-sm font-semibold">Scan Packages</div>
                  <div className="text-xs text-gray-500">
                    Request package list update
                  </div>
                </div>
              </div>
              <Play
                className="w-4 h-4 text-gray-400 cursor-pointer"
                onClick={() => reportPackagesMutation.mutate()}
              />
            </button>

            {/* Upgrade Package */}
            <button
              disabled={!agent.isActive}
              className="w-full flex items-center justify-between p-3 rounded-lg border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
            >
              <div className="flex items-center gap-3">
                <div className="bg-emerald-50 text-emerald-600 p-2 rounded-md">
                  <DownloadCloud className="w-4 h-4" />
                </div>
                <div className="text-left">
                  <div className="text-sm font-semibold">Upgrade Package</div>
                  <div className="text-xs text-gray-500">
                    Update specific software
                  </div>
                </div>
              </div>
              <Play
                className="w-4 h-4 text-gray-400 cursor-pointer"
                onClick={() => setShowUpgradeModal(true)}
              />
            </button>

            {/* Disable Agent */}
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
          </div>
        </Card>

        {/* 3. Vulnerable Packages Block*/}
        <Card
          title={`Security Findings (${vulnData?.packages.length ?? 0})`}
          className="md:col-span-2 lg:col-span-1 h-100 flex flex-col"
        >
          {vulnLoading ? (
            <div className="flex justify-center items-center h-40">
              <Loader2 className="animate-spin text-gray-400" />
            </div>
          ) : (
            <div className="space-y-4 overflow-y-auto pr-2 flex-1 custom-scrollbar">
              {organizedVulns.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-green-500">
                  <ShieldCheckIcon className="w-12 h-12 mb-2" />
                  <p>No vulnerabilities found.</p>
                </div>
              ) : (
                organizedVulns.map((pkg) => (
                  // Package Container
                  <div
                    key={pkg.packageId}
                    className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white dark:bg-gray-800"
                  >
                    {/* Package Header */}
                    <div
                      className="bg-gray-50 dark:bg-gray-900/50 p-3 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center cursor-pointer"
                      onClick={() =>
                        setExpandedPackageId((prev) =>
                          prev === null ? pkg.packageId : null
                        )
                      }
                    >
                      <div className="flex items-center gap-2">
                        <Package className="w-4 h-4 text-gray-400" />
                        <div>
                          <span className="font-bold text-gray-700 dark:text-gray-200 mr-2">
                            {pkg.name}
                          </span>
                          <span className="text-xs font-mono text-gray-500">
                            v{pkg.version}
                          </span>
                        </div>
                      </div>
                      <span className="text-xs font-bold text-gray-400">
                        Max CVSS: {pkg.maxScore.toFixed(1)}
                      </span>
                    </div>

                    {/* BLOCK 2: Severity Intervals */}
                    {expandedPackageId === pkg.packageId && (
                      <div className="p-3 space-y-3">
                        {(
                          [
                            "CRITICAL",
                            "HIGH",
                            "MEDIUM",
                            "LOW",
                          ] as SeverityLevel[]
                        ).map((severity) => {
                          const cves = pkg.buckets[severity];
                          if (cves.length === 0) return null;

                          const style = SEVERITY_CONFIG[severity];

                          return (
                            <div key={severity} className="space-y-1">
                              <h5
                                className={`text-[10px] font-bold tracking-wider ${style.text} mb-1 flex items-center gap-1`}
                              >
                                <div
                                  className={`w-1.5 h-1.5 rounded-full ${style.badge}`}
                                />
                                {severity}
                              </h5>

                              <div className="flex flex-col gap-2">
                                {cves.map((vuln) => {
                                  const isExpanded = expandedCveId === vuln.id;
                                  const score = getCvssScore(vuln);

                                  return (
                                    <div
                                      key={vuln.id}
                                      className={`
                                    border rounded-md transition-all duration-200
                                    ${
                                      isExpanded
                                        ? "shadow-md ring-1 ring-gray-200 dark:ring-gray-700"
                                        : "hover:bg-gray-50 dark:hover:bg-gray-700/30 cursor-pointer"
                                    }
                                    ${style.bg} ${style.border}
                                  `}
                                      onClick={() =>
                                        !isExpanded && setExpandedCveId(vuln.id)
                                      }
                                    >
                                      {/* CVE Header Line */}
                                      <div
                                        className="flex items-center justify-between p-2 cursor-pointer"
                                        onClick={(e) => {
                                          if (!isExpanded) return;

                                          e.stopPropagation();
                                          setExpandedCveId(null);
                                        }}
                                      >
                                        <div className="flex items-center gap-2">
                                          <ShieldAlert
                                            className={`w-3.5 h-3.5 ${style.text}`}
                                          />
                                          <span
                                            className={`text-sm font-medium ${style.text}`}
                                          >
                                            {vuln.cveId}
                                          </span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                          <span
                                            className={`text-xs font-mono font-bold px-1.5 py-0.5 rounded ${style.badge} bg-opacity-90`}
                                          >
                                            {score.toFixed(1)}
                                          </span>
                                          {isExpanded ? (
                                            <ChevronUp className="w-4 h-4 opacity-50" />
                                          ) : (
                                            <ChevronDown className="w-4 h-4 opacity-50" />
                                          )}
                                        </div>
                                      </div>

                                      {/* Expanded Details */}
                                      {isExpanded && (
                                        <div className="px-3 pb-3 pt-1 border-t border-gray-200/50 dark:border-gray-700/50 bg-white/50 dark:bg-black/20">
                                          <p className="text-xs text-gray-600 dark:text-gray-300 mt-2 leading-relaxed">
                                            {vuln.description ??
                                              "No description provided."}
                                          </p>

                                          <div className="flex items-center justify-between mt-4">
                                            <a
                                              href={`https://cti.wazuh.com/vulnerabilities/cves/${vuln.cveId}`}
                                              target="_blank"
                                              rel="noopener noreferrer"
                                              className="flex items-center gap-1.5 text-xs text-blue-600 hover:text-blue-700 hover:underline"
                                              onClick={(e) =>
                                                e.stopPropagation()
                                              }
                                            >
                                              <ExternalLink className="w-3 h-3" />
                                              Wazuh CTI
                                            </a>

                                            <button
                                              onClick={(e) => {
                                                e.stopPropagation();
                                                setVulnIdToMark(vuln.id);
                                              }}
                                              className="flex items-center gap-1.5 text-xs bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-600 dark:text-gray-300 px-3 py-1.5 rounded transition-colors"
                                            >
                                              <Ban className="w-3 h-3" />
                                              Mark False Positive
                                            </button>
                                          </div>
                                        </div>
                                      )}
                                    </div>
                                  );
                                })}
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                ))
              )}
            </div>
          )}
        </Card>

        {/* 4. Command History List */}
        <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
          <button
            onClick={() => setIsCommandsOpen(!isCommandsOpen)}
            className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
          >
            <div className="space-y-1">
              <h3 className="font-bold text-lg flex items-center gap-2">
                <Terminal className="w-5 h-5 text-gray-500" />
                Command History
              </h3>
              <p className="text-sm text-gray-500">
                {commandsData
                  ? `${commandsData.commands.length} commands executed`
                  : "Click to load command history"}
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
                  <span>Fetching commands...</span>
                </div>
              ) : (
                <div className="overflow-x-auto max-h-96 overflow-y-auto">
                  <table className="w-full text-sm text-left">
                    <thead className="text-xs uppercase bg-gray-50 dark:bg-gray-800/50 text-gray-500 sticky top-0 backdrop-blur-sm">
                      <tr>
                        <th className="px-6 py-3">Command Type</th>
                        <th className="px-6 py-3">Status / Response</th>
                        <th className="px-6 py-3">Parameters</th>
                        <th className="px-6 py-3 text-right">Timestamps</th>
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
                                    const blob = new Blob(
                                      [cmd.agentResponse!],
                                      { type: "text/plain" }
                                    );
                                    const url = URL.createObjectURL(blob);
                                    window.open(url, "_blank");
                                  }}
                                  className="text-xs text-blue-600 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300 flex items-center gap-1 font-medium cursor-pointer"
                                >
                                  <ExternalLink className="w-3 h-3" />
                                  View Full Response
                                </button>
                              </div>
                            ) : cmd.sentAt ? (
                              <div className="flex items-center gap-2 text-blue-600">
                                <Clock className="w-4 h-4" />
                                <span>Sent to agent</span>
                              </div>
                            ) : (
                              <div className="flex items-center gap-2 text-gray-400">
                                <Loader2 className="w-4 h-4 animate-spin" />
                                <span>Pending pickup</span>
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
                              Created:{" "}
                              {new Date(cmd.createdAt).toLocaleString()}
                            </div>
                            {cmd.sentAt && (
                              <div className="text-[10px] text-gray-400">
                                Sent: {new Date(cmd.sentAt).toLocaleString()}
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
                            No commands found.
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

        {/* 5. Installed Packages Accordion (Existing) */}
        <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
          {/* ... Existing Package Table logic ... */}
          <button
            onClick={() => setIsPackagesOpen(!isPackagesOpen)}
            className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
          >
            {/* ... content ... */}
            <div className="space-y-1">
              <h3 className="font-bold text-lg">Installed Packages</h3>
              <p className="text-sm text-gray-500">
                {pkgData
                  ? `${pkgData.packages.length} packages found`
                  : "Click to load packages"}
              </p>
            </div>
            {isPackagesOpen ? (
              <ChevronUp className="w-5 h-5 text-gray-400" />
            ) : (
              <ChevronDown className="w-5 h-5 text-gray-400" />
            )}
          </button>

          {isPackagesOpen && (
            <div className="border-t border-gray-100 dark:border-gray-800">
              {pkgLoading ? (
                <div className="p-8 flex justify-center items-center text-gray-500 gap-2">
                  <Loader2 className="w-5 h-5 animate-spin" />
                  <span>Fetching package list...</span>
                </div>
              ) : (
                // ... Table ...
                <div className="overflow-x-auto max-h-96 overflow-y-auto">
                  <table className="w-full text-sm text-left">
                    {/* ... existing table header/body ... */}
                    <thead className="text-xs uppercase bg-gray-50 dark:bg-gray-800/50 text-gray-500 sticky top-0 backdrop-blur-sm">
                      <tr>
                        <th className="px-6 py-3">Package Name</th>
                        <th className="px-6 py-3">Version</th>
                        <th className="px-6 py-3 text-right">ID</th>
                      </tr>
                    </thead>
                    <tbody>
                      {pkgData?.packages.map((pkg) => (
                        <tr
                          key={pkg.id}
                          className="border-b border-gray-100 dark:border-gray-800 hover:bg-gray-50 dark:hover:bg-gray-800/30"
                        >
                          <td className="px-6 py-3 font-medium flex items-center gap-2">
                            <Package className="w-4 h-4 text-gray-400" />
                            {pkg.name}
                          </td>
                          <td className="px-6 py-3 font-mono text-gray-600 dark:text-gray-400">
                            {pkg.version}
                          </td>
                          <td className="px-6 py-3 text-right text-gray-400">
                            #{pkg.id}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          )}
        </div>
      </div>

      {/* --- MODALS --- */}

      {/* 1. False Positive Modal (Existing) */}
      <ConfirmationModal
        isOpen={!!vulnIdToMark}
        onClose={() => setVulnIdToMark(null)}
        onConfirm={() =>
          vulnIdToMark && markFalsePositiveMutation.mutate(vulnIdToMark)
        }
        title="Mark as False Positive?"
        confirmLabel="Yes, Mark False Positive"
        isLoading={markFalsePositiveMutation.isPending}
        message={
          <p>
            Are you sure you want to mark this package as a{" "}
            <b>False Positive</b>? It will be hidden from the vulnerability
            report.
          </p>
        }
      />

      {/* 2. Disable Agent Modal */}
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

      {/* 3. Upgrade Package Modal (Inline Implementation for simplicity) */}
      {showUpgradeModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm animate-in fade-in">
          <div className="bg-white dark:bg-gray-900 rounded-lg shadow-xl w-full max-w-md border border-gray-200 dark:border-gray-800 overflow-hidden">
            <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 flex justify-between items-center">
              <h3 className="font-semibold text-lg">Upgrade Package</h3>
              <button onClick={() => setShowUpgradeModal(false)}>
                <XCircle className="w-5 h-5 text-gray-400 hover:text-gray-600" />
              </button>
            </div>
            <div className="p-6 space-y-4">
              <p className="text-sm text-gray-600 dark:text-gray-300">
                Enter the exact name of the package you wish to upgrade via the
                package manager.
              </p>
              <div>
                <label className="block text-xs font-medium text-gray-700 dark:text-gray-300 mb-1">
                  Package Name
                </label>
                <input
                  type="text"
                  value={upgradePackageName}
                  onChange={(e) => setUpgradePackageName(e.target.value)}
                  placeholder="e.g. 7zip"
                  className="w-full px-3 py-2 border rounded-md dark:bg-gray-800 dark:border-gray-700 focus:ring-2 focus:ring-blue-500 outline-none"
                />
              </div>
            </div>
            <div className="px-6 py-4 bg-gray-50 dark:bg-gray-800/50 flex justify-end gap-3">
              <button
                onClick={() => setShowUpgradeModal(false)}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={() =>
                  upgradePackageMutation.mutate(upgradePackageName)
                }
                disabled={
                  !upgradePackageName.trim() || upgradePackageMutation.isPending
                }
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 rounded-md hover:bg-blue-700 disabled:opacity-50 flex items-center gap-2"
              >
                {upgradePackageMutation.isPending && (
                  <Loader2 className="w-4 h-4 animate-spin" />
                )}
                Send Command
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
