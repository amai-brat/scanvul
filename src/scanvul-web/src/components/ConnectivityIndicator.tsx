import { differenceInMinutes, parseISO } from "date-fns";
import { WifiOff } from "lucide-react";

interface ConnectivityIndicatorProps {
  lastPingAt: string;
}

export const ConnectivityIndicator = ({
  lastPingAt,
}: ConnectivityIndicatorProps) => {
  const date = parseISO(lastPingAt);
  const now = new Date();
  const diffMinutes = differenceInMinutes(now, date);

  // Logic for signal strength
  // Level 4: < 5 mins (Full Green)
  // Level 3: < 10 mins (Missing outer arc, Yellow/Orange)
  // Level 2: < 1 hour (Missing two outer arcs, Orange)
  // Level 1: < 1 week (Dot only, Red)
  // Level 0: > 1 week (Offline Icon, Red)

  let signalLevel = 0;
  let colorClass = "text-red-500";

  if (diffMinutes <= 5) {
    signalLevel = 4;
    colorClass = "text-green-500 dark:text-green-400";
  } else if (diffMinutes <= 10) {
    signalLevel = 3;
    colorClass = "text-yellow-500 dark:text-yellow-400";
  } else if (diffMinutes <= 60) {
    signalLevel = 2;
    colorClass = "text-orange-500 dark:text-orange-400";
  } else if (diffMinutes <= 10080) {
    // 1 week
    signalLevel = 1;
    colorClass = "text-red-500 dark:text-red-400";
  } else {
    signalLevel = 0;
    colorClass = "text-red-600 dark:text-red-500";
  }

  const title = `Last ping: ${date.toLocaleString()} (${diffMinutes} mins ago)`;

  if (signalLevel === 0) {
    return (
      <div title={title} className={colorClass}>
        <WifiOff className="w-6 h-6" />
      </div>
    );
  }

  // Custom SVG to match Lucide style (24x24, stroke 2, round caps)
  return (
    <div
      title={title}
      className={`${colorClass} transition-colors duration-300`}
    >
      <svg
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
        {/* Dot */}
        <path d="M12 20h.01" />

        {/* Inner Arc (Level 2+) */}
        <path
          d="M8.5 16.429a5 5 0 0 1 7 0"
          className={signalLevel < 2 ? "opacity-20" : ""}
        />

        {/* Middle Arc (Level 3+) */}
        <path
          d="M5 12.859a10 10 0 0 1 14 0"
          className={signalLevel < 3 ? "opacity-20" : ""}
        />

        {/* Outer Arc (Level 4 only) */}
        <path
          d="M2 8.571a16 16 0 0 1 20 0"
          className={signalLevel < 4 ? "opacity-20" : ""}
        />
      </svg>
    </div>
  );
};
