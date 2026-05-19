export type PackageManager = "unknown" | "choco" | "pacman" | "rpm" | "winget";

export const getPackageManagers = (operatingSystem: string) : PackageManager[] => {
  const os = operatingSystem.trim().toLowerCase()

  if (os.startsWith("win")) return ["winget", "choco"];
  if (os.startsWith("arch")) return ["pacman"];
  if (os.startsWith("alt")) return ["rpm"];

  return [];
}

export const isVersionsSupported = (packageManager: PackageManager) : boolean => {
  return ["winget", "choco"].includes(packageManager);
}