import { Fragment, useMemo, useState } from "react";
import {
  agentsApi,
  type BduVulnerablePackageResponse,
  type VulnerablePackageStatus,
} from "../../../api/agentsApi";
import {
  getSeverityLevel,
  getBduScore,
  SEVERITY_CONFIG,
  type SeverityLevel,
} from "../../../utils/severity";
import {
  ChevronDown,
  ChevronUp,
  ExternalLink,
  Globe,
  Hash,
  Layers,
  Loader2,
  Package,
  ShieldCheck,
  Tag,
} from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { AccordionBlock } from "./AccordionBlock";

const TABS: VulnerablePackageStatus[] = [
  "vulnerable",
  "falsePositive",
  "patchless",
  "fixed",
];

export const BduVulnerablePackagesBlock = ({
  agentId,
}: {
  agentId: string;
}) => {
  const queryClient = useQueryClient();
  const { t } = useTranslation();

  const [isOpen, setIsOpen] = useState(false);
  const [activeTab, setActiveTab] =
    useState<VulnerablePackageStatus>("vulnerable");
  const [expandedBduId, setExpandedBduId] = useState<number | null>(null);
  const [expandedPackageId, setExpandedPackageId] = useState<number | null>(
    null,
  );

  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns-bdu", agentId, activeTab],
    queryFn: () => agentsApi.getBduVulnPackages(agentId!, activeTab),
    enabled: !!agentId && isOpen,
  });

  const changeStatusMutation = useMutation({
    mutationFn: ({
      id,
      status,
    }: {
      id: number;
      status: VulnerablePackageStatus;
    }) => agentsApi.changeVulnStatusBdu(id, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vulns-bdu", agentId] });
    },
  });

  const organizedVulns = useMemo(() => {
    if (!vulnData?.packages) return [];

    const groups = new Map<
      number,
      {
        packageId: number;
        name: string;
        version: string;
        maxScore: number;
        buckets: Record<SeverityLevel, BduVulnerablePackageResponse[]>;
      }
    >();

    vulnData.packages.forEach((v) => {
      const score = getBduScore(v);
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
      group.buckets[severity].push(v);

      if (score > group.maxScore) {
        group.maxScore = score;
      }
    });

    return Array.from(groups.values()).sort((a, b) => b.maxScore - a.maxScore);
  }, [vulnData]);

  const getFstecLink = (bduId: string) => {
    const idPart = bduId.replace(/^BDU:/i, "");
    return `https://bdu.fstec.ru/vul/${idPart}`;
  };

  return (
    <AccordionBlock
      isOpen={isOpen}
      setIsOpen={setIsOpen}
      header={
        <>
          <h3 className="font-bold text-lg">
            {t("agent_details.bdu_vulns", {
              defaultValue: `Vulnerabilities (BDU)`,
            })}
          </h3>
          <p className="text-sm text-gray-500">
            {vulnData && organizedVulns
              ? t("agent_details.bdu_vulns_total", {
                  amount: organizedVulns.length,
                })
              : t("agent_details.click_to_load")}
          </p>
        </>
      }
      body={
        <div className="flex flex-col flex-1 h-full">
          {/* Tabs Navigation */}
          <div className="flex border-b border-gray-200 dark:border-gray-700 px-4 pt-2 mb-4 shrink-0 overflow-x-auto custom-scrollbar">
            {TABS.map((tab) => (
              <button
                key={tab}
                onClick={() => {
                  setActiveTab(tab);
                  setExpandedPackageId(null);
                  setExpandedBduId(null);
                }}
                className={`px-4 py-2 border-b-2 font-medium text-sm transition-colors whitespace-nowrap ${
                  activeTab === tab
                    ? "border-blue-500 text-blue-600 dark:text-blue-400"
                    : "border-transparent text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                }`}
              >
                {t(`agent_details.status_${tab}`)}
              </button>
            ))}
          </div>

          {vulnLoading ? (
            <div className="flex justify-center items-center h-40">
              <Loader2 className="animate-spin text-gray-400" />
            </div>
          ) : (
            <div className="space-y-4 overflow-y-auto pl-4 pr-4 flex-1 custom-scrollbar pb-4">
              {organizedVulns.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-40 text-green-500">
                  <ShieldCheck className="w-12 h-12 mb-2" />
                  <p>
                    {t("agent_details.no_vulns", "No vulnerabilities found")}
                  </p>
                </div>
              ) : (
                organizedVulns.map((pkg) => (
                  <div
                    key={pkg.packageId}
                    className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white dark:bg-gray-800"
                  >
                    {/* Package Header */}
                    <div
                      className="bg-gray-50 dark:bg-gray-900/50 p-3 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center cursor-pointer hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
                      onClick={() =>
                        setExpandedPackageId((prev) =>
                          prev === pkg.packageId ? null : pkg.packageId,
                        )
                      }
                    >
                      <div className="flex items-center gap-2 overflow-hidden">
                        <Package className="w-4 h-4 text-gray-400 shrink-0" />
                        <div className="truncate">
                          <span className="font-bold text-gray-700 dark:text-gray-200 mr-2">
                            {pkg.name}
                          </span>
                          <span className="text-xs font-mono text-gray-500 bg-gray-200 dark:bg-gray-700 px-1.5 py-0.5 rounded">
                            v{pkg.version}
                          </span>
                        </div>
                      </div>
                      <span className="text-xs font-bold text-gray-500 whitespace-nowrap ml-2">
                        {t("agent_details.max_cvss", {
                          score: pkg.maxScore.toFixed(1),
                          defaultValue: `Max CVSS: ${pkg.maxScore.toFixed(1)}`,
                        })}
                      </span>
                    </div>

                    {/* Severity Intervals */}
                    {expandedPackageId === pkg.packageId && (
                      <div className="p-3 space-y-3 bg-gray-50/50 dark:bg-black/10">
                        {(
                          [
                            "CRITICAL",
                            "HIGH",
                            "MEDIUM",
                            "LOW",
                          ] as SeverityLevel[]
                        ).map((severity) => {
                          const vulns = pkg.buckets[severity];
                          if (vulns.length === 0) return null;

                          const style = SEVERITY_CONFIG[severity];

                          return (
                            <div key={severity} className="space-y-1.5">
                              <h5
                                className={`text-[10px] font-bold tracking-wider ${style.text} flex items-center gap-1.5 px-1`}
                              >
                                <div
                                  className={`w-2 h-2 rounded-full ${style.badge}`}
                                />
                                {severity}
                              </h5>

                              <div className="flex flex-col gap-2">
                                {vulns.map((vuln) => {
                                  const isExpanded = expandedBduId === vuln.id;
                                  const score = getBduScore(vuln);

                                  return (
                                    <div
                                      key={vuln.id}
                                      className={`
                                        border rounded-md transition-all duration-200
                                        ${
                                          isExpanded
                                            ? "shadow-sm ring-1 ring-gray-200 dark:ring-gray-600"
                                            : "hover:bg-white dark:hover:bg-gray-700 cursor-pointer"
                                        }
                                        ${style.bg} ${style.border}
                                      `}
                                      onClick={() =>
                                        !isExpanded && setExpandedBduId(vuln.id)
                                      }
                                    >
                                      {/* BDU ID Header Line */}
                                      <div
                                        className="flex items-center justify-between p-2.5 cursor-pointer"
                                        onClick={(e) => {
                                          if (!isExpanded) return;
                                          e.stopPropagation();
                                          setExpandedBduId(null);
                                        }}
                                      >
                                        <div className="flex items-center gap-2">
                                          <Globe
                                            className={`w-3.5 h-3.5 ${style.text}`}
                                          />
                                          <span
                                            className={`text-sm font-medium ${style.text}`}
                                          >
                                            {vuln.bduId}
                                          </span>
                                        </div>
                                        <div className="flex items-center gap-2">
                                          <span
                                            className={`text-xs font-mono font-bold px-1.5 py-0.5 rounded ${style.badge} bg-opacity-90 min-w-8 text-center`}
                                          >
                                            {score.toFixed(1)}
                                          </span>
                                          {isExpanded ? (
                                            <ChevronUp className="w-4 h-4 text-gray-400" />
                                          ) : (
                                            <ChevronDown className="w-4 h-4 text-gray-400" />
                                          )}
                                        </div>
                                      </div>

                                      {/* Expanded Details */}
                                      {isExpanded && (
                                        <div className="px-3 pb-3 pt-1 border-t border-gray-100 dark:border-gray-700/50">
                                          {/* Description */}
                                          <p className="text-sm text-gray-600 dark:text-gray-300 mt-2 leading-relaxed">
                                            {vuln.description ||
                                              t(
                                                "no_description",
                                                "No description provided.",
                                              )}
                                          </p>

                                          <div className="mt-4 space-y-3">
                                            {/* CWEs */}
                                            {vuln.cwes &&
                                              vuln.cwes.length > 0 && (
                                                <div className="flex flex-wrap gap-2 items-center">
                                                  <div className="flex items-center gap-1 text-xs text-gray-400 uppercase tracking-wider font-semibold">
                                                    <Tag className="w-3 h-3" />
                                                  </div>
                                                  <div className="flex flex-wrap gap-1">
                                                    {vuln.cwes.map((cwe) => (
                                                      <span
                                                        key={cwe.id}
                                                        className="text-[10px] bg-gray-100 dark:bg-gray-700 px-1.5 py-0.5 rounded text-gray-700 dark:text-gray-200 border border-gray-200 dark:border-gray-600"
                                                        title={cwe.name}
                                                      >
                                                        {cwe.id}
                                                      </span>
                                                    ))}
                                                  </div>
                                                </div>
                                              )}

                                            {/* Identifiers */}
                                            {vuln.identifiers &&
                                              vuln.identifiers.length > 0 && (
                                                <div className="grid grid-cols-[min-content_1fr] gap-x-2 gap-y-1.5 items-start text-xs">
                                                  {vuln.identifiers.map(
                                                    (ident) => (
                                                      <Fragment
                                                        key={ident.value}
                                                      >
                                                        <div className="pt-0.5">
                                                          <Hash className="w-3 h-3 text-gray-400" />
                                                        </div>
                                                        <a
                                                          href={
                                                            ident.link ?? "#"
                                                          }
                                                          target="_blank"
                                                          rel="noopener noreferrer"
                                                          className="text-blue-500 hover:text-blue-600 hover:underline break-all"
                                                        >
                                                          {ident.value}
                                                        </a>
                                                      </Fragment>
                                                    ),
                                                  )}
                                                </div>
                                              )}

                                            {/* Affected Software */}
                                            {vuln.software &&
                                              vuln.software.length > 0 && (
                                                <div className="text-xs text-gray-500 flex flex-col items-start gap-2 pt-2">
                                                  <div className="flex flex-row gap-1.5 items-center">
                                                    <Layers className="w-3 h-3 text-gray-500" />
                                                    <span className="font-semibold opacity-90">
                                                      {t(
                                                        "agent_details.affected_software",
                                                        "Affected Software",
                                                      )}
                                                    </span>
                                                  </div>
                                                  <div className="flex flex-col gap-2 w-full pl-4 border-l-2 border-gray-100 dark:border-gray-700 ml-1.5">
                                                    {vuln.software.map((s) => (
                                                      <div
                                                        key={`${s.name}-${s.version}-${s.vendor}-${s.platform}`}
                                                        className="p-2.5 rounded-md text-sm bg-blue-400 dark:bg-blue-500 text-white shadow-sm flex flex-col sm:flex-row sm:justify-between sm:items-center gap-1.5"
                                                      >
                                                        <span className="font-bold tracking-wide">
                                                          {s.name}
                                                        </span>
                                                        <span className="font-mono text-xs bg-blue-800/50 px-2 py-0.5 rounded text-blue-50">
                                                          {s.version}
                                                        </span>
                                                      </div>
                                                    ))}
                                                  </div>
                                                </div>
                                              )}
                                          </div>

                                          {/* Footer: Link to FSTEC and Status Dropdown */}
                                          <div className="flex items-center justify-between mt-4">
                                            <a
                                              href={getFstecLink(vuln.bduId)}
                                              target="_blank"
                                              rel="noopener noreferrer"
                                              className="flex items-center gap-1.5 text-xs text-blue-600 hover:text-blue-700 hover:underline font-medium"
                                              onClick={(e) =>
                                                e.stopPropagation()
                                              }
                                            >
                                              <ExternalLink className="w-3 h-3" />
                                              {t(
                                                "agent_details.bdu_fstec",
                                                "FSTEC BDU",
                                              )}
                                            </a>

                                            {/* Status Change Dropdown */}
                                            <select
                                              value="" // Empty string acts as placeholder
                                              onClick={(e) =>
                                                e.stopPropagation()
                                              }
                                              onChange={(e) => {
                                                e.stopPropagation();
                                                const selectedStatus = e.target
                                                  .value as VulnerablePackageStatus;
                                                if (selectedStatus) {
                                                  changeStatusMutation.mutate({
                                                    id: vuln.id,
                                                    status: selectedStatus,
                                                  });
                                                }
                                              }}
                                              disabled={
                                                changeStatusMutation.isPending
                                              }
                                              className="text-xs font-medium bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 px-3 py-1.5 rounded transition-colors border-none cursor-pointer focus:ring-2 focus:ring-blue-500/50 outline-none"
                                            >
                                              <option value="" disabled>
                                                {t(
                                                  "agent_details.change_status",
                                                )}
                                              </option>
                                              {TABS.filter(
                                                (s) => s !== activeTab,
                                              ).map((s) => (
                                                <option key={s} value={s}>
                                                  {t(
                                                    `agent_details.status_${s}`,
                                                  )}
                                                </option>
                                              ))}
                                            </select>
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
        </div>
      }
    />
  );
};
