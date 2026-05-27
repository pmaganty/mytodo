import { useState } from "react";
import type { Task } from "../../types";
import api from "../../services/api";
import Input from "../ui/Input";
import Button from "../ui/Button";
import ConfirmModal from "../ui/ConfirmModal";

interface TaskDetailCardProps {
  task: Task;
  onTaskUpdated: (task: Task) => void;
  onTaskDeleted: () => void;
}

const PRIORITIES = ["Low", "Medium", "High", "Urgent"];
const STATUSES = ["Todo", "In Progress", "Done"];

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

export default function TaskDetailCard({ task, onTaskUpdated, onTaskDeleted }: TaskDetailCardProps) {
  const [title, setTitle] = useState(task.title);
  const [description, setDescription] = useState(task.description ?? "");
  const [dueDate, setDueDate] = useState(
    task.dueDate ? task.dueDate.split("T")[0] : ""
  );
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const handleUpdate = async (fields: Partial<Task>) => {
    setIsSaving(true);
    try {
      const { data } = await api.patch(`/api/tasks/${task.id}`, {
        title,
        description: description || null,
        priority: task.priority,
        status: task.status,
        dueDate: dueDate || null,
        ...fields,
      });
      onTaskUpdated(data);
    } catch {
      console.error("Failed to update task");
    } finally {
      setIsSaving(false);
    }
  };

  const handleStatusChange = (status: string) => {
    handleUpdate({
      status,
      completedAt: status === "Done" ? new Date().toISOString() : null,
    });
  };

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
      await api.delete(`/api/tasks/${task.id}`);
      onTaskDeleted();
    } catch {
      console.error("Failed to delete task");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="bg-brand-paper border border-brand-border rounded-2xl p-6 mb-6">

        {/* Title + Delete button */}
        <div className="flex items-start justify-between mb-6">
          <div className="flex-1">
            <input
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onBlur={() => handleUpdate({ title })}
              className="font-display text-3xl font-bold text-brand-text bg-transparent border-b-2 border-transparent hover:border-brand-border focus:border-brand-primary outline-none w-full transition-all pb-1"
            />
            {isSaving && (
              <p className="text-xs text-brand-text-light mt-1">Saving...</p>
            )}
          </div>
          <Button
            variant="danger"
            size="sm"
            onClick={() => setIsDeleteModalOpen(true)}
          >
            Delete
          </Button>
        </div>

        {/* Status + Priority */}
        <div className="grid grid-cols-2 gap-6 mb-6">
          <div>
            <label className="text-xs font-medium text-brand-text-light uppercase tracking-wider block mb-2">
              Status
            </label>
            <div className="flex gap-2 flex-wrap">
              {STATUSES.map((s) => (
                <button
                  key={s}
                  onClick={() => handleStatusChange(s)}
                  className={`px-3 py-1.5 rounded-full text-xs font-medium transition-all border-2 ${
                    task.status === s
                      ? `${statusColors[s]} border-brand-primary`
                      : "bg-brand-bg text-brand-text-light border-transparent hover:border-brand-border"
                  }`}
                >
                  {s}
                </button>
              ))}
            </div>
          </div>

          <div>
            <label className="text-xs font-medium text-brand-text-light uppercase tracking-wider block mb-2">
              Priority
            </label>
            <div className="flex gap-2 flex-wrap">
              {PRIORITIES.map((p) => (
                <button
                  key={p}
                  onClick={() => handleUpdate({ priority: p })}
                  className={`px-3 py-1.5 rounded-full text-xs font-medium transition-all border-2 ${
                    task.priority === p
                      ? `${priorityColors[p]} border-brand-primary`
                      : "bg-brand-bg text-brand-text-light border-transparent hover:border-brand-border"
                  }`}
                >
                  {p}
                </button>
              ))}
            </div>
          </div>
        </div>

        {/* Due Date */}
        <div className="mb-6">
          <Input
            label="Due Date"
            value={dueDate}
            onChange={setDueDate}
            onBlur={() => handleUpdate({ dueDate: dueDate || null })}
            type="date"
          />
        </div>

        {/* Description */}
        <div>
          <label className="text-xs font-medium text-brand-text-light uppercase tracking-wider block mb-2">
            Description
          </label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            onBlur={() => handleUpdate({ description: description || null })}
            placeholder="Add a description..."
            rows={4}
            className="w-full px-4 py-2.5 rounded-xl border border-brand-border bg-brand-bg text-brand-text placeholder-brand-text-light focus:outline-none focus:ring-2 focus:ring-brand-primary text-sm resize-none transition-all"
          />
        </div>

        {/* Metadata */}
        <div className="mt-6 pt-4 border-t border-brand-border flex gap-6">
          <div>
            <p className="text-xs text-brand-text-light">Created</p>
            <p className="text-sm text-brand-text">
              {new Date(task.createdAt).toLocaleDateString()}
            </p>
          </div>
          {task.completedAt && (
            <div>
              <p className="text-xs text-brand-text-light">Completed</p>
              <p className="text-sm text-brand-text">
                {new Date(task.completedAt).toLocaleDateString()}
              </p>
            </div>
          )}
        </div>
      </div>

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        onConfirm={handleDelete}
        title="Delete Task"
        message="Are you sure you want to delete this task? This action cannot be undone."
        isLoading={isDeleting}
      />
    </>
  );
}
