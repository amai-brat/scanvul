import { ChevronDown, ChevronUp } from "lucide-react";
import { type ReactNode } from "react";

export const AccordionBlock = ({
  isOpen,
  setIsOpen,
  header,
  body,
}: {
  isOpen: boolean;
  setIsOpen: (open: boolean) => void;
  header: ReactNode;
  body: ReactNode;
}) => {
  return (
    <div className="md:col-span-2 lg:col-span-3 bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl overflow-hidden shadow-sm">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="w-full flex items-center justify-between p-6 hover:bg-gray-50 dark:hover:bg-gray-800/50 transition-colors text-left"
      >
        <div className="space-y-1">{header}</div>
        {isOpen ? (
          <ChevronUp className="w-5 h-5 text-gray-400" />
        ) : (
          <ChevronDown className="w-5 h-5 text-gray-400" />
        )}
      </button>
      {isOpen && (
        <div className="border-t border-gray-100 dark:border-gray-800">
          {body}
        </div>
      )}
    </div>
  );
};
