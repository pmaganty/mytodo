import type { Project } from "../../types";

interface ProjectDetailCardProps {
  project: Project;
}

export default function ProjectDetailCard({ project }: ProjectDetailCardProps) {
  const completionPercentage =
    project.taskCount === 0
      ? 0
      : Math.round((project.completedTaskCount / project.taskCount) * 100);

  return (
    <div className="bg-brand-paper border border-brand-border rounded-2xl p-6 mb-6">
      {/* Header */}
      <div className="flex items-start gap-4 mb-6">
        {project.emoji && (
          <span className="text-5xl">{project.emoji}</span>
        )}
        <div className="flex-1">
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
  );
}
