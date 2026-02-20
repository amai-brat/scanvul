import { AgentsList } from "./components/AgentsList";
import { ReportsList } from "./components/ReportsList";

export const MainPage = () => {
  return (
    <div className="flex flex-col gap-12">
      <ReportsList />
      <AgentsList />
    </div>
  );
}