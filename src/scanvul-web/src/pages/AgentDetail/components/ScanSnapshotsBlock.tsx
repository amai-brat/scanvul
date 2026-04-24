import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import {
  ChevronDown,
  ChevronUp,
  Package
} from "lucide-react";
import { agentsApi, type ScanSnapshotSummary } from "../../../api/agentsApi";
import { Card } from "../../../components/Card";

interface ScanSnapshotsBlockProps {
  agentId: string;
}

export const ScanSnapshotsBlock: React.FC<ScanSnapshotsBlockProps> = ({
  agentId,
}) => {
  const { t } = useTranslation();
  const [selectedSnapshotId, setSelectedSnapshotId] = useState<string | null>(
    null,
  );

  const { data, isLoading, isError } = useQuery({
    queryKey: ["agent-snapshots", agentId],
    queryFn: () => agentsApi.getSnapshotSummaries(agentId),
  });

  // Sort summaries by creation time (latest first)
  const sortedSummaries =
    data?.summaries.sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    ) ?? [];

  return (
    <Card title={t("agent_details.scan_snapshots")} className="h-full">
      <div className="space-y-4 max-h-96 overflow-y-auto pr-2 custom-scrollbar">
        {isLoading && (
          <p className="text-gray-500 text-sm">{t("common.loading")}</p>
        )}
        {isError && <p className="text-red-500 text-sm">{t("common.error")}</p>}
        {!isLoading && sortedSummaries.length === 0 && (
          <p className="text-gray-500 text-sm">
            {t("agent_details.no_snapshots")}
          </p>
        )}

        {sortedSummaries.map((summary) => (
          <SnapshotSummaryRow
            key={summary.snapshotId}
            summary={summary}
            onClick={() => setSelectedSnapshotId(summary.snapshotId)}
          />
        ))}
      </div>

      {selectedSnapshotId && (
        <SnapshotDetailModal
          snapshotId={selectedSnapshotId}
          onClose={() => setSelectedSnapshotId(null)}
        />
      )}
    </Card>
  );
};

const StatBadge = ({
  icon,
  total,
  added,
  removed,
  type,
  titleKey,
}: {
  icon: React.ReactNode;
  total: number;
  added?: number;
  removed?: number;
  type: "packages" | "vuln";
  titleKey: string;
}) => {
  const { t } = useTranslation();

  const addedColor =
    type === "vuln"
      ? "text-red-600 dark:text-red-400"
      : "text-green-600 dark:text-green-400";
  const removedColor =
    type === "vuln"
      ? "text-green-600 dark:text-green-400"
      : "text-gray-600 dark:text-gray-400";

  const hasDiff = added !== undefined && removed !== undefined;

  const tooltip = t(`snapshots.tooltips.${titleKey}`, {
    total,
    added: added ?? 0,
    removed: removed ?? 0,
  });

  return (
    <div
      className="flex items-center space-x-2 bg-gray-50 dark:bg-gray-800/60 px-2.5 py-1.5 rounded-lg border border-gray-200 dark:border-gray-700/50"
      title={tooltip}
    >
      {/* Icon & Total count */}
      <div className="flex items-center space-x-1.5 border-r border-gray-200 dark:border-gray-700 pr-2">
        <span className="text-gray-500 dark:text-gray-400">{icon}</span>
        <span className="font-semibold text-gray-900 dark:text-gray-100">
          {total}
        </span>
      </div>

      {/* Diff counts */}
      {hasDiff ? (
        <div className="flex items-center space-x-2 text-xs font-bold font-mono tracking-tight">
          <span className={addedColor}>+{added}</span>
          <span className={removedColor}>-{removed}</span>
        </div>
      ) : (
        <div className="text-xs text-gray-400 dark:text-gray-500 italic font-medium">
          {t("snapshots.initial", "Initial")}
        </div>
      )}
    </div>
  );
};

const SnapshotSummaryRow: React.FC<{ summary: ScanSnapshotSummary; onClick: () => void }> = ({
  summary,
  onClick,
}) => {  
  const date = new Intl.DateTimeFormat(navigator.language, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(summary.createdAt));

  return (
    <div
      onClick={onClick}
      className="p-3 border border-gray-200 dark:border-gray-800 rounded-xl hover:bg-gray-100/50 dark:hover:bg-gray-800/80 transition-all cursor-pointer group"
    >
      <div className="text-xs font-medium text-gray-500 dark:text-gray-400 mb-3 group-hover:text-blue-600 dark:group-hover:text-blue-400 transition-colors">
        {date}
      </div>
      
      <div className="flex flex-wrap gap-3">
        <StatBadge
          icon={<Package size={16} />}
          total={summary.payload.packages}
          added={summary.diff?.addedPackages}
          removed={summary.diff?.removedPackages}
          type="packages"
          titleKey="packages"
        />
        <StatBadge
          icon={<p className="text-sm">CVE</p>}
          total={summary.payload.vulnerablePackages}
          added={summary.diff?.addedVulnerablePackages}
          removed={summary.diff?.removedVulnerablePackages}
          type="vuln"
          titleKey="cve"
        />
        <StatBadge
          icon={<p className="text-sm">БДУ</p>}
          total={summary.payload.bduVulnerablePackages}
          added={summary.diff?.addedBduVulnerablePackages}
          removed={summary.diff?.removedBduVulnerablePackages}
          type="vuln"
          titleKey="bdu"
        />
      </div>
    </div>
  );
};

