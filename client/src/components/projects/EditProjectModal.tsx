import { useState } from "react";
import type { Project } from "../../types";
import api from "../../services/api";
import Modal from "../ui/Modal";
import Input from "../ui/Input";
import Button from "../ui/Button";

interface EditProjectModalProps {
  isOpen: boolean;
  onClose: () => void;
  project: Project;
  onProjectUpdated: (project: Project) => void;
}

const EMOJI_OPTIONS = ["📝", "🚀", "🎯", "💡", "🏠", "💪", "🎨", "📚", "🌱", "✈️", "🎵", "🍕", "💻", "🏋️"];

export default function EditProjectModal({
  isOpen,
  onClose,
  project,
  onProjectUpdated,
}: EditProjectModalProps) {
  const [title, setTitle] = useState(project.title);
  const [description, setDescription] = useState(project.description ?? "");
  const [selectedEmoji, setSelectedEmoji] = useState(project.emoji ?? "");
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
      const { data } = await api.patch(`/api/projects/${project.id}`, {
        title: title.trim(),
        description: description.trim() || null,
        emoji: selectedEmoji || null,
      });

      onProjectUpdated({ ...project, ...data });
      onClose();
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Something went wrong");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Edit Project">
      <div className="space-y-4">

        {/* Emoji picker */}
        <div>
          <label className="text-sm font-medium text-brand-text block mb-2">
            Pick an emoji
          </label>
          <div className="flex gap-2 flex-wrap">
            {EMOJI_OPTIONS.map((emoji) => (
              <button
                key={emoji}
                onClick={() => setSelectedEmoji(emoji === selectedEmoji ? "" : emoji)}
                className={`text-2xl p-2 rounded-xl transition-all ${
                  selectedEmoji === emoji
                    ? "bg-brand-secondary border-2 border-brand-primary"
                    : "hover:bg-brand-secondary border-2 border-transparent"
                }`}
              >
                {emoji}
              </button>
            ))}
          </div>
        </div>

        <Input
          label="Title"
          value={title}
          onChange={setTitle}
          placeholder="My awesome project"
        />

        <Input
          label="Description"
          value={description}
          onChange={setDescription}
          placeholder="What's this project about?"
          type="textarea"
        />

        {error && <p className="text-brand-error text-sm">{error}</p>}

        <div className="flex gap-3 pt-2">
          <Button variant="secondary" onClick={onClose} fullWidth>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={isLoading} fullWidth>
            {isLoading ? "Saving..." : "Save Changes"}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
