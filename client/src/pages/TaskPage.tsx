import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { Task, Comment } from "../types";
import api from "../services/api";
import Navbar from "../components/layout/Navbar";
import TaskDetailCard from "../components/tasks/TaskDetailCard";
import CommentList from "../components/comments/CommentList";
import { useAuth } from "../context/AuthContext";

export default function TaskPage() {
  const { id } = useParams<{ id: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();

  const [task, setTask] = useState<Task | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (id) fetchAll();
  }, [id]);

  const fetchAll = async () => {
    try {
      const [taskRes, commentsRes] = await Promise.all([
        api.get(`/api/tasks/${id}`),
        api.get(`/api/tasks/${id}/comments`),
      ]);
      setTask(taskRes.data);
      setComments(commentsRes.data);
    } catch {
      navigate("/");
    } finally {
      setIsLoading(false);
    }
  };

  const handleTaskUpdated = (updatedTask: Task) => {
    setTask(updatedTask);
  };

  const handleAddComment = async (body: string) => {
    const { data } = await api.post(`/api/tasks/${id}/comments`, { body });
    setComments((prev) => [data, ...prev]);
    return data;
  };

  const handleTaskDeleted = () => {
    navigate(`/projects/${task?.projectId}`);
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-brand-bg">
        <Navbar />
        <div className="flex items-center justify-center py-16">
          <p className="text-brand-text-light">Loading task...</p>
        </div>
      </div>
    );
  }

  if (!task) return null;

  return (
    <div className="min-h-screen bg-brand-bg">
      <Navbar />
      <main className="max-w-3xl mx-auto px-6 py-8">
        
        {/* Back button */}
        <button
          onClick={() => navigate(-1)}
          className="text-brand-text-light hover:text-brand-text text-sm mb-6 flex items-center gap-1 transition-all"
        >
          ← Back
        </button>

        <TaskDetailCard
          task={task}
          onTaskUpdated={handleTaskUpdated}
          onTaskDeleted={handleTaskDeleted}
          isOwner={user?.id === task.createdById}
        />
        
        <CommentList
          comments={comments}
          onAddComment={handleAddComment}
        />
      </main>
    </div>
  );
}
