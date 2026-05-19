import React, { useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { ChevronDown, ChevronUp, Package } from "lucide-react";
import { agentsApi, type PackageInfo, type ScanSnapshotSummary, type VulnerablePackage, type VulnerablePackageStatus } from "../../../api/agentsApi";
import { Card } from "../../../components/Card";
import { modalEffect } from "../../../utils/modal";

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
      <div className="flex items-center space-x-1.5 border-r border-gray-200 dark:border-gray-700 pr-2">
        <span className="text-gray-500 dark:text-gray-400">{icon}</span>
        <span className="font-semibold text-gray-900 dark:text-gray-100">
          {total}
        </span>
      </div>

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

const SnapshotSummaryRow: React.FC<{
  summary: ScanSnapshotSummary;
  onClick: () => void;
}> = ({ summary, onClick }) => {
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
          icon={<p className="text-[11px] font-bold">CVE</p>}
          total={summary.payload.vulnerablePackages}
          added={summary.diff?.addedVulnerablePackages}
          removed={summary.diff?.removedVulnerablePackages}
          type="vuln"
          titleKey="cve"
        />
        <StatBadge
          icon={<p className="text-[11px] font-bold">БДУ</p>}
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

  const { data: diffData, isLoading: isDiffLoading } = useQuery({
    queryKey: ["snapshot-detail-diff", snapshotId],
    queryFn: () => agentsApi.getSnapshotLastDiff(snapshotId),
  });

  const { data: payloadData, isLoading: isPayloadLoading } = useQuery({
    queryKey: ["snapshot-detail", snapshotId],
    queryFn: () => agentsApi.getSnapshotPayload(snapshotId),
    enabled: showFullPayload,
  });

  React.useEffect(() => {
    modalEffect(onClose);
  }, [onClose]);

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
      onClick={onClose}
    >
      <div
        className="bg-white dark:bg-gray-900 rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] flex flex-col overflow-hidden border border-gray-200 dark:border-gray-800"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center p-5 border-b border-gray-200 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/20">
          <h3 className="text-lg font-bold text-gray-900 dark:text-gray-100">
            {t("snapshots.modal_title")}
          </h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 transition-colors p-1"
          >
            ✕
          </button>
        </div>

        <div className="p-5 overflow-y-auto custom-scrollbar flex-1 space-y-8">
          {isDiffLoading ? (
            <p className="text-center text-gray-500 py-8">
              {t("common.loading")}
            </p>
          ) : (
            <>
              {/* DIFF SECTION */}
              <section>
                <h4 className="font-bold text-gray-800 dark:text-gray-200 mb-4 tracking-tight">
                  {t("snapshots.diff_section")}
                </h4>
                {!diffData?.diff ? (
                  <p className="text-sm text-gray-500 italic bg-gray-50 dark:bg-gray-800 p-4 rounded-xl border border-gray-100 dark:border-gray-800">
                    {t("snapshots.no_diff")}
                  </p>
                ) : (
                  <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
                    <DiffList
                      title={t("snapshots.added_packages")}
                      items={diffData.diff.addedPackages}
                      type="success"
                    />
                    <DiffList
                      title={t("snapshots.removed_packages")}
                      items={diffData.diff.removedPackages}
                      type="danger"
                    />
                    <DiffList
                      title={t("snapshots.added_cve")}
                      items={diffData.diff.addedVulnerablePackages}
                      type="danger"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.removed_cve")}
                      items={diffData.diff.removedVulnerablePackages}
                      type="success"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.added_bdu")}
                      items={diffData.diff.addedBduVulnerablePackages}
                      type="danger"
                      isVuln
                    />
                    <DiffList
                      title={t("snapshots.removed_bdu")}
                      items={diffData.diff.removedBduVulnerablePackages}
                      type="success"
                      isVuln
                    />
                  </div>
                )}
              </section>

              {/* PAYLOAD ACCORDION */}
              <section className="border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
                <button
                  onClick={() => setShowFullPayload(!showFullPayload)}
                  className="w-full flex justify-between items-center p-4 bg-gray-50 dark:bg-gray-800 hover:bg-gray-100 dark:hover:bg-gray-750 transition-colors outline-none focus:ring-2 focus:ring-blue-500/50"
                >
                  <span className="font-semibold text-gray-800 dark:text-gray-200 tracking-tight">
                    {t("snapshots.full_payload")}
                  </span>
                  {showFullPayload ? (
                    <ChevronUp size={20} />
                  ) : (
                    <ChevronDown size={20} />
                  )}
                </button>

                {isPayloadLoading ? (
                  <p className="text-center text-gray-500 py-6">
                    {t("common.loading")}
                  </p>
                ) : showFullPayload ? (
                  payloadData?.payload == null ? (
                    <p className="text-center text-gray-500 italic py-6 bg-white dark:bg-gray-900">
                      {t("snapshots.no_payload_data")}
                    </p>
                  ) : (
                    <div className="p-5 space-y-6 bg-white dark:bg-gray-900 grid grid-cols-1 lg:grid-cols-2 gap-4">
                      <PayloadList
                        title={t("snapshots.total_packages")}
                        items={payloadData.payload.packages}
                        className="lg:col-span-2"
                      />
                      <PayloadList
                        title={t("snapshots.total_cve")}
                        items={payloadData.payload.vulnerablePackages}
                        isVuln
                      />
                      <PayloadList
                        title={t("snapshots.total_bdu")}
                        items={payloadData.payload.bduVulnerablePackages}
                        isVuln
                      />
                    </div>
                  )
                ) : null}
              </section>
            </>
          )}
        </div>
      </div>
    </div>
  );
};

