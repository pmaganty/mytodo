import type { Project } from "../../types";
import ProjectCard from "./ProjectCard";

interface ProjectListProps {
  projects: Project[];
}

export default function ProjectList({ projects }: ProjectListProps) {
  if (projects.length === 0) {
    return (
      <div className="text-center py-16">
        <p className="text-4xl mb-4">📝</p>
        <h3 className="font-display text-xl font-bold text-brand-text mb-2">
          No projects yet
        </h3>
        <p className="text-brand-text-light font-sans">
          Create your first project to get started
        </p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {projects.map((project) => (
        <ProjectCard key={project.id} project={project} />
      ))}
    </div>
  );
}
