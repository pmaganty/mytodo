import { useAuth } from "../../context/AuthContext";
import type { ProjectMemberResponse } from "../../types";

interface TaskFiltersProps {
  status: string[];
  priority: string[];
  createdById: string[];
  members: ProjectMemberResponse[];
  onFilterChange: (key: string, value: string) => void;
  onReset: () => void;
}

const STATUSES = ["Todo", "In Progress", "Done"];
const PRIORITIES = ["Low", "Medium", "High", "Urgent"];

export default function TaskFilters({
  status,
  priority,
  createdById,
  members,
  onFilterChange,
  onReset,
}: TaskFiltersProps) {
  const { user } = useAuth();

  const hasActiveFilters = status.length > 0 || priority.length > 0 || createdById.length > 0;

  const allMembers = [
    { userId: user?.id ?? "", name: "Me" },
    ...members,
  ];

  return (
    <div className="space-y-3">
      {/* Status */}
      <div className="flex items-center gap-2 flex-wrap">
        <span className="text-xs font-medium text-brand-text-light w-16">Status</span>
        {STATUSES.map((s) => (
          <button
            key={s}
            onClick={() => onFilterChange("status", s)}
            className={`px-3 py-1 rounded-full text-xs font-medium border transition-all ${
              status.includes(s)
                ? "bg-brand-primary text-white border-brand-primary"
                : "bg-brand-bg text-brand-text-light border-brand-border hover:border-brand-primary"
            }`}
          >
            {s}
          </button>
        ))}
      </div>

      {/* Priority */}
      <div className="flex items-center gap-2 flex-wrap">
        <span className="text-xs font-medium text-brand-text-light w-16">Priority</span>
        {PRIORITIES.map((p) => (
          <button
            key={p}
            onClick={() => onFilterChange("priority", p)}
            className={`px-3 py-1 rounded-full text-xs font-medium border transition-all ${
              priority.includes(p)
                ? "bg-brand-primary text-white border-brand-primary"
                : "bg-brand-bg text-brand-text-light border-brand-border hover:border-brand-primary"
            }`}
          >
            {p}
          </button>
        ))}
      </div>

      {/* Created By */}
      <div className="flex items-center gap-2 flex-wrap">
        <span className="text-xs font-medium text-brand-text-light w-16">Created</span>
        {allMembers.map((m) => (
          <button
            key={m.userId}
            onClick={() => onFilterChange("createdById", m.userId)}
            className={`px-3 py-1 rounded-full text-xs font-medium border transition-all ${
              createdById.includes(m.userId)
                ? "bg-brand-primary text-white border-brand-primary"
                : "bg-brand-bg text-brand-text-light border-brand-border hover:border-brand-primary"
            }`}
          >
            {m.name}
          </button>
        ))}
      </div>

      {/* Reset */}
      {hasActiveFilters && (
        <button
          onClick={onReset}
          className="text-xs text-brand-error hover:opacity-75 transition-all"
        >
          Reset all
        </button>
      )}
    </div>
  );
}
