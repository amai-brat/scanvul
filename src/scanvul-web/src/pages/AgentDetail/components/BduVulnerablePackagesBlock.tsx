import { Fragment, useMemo, useState } from "react";
import {
  agentsApi,
  type BduVulnerablePackageResponse,
} from "../../../api/agentsApi";
import { Card } from "../../../components/Card";
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
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";


export const BduVulnerablePackagesBlock = ({
  agentId,
}: {
  agentId: string;
}) => {
  const { t } = useTranslation();

  const [expandedBduId, setExpandedBduId] = useState<number | null>(null);
  const [expandedPackageId, setExpandedPackageId] = useState<number | null>(
    null,
  );

  const { data: vulnData, isLoading: vulnLoading } = useQuery({
    queryKey: ["vulns-bdu", agentId],
    queryFn: () => agentsApi.getBduVulnPackages(agentId!),
    enabled: !!agentId,
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
    <Card
      title={t("agent_details.bdu_vulns", {
        amount: vulnData?.packages.length ?? 0,
        defaultValue: `Vulnerabilities (BDU)`,
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
              <p>{t("agent_details.no_vulns", "No vulnerabilities found")}</p>
            </div>
          ) : (
            <>
              <div className="bg-yellow-50 dark:bg-yellow-950/20 border border-yellow-200 dark:border-yellow-900 rounded-md p-1 w-min">
                <span
                  className="text-yellow-700 dark:text-yellow-300 font-bold"
                  title={t(
                    "agent_details.bdu_vulns_warning_title",
                    "Package versions are not checked automatically against vulnerabilities from BDU. Please verify according to affected software listed in each vulnerability",
                  )}
                >
                  {t("app.attention", "Attention")}
                </span>
              </div>
              {organizedVulns.map((pkg) => (
                <div
                  key={pkg.packageId}
                  className="border border-gray-200 dark:border-gray-700 rounded-lg overflow-hidden bg-white dark:bg-gray-800"
                >
                  {/* Package Header */}
                  <div
                    className="bg-gray-50 dark:bg-gray-900/50 p-3 border-b border-gray-100 dark:border-gray-700 flex justify-between items-center cursor-pointer"
                    onClick={() =>
                      setExpandedPackageId((prev) =>
                        prev === null ? pkg.packageId : null,
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
                        defaultValue: `Max CVSS: ${pkg.maxScore.toFixed(1)}`,
                      })}
                    </span>
                  </div>

                  {/* Severity Intervals */}
                  {expandedPackageId === pkg.packageId && (
                    <div className="p-3 space-y-3">
                      {(
                        ["CRITICAL", "HIGH", "MEDIUM", "LOW"] as SeverityLevel[]
                      ).map((severity) => {
                        const vulns = pkg.buckets[severity];
                        if (vulns.length === 0) return null;

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
                                        ? "shadow-md ring-1 ring-gray-200 dark:ring-gray-700"
                                        : "hover:bg-gray-50 dark:hover:bg-gray-700/30 cursor-pointer"
                                    }
                                    ${style.bg} ${style.border}
                                  `}
                                    onClick={() =>
                                      !isExpanded && setExpandedBduId(vuln.id)
                                    }
                                  >
                                    {/* BDU ID Header Line */}
                                    <div
                                      className="flex items-center justify-between p-2 cursor-pointer"
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
                                        {/* Description */}
                                        <p className="text-xs text-gray-600 dark:text-gray-300 mt-2 leading-relaxed">
                                          {vuln.description ||
                                            t(
                                              "no_description",
                                              "No description provided.",
                                            )}
                                        </p>

                                        <div className="mt-3 space-y-2">
                                          {/* CWEs */}
                                          {vuln.cwes &&
                                            vuln.cwes.length > 0 && (
                                              <div className="flex flex-wrap gap-1 items-center">
                                                <Tag className="w-3 h-3 text-gray-400" />
                                                {vuln.cwes.map((cwe) => (
                                                  <span
                                                    key={cwe.id}
                                                    className="text-[10px] bg-gray-100 dark:bg-gray-700 px-1.5 py-0.5 rounded text-gray-600 dark:text-gray-300 border border-gray-200 dark:border-gray-600"
                                                    title={cwe.name}
                                                  >
                                                    {cwe.id}
                                                  </span>
                                                ))}
                                              </div>
                                            )}

                                          {/* Identifiers (Other Links) */}
                                          {vuln.identifiers &&
                                            vuln.identifiers.length > 0 && (
                                              <div className="grid grid-cols-[1fr_20fr] gap-2 items-center text-xs">
                                                {vuln.identifiers.map(
                                                  (ident) => (
                                                    <Fragment key={ident.value}>
                                                      <Hash className="w-3 h-3 text-gray-400" />
                                                      <a
                                                        key={ident.value}
                                                        href={ident.link}
                                                        target="_blank"
                                                        rel="noopener noreferrer"
                                                        className="text-blue-500 hover:underline"
                                                      >
                                                        {ident.value}
                                                      </a>
                                                    </Fragment>
                                                  ),
                                                )}
                                              </div>
                                            )}

                                          {/* Affected Software (Summary) */}
                                          {vuln.software &&
                                            vuln.software.length > 0 && (
                                              <div className="text-xs text-gray-500 mt-1 flex-col items-start gap-1.5">
                                                <div className="flex flex-row gap-1.5 items-center pb-1">
                                                  <Layers className="w-3 h-3 mt-0.5 text-gray-400" />
                                                  <span className="opacity-80">
                                                    {t(
                                                      "agent_details.affected_software",
                                                      "Affected Software",
                                                    )}
                                                    :{" "}
                                                  </span>
                                                </div>
                                                <div className="flex flex-col">
                                                  {vuln.software.map((s) => (
                                                    <div
                                                      key={
                                                        s.name +
                                                        s.version +
                                                        s.platform +
                                                        s.vendor
                                                      }
                                                      className="p-2 rounded-sm text-xs bg-blue-800 text-amber-50 flex flex-col gap-0.5 mb-0.5"
                                                    >
                                                      <p>{s.name}</p>
                                                      <p>{s.version}</p>
                                                    </div>
                                                  ))}
                                                </div>
                                              </div>
                                            )}
                                        </div>

                                        {/* Footer: Link to FSTEC */}
                                        <div className="flex items-center justify-end mt-4 pt-2 border-t border-gray-100 dark:border-gray-700/30">
                                          <a
                                            href={getFstecLink(vuln.bduId)}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="flex items-center gap-1.5 text-xs text-blue-600 hover:text-blue-700 hover:underline font-medium"
                                            onClick={(e) => e.stopPropagation()}
                                          >
                                            <ExternalLink className="w-3 h-3" />
                                            {t(
                                              "agent_details.bdu_fstec",
                                              "FSTEC BDU",
                                            )}
                                          </a>
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
              ))}
            </>
          )}
        </div>
      )}
    </Card>
  );
};
