import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { Project, Task, Comment, ProjectMemberResponse } from "../types";
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
  const [members, setMembers] = useState<ProjectMemberResponse[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);

  // Filter state
  const [status, setStatus] = useState<string[]>([]);
  const [priority, setPriority] = useState<string[]>([]);
  const [createdById, setCreatedById] = useState<string[]>([]);
  const [sortBy, setSortBy] = useState("");
  const [sortOrder, setSortOrder] = useState("asc");

  useEffect(() => {
    if (id) fetchAll();
  }, [id]);

  useEffect(() => {
    if (id) fetchTasks();
  }, [status, priority, createdById, sortBy, sortOrder]);

  const fetchAll = async () => {
    try {
      const [projectRes, tasksRes, commentsRes, membersRes] = await Promise.all([
        api.get(`/api/projects/${id}`),
        api.get(`/api/projects/${id}/tasks`),
        api.get(`/api/projects/${id}/comments`),
        api.get(`/api/projects/${id}/members`),
      ]);
      setProject(projectRes.data);
      setTasks(tasksRes.data);
      setComments(commentsRes.data);
      setMembers(membersRes.data);
    } catch {
      navigate("/");
    } finally {
      setIsLoading(false);
    }
  };

  const fetchTasks = async () => {
    try {
        const params = new URLSearchParams();
        status.forEach((s) => params.append("status", s));
        priority.forEach((p) => params.append("priority", p));
        createdById.forEach((id) => params.append("createdById", id));
        if (sortBy) params.append("sortBy", sortBy);
        if (sortOrder) params.append("sortOrder", sortOrder);

        const { data } = await api.get(`/api/projects/${id}/tasks?${params.toString()}`);
        setTasks(data);
    } catch {
        console.error("Failed to fetch tasks");
    }
  };

  const handleFilterChange = (key: string, value: string) => {
    switch (key) {
        case "status":
        setStatus((prev) =>
            prev.includes(value) ? prev.filter((s) => s !== value) : [...prev, value]
        );
        break;
        case "priority":
        setPriority((prev) =>
            prev.includes(value) ? prev.filter((p) => p !== value) : [...prev, value]
        );
        break;
        case "createdById":
        setCreatedById((prev) =>
            prev.includes(value) ? prev.filter((id) => id !== value) : [...prev, value]
        );
        break;
        case "sortBy": setSortBy(value); break;
        case "sortOrder": setSortOrder(value); break;
    }
  };

  const handleReset = () => {
    setStatus([]);
    setPriority([]);
    setCreatedById([]);
    setSortBy("");
    setSortOrder("asc");
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

  const handleProjectDeleted = () => {
    navigate("/");
  };

  const handleAddComment = async (body: string) => {
    const { data } = await api.post(`/api/projects/${id}/comments`, { body });
    setComments((prev) => [data, ...prev]);
    return data;
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
            members={members}
            status={status}
            priority={priority}
            createdById={createdById}
            sortBy={sortBy}
            sortOrder={sortOrder}
            onFilterChange={handleFilterChange}
            onReset={handleReset}
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