const StatusBadge: React.FC<{ status: VulnerablePackageStatus }> = ({
  status,
}) => {
  const config: Record<
    VulnerablePackageStatus,
    { bg: string; text: string; label: string }
  > = {
    unknown: {
      bg: "bg-gray-100 dark:bg-gray-800",
      text: "text-gray-600 dark:text-gray-400",
      label: "Unknown",
    },
    vulnerable: {
      bg: "bg-red-100 dark:bg-red-900/40",
      text: "text-red-700 dark:text-red-400",
      label: "Vulnerable",
    },
    falsePositive: {
      bg: "bg-yellow-100 dark:bg-yellow-900/40",
      text: "text-yellow-700 dark:text-yellow-400",
      label: "False Positive",
    },
    patchless: {
      bg: "bg-purple-100 dark:bg-purple-900/40",
      text: "text-purple-700 dark:text-purple-400",
      label: "Patchless",
    },
    fixed: {
      bg: "bg-green-100 dark:bg-green-900/40",
      text: "text-green-700 dark:text-green-400",
      label: "Fixed",
    },
  };
  const { bg, text, label } = config[status] || config.unknown;

  return (
    <span
      className={`px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider whitespace-nowrap ${bg} ${text}`}
    >
      {label}
    </span>
  );
};

const PackageListItem: React.FC<{ item: PackageInfo }> = ({ item }) => (
  <li className="flex justify-between items-center py-2 border-b border-black/5 dark:border-white/5 last:border-0">
    <span
      className="font-medium text-gray-800 dark:text-gray-200 truncate pr-2"
      title={`${item.name}`}
    >
      {item.name}
    </span>
    <span className="text-gray-500 dark:text-gray-400 font-mono text-xs bg-black/5 dark:bg-white/5 px-1.5 py-0.5 rounded">
      v{item.version}
    </span>
  </li>
);

const VulnListItem: React.FC<{ item: VulnerablePackage }> = ({ item }) => (
  <li className="flex flex-col py-2.5 border-b border-black/5 dark:border-white/5 last:border-0 gap-1.5">
    <div className="flex justify-between items-start gap-2">
      <span className="font-bold text-gray-900 dark:text-gray-100 break-all">
        {item.vulnerabilityId}
      </span>
      <StatusBadge status={item.status} />
    </div>
    <div className="flex justify-between items-center text-xs text-gray-500 dark:text-gray-400 mt-0.5">
      <span
        className="truncate pr-2"
        title={`Package ID: ${item.packageInfoId}. Name: ${item.packageName}`}
      >
        {item.packageName}
      </span>
      <span className="font-mono bg-black/5 dark:bg-white/5 px-1.5 py-0.5 rounded whitespace-nowrap">
        v{item.packageVersion}
      </span>
    </div>
  </li>
);

interface ListBaseProps { title: string; className?: string };
type VulnListProps = ListBaseProps & {
  isVuln: true;
  items: VulnerablePackage[];
};
type PkgListProps = ListBaseProps & { isVuln?: false; items: PackageInfo[] };

type DiffListProps = (VulnListProps | PkgListProps) & {
  type: "success" | "danger";
};
type PayloadListProps = VulnListProps | PkgListProps;

const DiffList = (props: DiffListProps) => {
  if (!props.items || props.items.length === 0) return null;

  const bgClasses =
    props.type === "success"
      ? "bg-green-50/50 dark:bg-green-900/10 border-green-200/60 dark:border-green-900/40"
      : "bg-red-50/50 dark:bg-red-900/10 border-red-200/60 dark:border-red-900/40";

  const textClasses =
    props.type === "success"
      ? "text-green-800 dark:text-green-400"
      : "text-red-800 dark:text-red-400";

  return (
    <div
      className={`p-4 rounded-xl border ${bgClasses} ${props.className ?? ""}`}
    >
      <h5
        className={`font-semibold text-sm mb-3 flex items-center justify-between ${textClasses}`}
      >
        {props.title}
        <span className="bg-white/50 dark:bg-black/20 px-2 py-0.5 rounded-full text-xs font-bold">
          {props.items.length}
        </span>
      </h5>
      <ul className="text-sm max-h-48 overflow-y-auto pr-2 custom-scrollbar">
        {props.isVuln
          ? props.items.map((item) => (
              <VulnListItem key={item.id} item={item} />
            ))
          : props.items.map((item) => (
              <PackageListItem key={item.id} item={item} />
            ))}
      </ul>
    </div>
  );
};

const PayloadList = (props: PayloadListProps) => {
  if (!props.items || props.items.length === 0) return null;

  return (
    <div
      className={`bg-gray-50/50 dark:bg-gray-800/30 border border-gray-200 dark:border-gray-800 rounded-xl p-4 ${props.className ?? ""}`}
    >
      <h5 className="font-semibold text-sm text-gray-700 dark:text-gray-300 mb-3 flex items-center justify-between">
        {props.title}
        <span className="bg-gray-200 dark:bg-gray-700 text-gray-700 dark:text-gray-300 py-0.5 px-2 rounded-full text-xs font-bold">
          {props.items.length}
        </span>
      </h5>
      <ul className="text-sm max-h-60 overflow-y-auto pr-2 custom-scrollbar">
        {props.isVuln
          ? props.items.map((item) => (
              <VulnListItem key={item.id} item={item} />
            ))
          : props.items.map((item) => (
              <PackageListItem key={item.id} item={item} />
            ))}
      </ul>
    </div>
  );
};
