import type { VulnerablePackageResponse } from "../api/agentsApi";


export type SeverityLevel = "CRITICAL" | "HIGH" | "MEDIUM" | "LOW";
export const getCvssScore = (v: VulnerablePackageResponse): number => {
  return v.cvssV3_1 ?? v.cvssV3_0 ?? v.cvssV2_0 ?? 0;
};
export const getSeverityLevel = (score: number): SeverityLevel => {
  if (score >= 9.0) return "CRITICAL";
  if (score >= 7.0) return "HIGH";
  if (score >= 4.0) return "MEDIUM";
  return "LOW";
};
export const SEVERITY_CONFIG: Record<SeverityLevel, {
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
