import { useNavigate } from "react-router-dom";
import type { Task } from "../../types";
import api from "../../services/api";

interface TaskRowProps {
  task: Task;
  onTaskUpdated: (task: Task) => void;
}

const priorityColors: Record<string, string> = {
  Low: "bg-priority-low text-green-800",
  Medium: "bg-priority-medium text-yellow-800",
  High: "bg-priority-high text-red-800",
  Urgent: "bg-priority-urgent text-red-900",
};

const statusColors: Record<string, string> = {
  Todo: "bg-status-todo text-gray-700",
  "In Progress": "bg-status-in-progress text-blue-800",
  Done: "bg-status-done text-green-800",
};

export default function TaskRow({ task, onTaskUpdated }: TaskRowProps) {
  const navigate = useNavigate();
  const isDone = task.status === "Done";

  const handleToggleComplete = async (e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      const { data } = await api.patch(`/api/tasks/${task.id}`, {
        status: isDone ? "Todo" : "Done",
        completedAt: isDone ? null : new Date().toISOString(),
      });
      onTaskUpdated(data);
    } catch {
      console.error("Failed to update task");
    }
  };

  return (
    <tr
      onClick={() => navigate(`/tasks/${task.id}`)}
      className="border-b border-brand-border hover:bg-brand-secondary cursor-pointer transition-all"
    >
      {/* Checkbox */}
      <td className="py-3 px-4 w-10">
        <div
          onClick={handleToggleComplete}
          className={`w-5 h-5 rounded-full border-2 flex items-center justify-center transition-all cursor-pointer ${
            isDone
              ? "bg-brand-primary border-brand-primary"
              : "border-brand-border hover:border-brand-primary"
          }`}
        >
          {isDone && <span className="text-white text-xs">✓</span>}
        </div>
      </td>

      <td className="py-3 px-4">
        <span className={`font-sans text-brand-text ${isDone ? "line-through text-brand-text-light" : ""}`}>
          {task.title}
        </span>
      </td>
      <td className="py-3 px-4">
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${statusColors[task.status] ?? "bg-gray-100 text-gray-700"}`}>
          {task.status}
        </span>
      </td>
      <td className="py-3 px-4">
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${priorityColors[task.priority] ?? "bg-gray-100 text-gray-700"}`}>
          {task.priority}
        </span>
      </td>
      <td className="py-3 px-4">
        <span className="text-sm text-brand-text-light">
          {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : "—"}
        </span>
      </td>
    </tr>
  );
}
