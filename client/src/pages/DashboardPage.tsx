import { useState, useEffect } from "react";
import type { Project } from "../types";
import api from "../services/api";
import Navbar from "../components/layout/Navbar";
import ProjectList from "../components/projects/ProjectList";
import Button from "../components/ui/Button";
import CreateProjectModal from "../components/projects/CreateProjectModal";

export default function DashboardPage() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [search, setSearch] = useState("");

  useEffect(() => {
    const timeout = setTimeout(() => {
        fetchProjects(search);
    }, 300);
    return () => clearTimeout(timeout);
  }, [search]);

  const fetchProjects = async (searchTerm = "") => {
    try {
        const params = new URLSearchParams();
        if (searchTerm) params.append("search", searchTerm);
        const { data } = await api.get(`/api/projects?${params.toString()}`);
        setProjects(data);
    } catch {
        setError("Failed to load projects");
    } finally {
        setIsLoading(false);
    }
  };

  const handleProjectCreated = (project: Project) => {
    setProjects((prev) => [project, ...prev]);
  };

  return (
    <div className="min-h-screen bg-brand-bg">
      <Navbar />
      <main className="max-w-5xl mx-auto px-6 py-8">

        {/* Header */}
        <div className="flex items-center justify-between mb-6">
        <div>
            <h2 className="font-display text-3xl font-bold text-brand-text">
            My Projects
            </h2>
            <p className="text-brand-text-light mt-1 font-sans">
            {projects.length} {projects.length === 1 ? "project" : "projects"}
            </p>
        </div>
        <Button onClick={() => setIsModalOpen(true)}>
            + New Project
        </Button>
        </div>

        {/* Search */}
        <div className="mb-6 flex">
        <input
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search projects..."
            className="w-full max-w-sm px-4 py-2.5 rounded-xl border border-brand-border bg-brand-paper text-brand-text placeholder-brand-text-light focus:outline-none focus:ring-2 focus:ring-brand-primary text-sm"
        />
        </div>

        {/* Content */}
        {isLoading ? (
          <div className="text-center py-16 text-brand-text-light">
            Loading your projects...
          </div>
        ) : error ? (
          <div className="text-center py-16 text-brand-error">{error}</div>
        ) : (
          <ProjectList projects={projects} />
        )}
      </main>

      <CreateProjectModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onProjectCreated={handleProjectCreated}
      />
    </div>
  );
}
