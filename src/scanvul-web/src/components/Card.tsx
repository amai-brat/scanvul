import React from "react";

export const Card = ({
  title,
  children,
  className = "",
}: {
  title: string;
  children: React.ReactNode;
  className?: string;
}) => (
  <div
    className={`bg-card text-card-foreground rounded-lg border border-gray-200 dark:border-gray-800 shadow-sm overflow-hidden flex flex-col ${className}`}
  >
    <div className="px-6 py-4 border-b border-gray-100 dark:border-gray-800 bg-gray-50/50 dark:bg-gray-800/50">
      <h3 className="font-semibold text-lg">{title}</h3>
    </div>
    <div className="p-4 flex-1 overflow-auto">{children}</div>
  </div>
);
