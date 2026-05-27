import { useState } from "react";
import type { Task, ProjectMemberResponse } from "../../types";
import TaskRow from "./TaskRow";
import Button from "../ui/Button";
import TaskFilters from "./TaskFilters";

interface TaskListProps {
  tasks: Task[];
  onAddTask: () => void;
  onTaskUpdated: (task: Task) => void;
  members: ProjectMemberResponse[];
  status: string[];
  priority: string[];
  createdById: string[];
  sortBy: string;
  sortOrder: string;
  onFilterChange: (key: string, value: string) => void;
  onReset: () => void;
}

export default function TaskList({
  tasks,
  onAddTask,
  onTaskUpdated,
  members,
  status,
  priority,
  createdById,
  sortBy,
  sortOrder,
  onFilterChange,
  onReset,
}: TaskListProps) {
  const [filtersOpen, setFiltersOpen] = useState(false);

  return (
    <div className="bg-brand-paper border border-brand-border rounded-2xl overflow-hidden">
      {/* Header */}
      <div className="flex items-center justify-between px-6 py-4 border-b border-brand-border">
        <h3 className="font-display text-lg font-bold text-brand-text">Tasks</h3>
        <Button size="sm" onClick={onAddTask}>+ Add Task</Button>
      </div>

      {/* Filters toggle */}
      <div
        className="px-6 py-3 border-b border-brand-border bg-brand-bg flex items-center justify-between cursor-pointer hover:bg-brand-secondary transition-all"
        onClick={() => setFiltersOpen((prev) => !prev)}
      >
        <span className="text-xs font-medium text-brand-text-light uppercase tracking-wider">
          Filters {status.length + priority.length + createdById.length > 0 && (
            <span className="ml-1 bg-brand-primary text-white rounded-full px-1.5 py-0.5 text-xs">
              {status.length + priority.length + createdById.length}
            </span>
          )}
        </span>
        <span className="text-brand-text-light text-xs">
          {filtersOpen ? "↑ Hide" : "↓ Show"}
        </span>
      </div>

      {/* Filters content */}
      {filtersOpen && (
        <div className="px-6 py-3 border-b border-brand-border bg-brand-bg">
          <TaskFilters
            status={status}
            priority={priority}
            createdById={createdById}
            members={members}
            onFilterChange={onFilterChange}
            onReset={onReset}
          />
        </div>
      )}

      {/* Table */}
      {tasks.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-3xl mb-3">✅</p>
          <p className="font-display text-lg font-bold text-brand-text mb-1">No tasks found</p>
          <p className="text-brand-text-light text-sm">Try adjusting your filters</p>
        </div>
      ) : (
        <table className="w-full">
          <thead>
            <tr className="border-b border-brand-border bg-brand-bg">
              <th className="py-3 px-4 w-10"></th>
              <th className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider">
                Task
              </th>
              <th
                className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider cursor-pointer hover:text-brand-text"
                onClick={() => {
                  onFilterChange("sortBy", "status");
                  if (sortBy === "status") onFilterChange("sortOrder", sortOrder === "asc" ? "desc" : "asc");
                }}
              >
                Status {sortBy === "status" ? (sortOrder === "asc" ? "↑" : "↓") : "↕"}
              </th>
              <th
                className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider cursor-pointer hover:text-brand-text"
                onClick={() => {
                  onFilterChange("sortBy", "priority");
                  if (sortBy === "priority") onFilterChange("sortOrder", sortOrder === "asc" ? "desc" : "asc");
                }}
              >
                Priority {sortBy === "priority" ? (sortOrder === "asc" ? "↑" : "↓") : "↕"}
              </th>
              <th
                className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider cursor-pointer hover:text-brand-text"
                onClick={() => {
                  onFilterChange("sortBy", "duedate");
                  if (sortBy === "duedate") onFilterChange("sortOrder", sortOrder === "asc" ? "desc" : "asc");
                }}
              >
                Due Date {sortBy === "duedate" ? (sortOrder === "asc" ? "↑" : "↓") : "↕"}
              </th>
              <th
                className="text-left py-3 px-4 text-xs font-medium text-brand-text-light uppercase tracking-wider cursor-pointer hover:text-brand-text"
                onClick={() => {
                  onFilterChange("sortBy", "createdby");
                  if (sortBy === "createdby") onFilterChange("sortOrder", sortOrder === "asc" ? "desc" : "asc");
                }}
              >
                Created By {sortBy === "createdby" ? (sortOrder === "asc" ? "↑" : "↓") : "↕"}
              </th>
            </tr>
          </thead>
          <tbody>
            {tasks.map((task) => (
              <TaskRow key={task.id} task={task} onTaskUpdated={onTaskUpdated} />
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
