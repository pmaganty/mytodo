import { useState } from "react";
import type { Task } from "../../types";
import api from "../../services/api";
import Modal from "../ui/Modal";
import Input from "../ui/Input";
import Button from "../ui/Button";

interface CreateTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  projectId: string;
  onTaskCreated: (task: Task) => void;
}

const PRIORITIES = ["Low", "Medium", "High", "Urgent"];
const STATUSES = ["Todo", "In Progress", "Done"];

export default function CreateTaskModal({
  isOpen,
  onClose,
  projectId,
  onTaskCreated,
}: CreateTaskModalProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState("Medium");
  const [status, setStatus] = useState("Todo");
  const [dueDate, setDueDate] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async () => {
    if (!title.trim()) {
      setError("Title is required");
      return;
    }

    setError("");
    setIsLoading(true);

    try {
      const { data } = await api.post(`/api/projects/${projectId}/tasks`, {
        title: title.trim(),
        description: description.trim() || null,
        priority,
        status,
        dueDate: dueDate || null,
      });

      onTaskCreated(data);
      handleClose();
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Something went wrong");
    } finally {
      setIsLoading(false);
    }
  };

  const handleClose = () => {
    setTitle("");
    setDescription("");
    setPriority("Medium");
    setStatus("Todo");
    setDueDate("");
    setError("");
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="New Task">
      <div className="space-y-4">
        <Input
          label="Title"
          value={title}
          onChange={setTitle}
          placeholder="What needs to be done?"
        />

        <Input
          label="Description"
          value={description}
          onChange={setDescription}
          placeholder="Add some details..."
          type="textarea"
        />

        {/* Priority */}
        <div>
          <label className="text-sm font-medium text-brand-text block mb-2">
            Priority
          </label>
          <div className="flex gap-2">
            {PRIORITIES.map((p) => (
              <button
                key={p}
                onClick={() => setPriority(p)}
                className={`flex-1 py-2 rounded-xl text-sm font-medium border-2 transition-all ${
                  priority === p
                    ? "border-brand-primary bg-brand-secondary text-brand-text"
                    : "border-brand-border text-brand-text-light hover:border-brand-primary"
                }`}
              >
                {p}
              </button>
            ))}
          </div>
        </div>

        {/* Status */}
        <div>
          <label className="text-sm font-medium text-brand-text block mb-2">
            Status
          </label>
          <div className="flex gap-2">
            {STATUSES.map((s) => (
              <button
                key={s}
                onClick={() => setStatus(s)}
                className={`flex-1 py-2 rounded-xl text-sm font-medium border-2 transition-all ${
                  status === s
                    ? "border-brand-primary bg-brand-secondary text-brand-text"
                    : "border-brand-border text-brand-text-light hover:border-brand-primary"
                }`}
              >
                {s}
              </button>
            ))}
          </div>
        </div>

        <Input
          label="Due Date"
          value={dueDate}
          onChange={setDueDate}
          type="date"
        />

        {error && <p className="text-brand-error text-sm">{error}</p>}

        <div className="flex gap-3 pt-2">
          <Button variant="secondary" onClick={handleClose} fullWidth>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={isLoading} fullWidth>
            {isLoading ? "Creating..." : "Create Task"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
