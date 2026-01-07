import React, { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { agentsApi } from "../api/endpoints";
import { Card } from "../components/Card";
import {
  Cpu,
  MemoryStick,
  Activity,
  Package,
  ChevronDown,
  ChevronUp,
  Loader2,
} from "lucide-react";

export const AgentDetail = () => {
  const { id } = useParams<{ id: string }>();
  const [isPackagesOpen, setIsPackagesOpen] = useState(false);

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

  // Group vulnerabilities by packageId
  const groupedVulns = useMemo(() => {
    if (!vulnData?.packages) return [];

    const groups = new Map<
      number,
      { name: string; version: string; cves: string[] }
    >();

    vulnData.packages.forEach((v) => {
      if (!groups.has(v.packageId)) {
        groups.set(v.packageId, {
          name: v.packageName,
          version: v.packageVersion,
          cves: [],
        });
      }
      groups.get(v.packageId)!.cves.push(v.cveId);
    });

    return Array.from(groups.values());
  }, [vulnData]);

  if (!agent)
    return <div className="p-8 text-center">Loading agent info...</div>;

  return (
    <div className="space-y-6">
      <div className="mb-6">
        <h2 className="text-3xl font-bold">
          {agent.computerName ?? "Unknown Computer"}
        </h2>
        <p className="text-gray-500 dark:text-gray-400">
          ID: {agent.id} • {agent.ipAddress}
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 auto-rows-min">
        {/* Block 1: Computer Info */}
        <Card title="Computer Info" className="h-full">
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

        {/* Block 2: Vulnerable Packages */}
        <Card
          title={`Vulnerable packages (${groupedVulns.length})`}
          className="md:col-span-2 lg:col-span-1 h-90"
        >
          {vulnLoading ? (
            <p>Loading...</p>
          ) : (
            <div className="space-y-3">
              {groupedVulns.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full text-green-500">
                  <ShieldCheckIcon className="w-12 h-12 mb-2" />
                  <p>No vulnerabilities found.</p>
                </div>
              ) : (
                groupedVulns.map((pkg) => (
                  <div
                    key={pkg.name}
                    className="flex flex-col p-4 bg-red-50 dark:bg-red-900/20 border border-red-100 dark:border-red-900/50 rounded-lg"
                  >
                    <div className="flex justify-between items-baseline mb-2">
                      <span className="font-bold text-red-700 dark:text-red-300">
                        {pkg.name}
                      </span>
                      <span className="text-sm text-red-600/80 dark:text-red-400 font-mono">
                        v{pkg.version}
                      </span>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {pkg.cves.map((cve) => (
                        <span
                          key={cve}
                          className="text-xs font-medium bg-red-200 dark:bg-red-900/60 text-red-800 dark:text-red-100 px-2 py-1 rounded border border-red-300 dark:border-red-800"
                        >
                          {cve}
                        </span>
                      ))}
                    </div>
                  </div>
                ))
              )}
            </div>
          )}
        </Card>

        {/* Block 3: All Packages Accordion */}
        <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
          <button
            onClick={() => setIsPackagesOpen(!isPackagesOpen)}
            className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
          >
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
                <div className="overflow-x-auto max-h-125 overflow-y-auto">
                  <table className="w-full text-sm text-left">
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
    </div>
  );
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

function ShieldCheckIcon(props: React.SVGProps<SVGSVGElement>) {
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
