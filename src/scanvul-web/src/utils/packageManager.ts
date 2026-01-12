export type PackageManager = "unknown" | "choco" | "pacman" | "rpm";

export const getPackageManager = (operationSystem: string) : PackageManager => {
  const os = operationSystem.trim().toLowerCase()

  if (os.startsWith("win")) return "choco";
  if (os.startsWith("arch")) return "pacman";
  if (os.startsWith("alt")) return "rpm";

  return "unknown";
}