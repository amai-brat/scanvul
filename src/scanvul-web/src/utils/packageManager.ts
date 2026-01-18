export type PackageManager = "unknown" | "choco" | "pacman" | "rpm";

export const getPackageManager = (operatingSystem: string) : PackageManager => {
  const os = operatingSystem.trim().toLowerCase()

  if (os.startsWith("win")) return "choco";
  if (os.startsWith("arch")) return "pacman";
  if (os.startsWith("alt")) return "rpm";

  return "unknown";
}