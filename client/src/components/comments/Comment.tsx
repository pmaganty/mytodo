import { useState } from "react";
import type { Comment as CommentType } from "../../types";
import { useAuth } from "../../context/AuthContext";
import api from "../../services/api";
import Button from "../ui/Button";

interface CommentProps {
  comment: CommentType;
  onCommentUpdated: (comment: CommentType) => void;
  onCommentDeleted: (commentId: string) => void;
}

export default function Comment({ comment, onCommentUpdated, onCommentDeleted }: CommentProps) {
  const { user } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [body, setBody] = useState(comment.body);
  const [isLoading, setIsLoading] = useState(false);
  const isAuthor = user?.id === comment.authorId;

  const handleUpdate = async () => {
    if (!body.trim()) return;
    setIsLoading(true);
    console.log("comment.authorId:", comment.authorId);
    console.log("user.id:", user?.id);
    console.log("Patching URL:", `/api/comments/${comment.id}`);
    console.log("Body:", { body: body.trim() });
    try {
      const { data } = await api.patch(`/api/comments/${comment.id}`, { body: body.trim() });
      onCommentUpdated(data);
      setIsEditing(false);
    } catch {
      console.error("Failed to update comment");
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!confirm("Delete this comment?")) return;
    try {
      await api.delete(`/api/comments/${comment.id}`);
      onCommentDeleted(comment.id);
    } catch {
      console.error("Failed to delete comment");
    }
  };

  return (
    <div className="flex gap-3 py-3 border-b border-brand-border last:border-0">
      {/* Avatar */}
      <div className="w-8 h-8 rounded-full bg-brand-secondary border border-brand-border flex items-center justify-center flex-shrink-0">
        <span className="text-xs font-medium text-brand-muted">
          {comment.authorName.charAt(0).toUpperCase()}
        </span>
      </div>

      {/* Content */}
      <div className="flex-1">
        <div className="flex items-center justify-between mb-1">
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium text-brand-text">
              {comment.authorName}
            </span>
            <span className="text-xs text-brand-text-light">
              {new Date(comment.createdAt).toLocaleDateString()}
            </span>
          </div>

          {/* Actions - only for author */}
          {isAuthor && !isEditing && (
          <div className="flex gap-1">
             <button
             onClick={() => setIsEditing(true)}
             className="text-xs text-brand-text-light hover:text-brand-text bg-brand-bg px-2 py-1 rounded-lg transition-all"
             >
             Edit
             </button>
             <button
             onClick={handleDelete}
             className="text-xs text-brand-error bg-brand-bg px-2 py-1 rounded-lg hover:opacity-75 transition-all"
             >
             Delete
             </button>
          </div>
          )}
        </div>

        {isEditing ? (
          <div className="space-y-2">
            <textarea
              value={body}
              onChange={(e) => setBody(e.target.value)}
              rows={2}
              className="w-full px-3 py-2 rounded-xl border border-brand-border bg-brand-bg text-brand-text text-sm focus:outline-none focus:ring-2 focus:ring-brand-primary resize-none"
            />
            <div className="flex gap-2">
              <Button size="sm" onClick={handleUpdate} disabled={isLoading}>
                {isLoading ? "Saving..." : "Save"}
              </Button>
              <Button size="sm" variant="ghost" onClick={() => {
                setIsEditing(false);
                setBody(comment.body);
              }}>
                Cancel
              </Button>
            </div>
          </div>
        ) : (
          <p className="text-sm text-brand-text-light font-sans">{comment.body}</p>
        )}
      </div>
    </div>
  );
}