const SnapshotDetailModal: React.FC<{
  snapshotId: string;
  onClose: () => void;
}> = ({ snapshotId, onClose }) => {
  const { t } = useTranslation();
  const [showFullPayload, setShowFullPayload] = useState(false);

  // Fetch payload details
  const { data, isLoading } = useQuery({
    queryKey: ["snapshot-detail", snapshotId],
    queryFn: () => agentsApi.getSnapshotPayload(snapshotId, true),
  });

  React.useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onClose();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [onClose]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
      onClick={onClose}
    >
      <div
        className="bg-white dark:bg-gray-900 rounded-xl shadow-xl w-full max-w-3xl max-h-[90vh] flex flex-col overflow-hidden border border-gray-200 dark:border-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-800">
          <h3 className="text-lg font-semibold text-gray-900 dark:text-gray-100">
            {t("snapshots.modal_title")}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors p-1"
          >
            ✕
          </button>
        </div>

        <div className="p-4 overflow-y-auto custom-scrollbar flex-1 space-y-6">
          {isLoading ? (
            <p className="text-center text-gray-500 py-8">
              {t("common.loading")}
            </p>
          ) : (
            <>
              {/* DIFF SECTION */}
              <section>
                <h4 className="font-semibold text-gray-700 dark:text-gray-300 mb-3">
                  {t("snapshots.diff_section")}
                </h4>
                {!data?.diff ? (
                  <p className="text-sm text-gray-500 italic bg-gray-50 dark:bg-gray-800 p-3 rounded">
                    {t("snapshots.no_diff")}
                  </p>
                ) : (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <DiffList
                      title={t("snapshots.added_packages")}
                      items={data.diff.addedPackages}
                      type="success"
                    />
                    <DiffList
                      title={t("snapshots.removed_packages")}
                      items={data.diff.removedPackages}
                      type="danger"
                    />
                    <DiffList
                      title={t("snapshots.added_cve")}
                      items={data.diff.addedVulnerablePackages}
                      type="danger"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.removed_cve")}
                      items={data.diff.removedVulnerablePackages}
                      type="success"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.added_cve")}
                      items={data.diff.addedBduVulnerablePackages}
                      type="danger"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.removed_cve")}
                      items={data.diff.removedBduVulnerablePackages}
                      type="success"
                      isVuln
                    />
                  </div>
                )}
              </section>

              {/* PAYLOAD ACCORDION */}
              <section className="border border-gray-200 dark:border-gray-800 rounded-lg overflow-hidden">
                <button
                  onClick={() => setShowFullPayload(!showFullPayload)}
                  className="w-full flex justify-between items-center p-3 bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-750 transition-colors outline-none focus:ring-2 focus:ring-blue-500/50"
                >
                  <span className="font-semibold text-gray-700 dark:text-gray-300">
                    {t("snapshots.full_payload")}
                  </span>
                  {showFullPayload ? (
                    <ChevronUp size={18} />
                  ) : (
                    <ChevronDown size={18} />
                  )}
                </button>

                {showFullPayload && data?.payload && (
                  <div className="p-4 space-y-4 bg-white dark:bg-gray-900">
                    <PayloadList
                      title={t("snapshots.total_packages")}
                      items={data.payload.packages}
                    />
                    <PayloadList
                      title={t("snapshots.total_cve")}
                      items={data.payload.vulnerablePackages}
                      isVuln
                    />
                    <PayloadList
                      title={t("snapshots.total_bdu")}
                      items={data.payload.bduVulnerablePackages}
                      isVuln
                    />
                  </div>
                )}
              </section>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

const DiffList: React.FC<{
  title: string;
  items: any[];
  type: "success" | "danger";
  isVuln?: boolean;
}> = ({ title, items, type, isVuln }) => {
  if (!items || items.length === 0) return null;
  const colorClass =
    type === "success"
      ? "text-green-600 dark:text-green-400 bg-green-50 dark:bg-green-900/20"
      : "text-red-600 dark:text-red-400 bg-red-50 dark:bg-red-900/20";

  return (
    <div className={`p-3 rounded-lg border border-transparent ${colorClass}`}>
      <h5 className="font-medium text-sm mb-2">
        {title} ({items.length})
      </h5>
      <ul className="text-xs space-y-1 max-h-32 overflow-y-auto pr-1">
        {items.map((item) => (
          <li key={item.id}>
            {isVuln ? (
              <span>
                {item.vulnerabilityId}{" "}
                <span className="opacity-70">
                  (Pkg ID: {item.packageInfoId})
                </span>
              </span>
            ) : (
              <span>
                {item.name} <span className="opacity-70">v{item.version}</span>
              </span>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
};

const PayloadList: React.FC<{
  title: string;
  items: any[];
  isVuln?: boolean;
}> = ({ title, items, isVuln }) => {
  if (!items || items.length === 0) return null;

  return (
    <div>
      <h5 className="font-medium text-sm text-gray-600 dark:text-gray-400 mb-2">
        {title} ({items.length})
      </h5>
      <ul className="text-xs space-y-1 max-h-40 overflow-y-auto bg-gray-50 dark:bg-gray-800/50 p-2 rounded border border-gray-100 dark:border-gray-800">
        {items.map((item) => (
          <li key={item.id} className="text-gray-700 dark:text-gray-300">
            {isVuln ? (
              <span>
                {item.vulnerabilityId}{" "}
                <span className="text-gray-400">
                  - Pkg ID: {item.packageInfoId}
                </span>
              </span>
            ) : (
              <span>
                {item.name}{" "}
                <span className="text-gray-400">v{item.version}</span>
              </span>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
};
