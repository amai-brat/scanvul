import { useMemo, useState } from "react";
import { agentsApi, type VulnerablePackageResponse } from "../../../api/agentsApi";
import { Card } from "../../../components/Card";
import { getCvssScore, getSeverityLevel, SEVERITY_CONFIG, type SeverityLevel } from "../../../utils/severity";
import { Ban, ChevronDown, ChevronUp, ExternalLink, Loader2, Package, ShieldAlert, ShieldCheck } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ConfirmationModal } from "../../../components/ConfimationModal";
import { Trans, useTranslation } from "react-i18next";


export const VulnerablePackagesBlock = ({ agentId }: { agentId: string }) => {
  const queryClient = useQueryClient();
  const { t } = useTranslation();

  const [expandedCveId, setExpandedCveId] = useState<number | null>(null);
  const [expandedPackageId, setExpandedPackageId] = useState<number | null>(null);
  const [vulnIdToMark, setVulnIdToMark] = useState<number | null>(null);

  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns", agentId],
    queryFn: () => agentsApi.getVulnPackages(agentId!),
    enabled: !!agentId,
  });

  const markFalsePositiveMutation = useMutation({
    mutationFn: (vulnerablePackageId: number) =>
      agentsApi.markFalsePositive(vulnerablePackageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["vulns", agentId] });
      setVulnIdToMark(null);
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


  return (
    <>
      <ConfirmationModal
        isOpen={!!vulnIdToMark}
        onClose={() => setVulnIdToMark(null)}
        onConfirm={() =>
          vulnIdToMark && markFalsePositiveMutation.mutate(vulnIdToMark)
        }
        title={t("agent_details.mark_false_positive")}
        confirmLabel={t("agent_details.mark_false_positive_confirm")}
        isLoading={markFalsePositiveMutation.isPending}
        message={
          <p>
            <Trans
              i18nKey="agent_details.mark_false_positive_desc"
              components={{
                bold: (
                  <span className="font-semibold text-gray-900 dark:text-white" />
                ),
              }}
            />
          </p>
        }
      />

      <Card
        title={t("agent_details.vulns", {
          amount: vulnData?.packages.length ?? 0,
        })}
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
                <ShieldCheck className="w-12 h-12 mb-2" />
                <p>{t("agent_details.no_vulns")}</p>
              </div>
            ) : (
              organizedVulns.map((pkg) => (
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
                      {t("agent_details.max_cvss", {
                        score: pkg.maxScore.toFixed(1),
                      })}
                    </span>
                  </div>

                  {/* Severity Intervals */}
                  {expandedPackageId === pkg.packageId && (
                    <div className="p-3 space-y-3">
                      {(
                        ["CRITICAL", "HIGH", "MEDIUM", "LOW"] as SeverityLevel[]
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
                                            onClick={(e) => e.stopPropagation()}
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
                                            {t(
                                              "agent_details.mark_false_positive"
                                            )}
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
    </>
  );
}