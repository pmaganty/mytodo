import { useNavigate } from "react-router-dom";
import type { Project } from "../../types";
import Card from "../ui/Card";

interface ProjectCardProps {
  project: Project;
}

export default function ProjectCard({ project }: ProjectCardProps) {
  const navigate = useNavigate();

  const completionPercentage =
    project.taskCount === 0
      ? 0
      : Math.round((project.completedTaskCount / project.taskCount) * 100);

  return (
    <Card hoverable onClick={() => navigate(`/projects/${project.id}`)}>
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div>
          {project.emoji && (
            <span className="text-3xl mb-2 block">{project.emoji}</span>
          )}
          <h2 className="font-display text-xl font-bold text-brand-text">
            {project.title}
          </h2>
          {project.description && (
            <p className="text-brand-text-light text-sm mt-1 line-clamp-2">
              {project.description}
            </p>
          )}
        </div>
      </div>

      {/* Progress */}
      <div className="mt-4">
        <div className="flex justify-between items-center mb-1">
          <span className="text-xs text-brand-text-light">Progress</span>
          <span className="text-xs font-medium text-brand-text">
            {project.completedTaskCount}/{project.taskCount} tasks
          </span>
        </div>
        <div className="w-full bg-brand-secondary rounded-full h-2">
          <div
            className="bg-brand-primary rounded-full h-2 transition-all"
            style={{ width: `${completionPercentage}%` }}
          />
        </div>
      </div>

      {/* Footer */}
      <div className="mt-4 pt-4 border-t border-brand-border">
        <span className="text-xs text-brand-text-light">
          {completionPercentage === 100
            ? "✅ All done!"
            : completionPercentage === 0
            ? "Not started yet"
            : `${completionPercentage}% complete`}
        </span>
      </div>
    </Card>
  );
}
