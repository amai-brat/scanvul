import type React from "react";

export const InfoRow = ({
  icon, 
  label, 
  value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
}) => (
  <div className="flex items-center gap-3">
    <div className="text-gray-400 w-5 flex justify-center">{icon}</div>
    <div>
      <p className="text-xs text-gray-500 uppercase font-semibold">{label}</p>
      <p className="font-medium text-sm">{value}</p>
    </div>
  </div>
);
