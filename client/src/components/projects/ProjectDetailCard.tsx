import { useState } from "react";
import type { Project } from "../../types";
import EditProjectModal from "./EditProjectModal";
import Button from "../ui/Button";
import api from "../../services/api";
import ConfirmModal from "../ui/ConfirmModal";
import ShareProjectModal from "./ShareProjectModal";
import { useAuth } from "../../context/AuthContext";

interface ProjectDetailCardProps {
  project: Project;
  onProjectUpdated: (project: Project) => void;
  onProjectDeleted: () => void;
}

export default function ProjectDetailCard({ project, onProjectUpdated, onProjectDeleted }: ProjectDetailCardProps) {
  const { user } = useAuth();
  const isOwner = user?.id === project.ownerId;

  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [isShareModalOpen, setIsShareModalOpen] = useState(false);

  const completionPercentage =
    project.taskCount === 0
      ? 0
      : Math.round((project.completedTaskCount / project.taskCount) * 100);

  const handleDelete = async () => {
    setIsDeleting(true);
    try {
        await api.delete(`/api/projects/${project.id}`);
        onProjectDeleted();
    } catch {
        console.error("Failed to delete project");
    } finally {
        setIsDeleting(false);
    }
  };

  return (
    <>
      <div className="bg-brand-paper border border-brand-border rounded-2xl p-6 mb-6">
        {/* Header */}
        <div className="flex items-start justify-between mb-6">
          <div className="flex items-start gap-4">
            {project.emoji && (
              <span className="text-5xl">{project.emoji}</span>
            )}
            <div>
              <h2 className="font-display text-3xl font-bold text-brand-text">
                {project.title}
              </h2>
              {project.description && (
                <p className="text-brand-text-light mt-1 font-sans">
                  {project.description}
                </p>
              )}
            </div>
          </div>
          <div className="flex gap-2">
            {isOwner && (
                <Button variant="ghost" size="sm" onClick={() => setIsEditModalOpen(true)}>
                Edit
                </Button>
            )}
            {isOwner && (
                <Button variant="ghost" size="sm" onClick={() => setIsShareModalOpen(true)}>
                Share
                </Button>
            )}
            {isOwner && (
                <Button variant="danger" size="sm" onClick={() => setIsDeleteModalOpen(true)}>
                Delete
                </Button>
            )}
          </div>
          </div>

        {/* Stats */}
        <div className="grid grid-cols-3 gap-4 mb-6">
          <div className="bg-brand-bg rounded-xl p-4 border border-brand-border">
            <p className="text-2xl font-bold text-brand-text font-display">
              {project.taskCount}
            </p>
            <p className="text-xs text-brand-text-light mt-1">Total Tasks</p>
          </div>
          <div className="bg-brand-bg rounded-xl p-4 border border-brand-border">
            <p className="text-2xl font-bold text-brand-text font-display">
              {project.completedTaskCount}
            </p>
            <p className="text-xs text-brand-text-light mt-1">Completed</p>
          </div>
          <div className="bg-brand-bg rounded-xl p-4 border border-brand-border">
            <p className="text-2xl font-bold text-brand-text font-display">
              {project.taskCount - project.completedTaskCount}
            </p>
            <p className="text-xs text-brand-text-light mt-1">Remaining</p>
          </div>
        </div>

        {/* Progress bar */}
        <div>
          <div className="flex justify-between items-center mb-2">
            <span className="text-sm text-brand-text-light">Overall Progress</span>
            <span className="text-sm font-medium text-brand-text">
              {completionPercentage}%
            </span>
          </div>
          <div className="w-full bg-brand-secondary rounded-full h-3">
            <div
              className="bg-brand-primary rounded-full h-3 transition-all"
              style={{ width: `${completionPercentage}%` }}
            />
          </div>
        </div>
      </div>

      <EditProjectModal
        isOpen={isEditModalOpen}
        onClose={() => setIsEditModalOpen(false)}
        project={project}
        onProjectUpdated={onProjectUpdated}
      />

      <ConfirmModal
        isOpen={isDeleteModalOpen}
        onClose={() => setIsDeleteModalOpen(false)}
        onConfirm={handleDelete}
        title="Delete Project"
        message="Are you sure you want to delete this project? All tasks and comments will be permanently deleted."
        isLoading={isDeleting}
      />

      <ShareProjectModal
        isOpen={isShareModalOpen}
        onClose={() => setIsShareModalOpen(false)}
        projectId={project.id}
      />
    </>
  );
}
