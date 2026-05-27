import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { Project, Task, Comment } from "../types";
import api from "../services/api";
import Navbar from "../components/layout/Navbar";
import ProjectDetailCard from "../components/projects/ProjectDetailCard";
import TaskList from "../components/tasks/TaskList";
import CreateTaskModal from "../components/tasks/CreateTaskModal";
import CommentList from "../components/comments/CommentList";

export default function ProjectPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [project, setProject] = useState<Project | null>(null);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [comments, setComments] = useState<Comment[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);

  useEffect(() => {
    if (id) fetchAll();
  }, [id]);

  const fetchAll = async () => {
    try {
      const [projectRes, tasksRes, commentsRes] = await Promise.all([
        api.get(`/api/projects/${id}`),
        api.get(`/api/projects/${id}/tasks`),
        api.get(`/api/projects/${id}/comments`),
      ]);
      setProject(projectRes.data);
      setTasks(tasksRes.data);
      setComments(commentsRes.data);
    } catch {
      navigate("/");
    } finally {
      setIsLoading(false);
    }
  };

  const handleTaskCreated = (task: Task) => {
    setTasks((prev) => [task, ...prev]);
    if (project) {
      setProject({ ...project, taskCount: project.taskCount + 1 });
    }
  };

  const handleTaskUpdated = (updatedTask: Task) => {
    setTasks((prev) =>
        prev.map((t) => (t.id === updatedTask.id ? updatedTask : t))
    );
    if (project) {
        const completedCount = tasks
        .map((t) => (t.id === updatedTask.id ? updatedTask : t))
        .filter((t) => t.status === "Done").length;
        setProject({ ...project, completedTaskCount: completedCount });
    }
  };

  const handleProjectUpdated = (updatedProject: Project) => {
    setProject(updatedProject);
  };

  const handleAddComment = async (body: string) => {
    const { data } = await api.post(`/api/projects/${id}/comments`, { body });
    setComments((prev) => [data, ...prev]);
    return data;
  };

  const handleProjectDeleted = () => {
    navigate("/");
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-brand-bg">
        <Navbar />
        <div className="flex items-center justify-center py-16">
          <p className="text-brand-text-light">Loading project...</p>
        </div>
      </div>
    );
  }

  if (!project) return null;

  return (
    <div className="min-h-screen bg-brand-bg">
      <Navbar />
      <main className="max-w-5xl mx-auto px-6 py-8">

        {/* Back button */}
        <button
          onClick={() => navigate("/")}
          className="text-brand-text-light hover:text-brand-text text-sm mb-6 flex items-center gap-1 transition-all"
        >
          ← Back to projects
        </button>

        <ProjectDetailCard
          project={project}
          onProjectUpdated={handleProjectUpdated}
          onProjectDeleted={handleProjectDeleted}
        />

        <div className="space-y-6">
          <TaskList
            tasks={tasks}
            onAddTask={() => setIsTaskModalOpen(true)}
            onTaskUpdated={handleTaskUpdated}
          />
          <CommentList
            comments={comments}
            onAddComment={handleAddComment}
          />
        </div>
      </main>

      <CreateTaskModal
        isOpen={isTaskModalOpen}
        onClose={() => setIsTaskModalOpen(false)}
        projectId={id!}
        onTaskCreated={handleTaskCreated}
      />
    </div>
  );
}
