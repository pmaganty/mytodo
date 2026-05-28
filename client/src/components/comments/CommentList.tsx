import { useState } from "react";
import type { Comment as CommentType } from "../../types";
import Comment from "./Comment";
import Button from "../ui/Button";

interface CommentListProps {
  comments: CommentType[];
  onAddComment: (body: string) => Promise<CommentType>;
}

export default function CommentList({ comments: initialComments, onAddComment }: CommentListProps) {
  const [comments, setComments] = useState(initialComments);
  const [body, setBody] = useState("");
  const [isLoading, setIsLoading] = useState(false);

   /**
   * Calls the parent's onAddComment handler (which posts to the API),
   * then prepends the new comment to the local state so it appears
   * immediately without a page refresh.
   */
  const handleSubmit = async () => {
    if (!body.trim()) return;
    setIsLoading(true);
    try {
        const newComment = await onAddComment(body.trim());
        if (newComment) {
            setComments((prev) => [newComment, ...prev]);
        }
        setBody("");
    } finally {
        setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  const handleCommentUpdated = (updatedComment: CommentType) => {
    setComments((prev) =>
      prev.map((c) => (c.id === updatedComment.id ? updatedComment : c))
    );
  };

  const handleCommentDeleted = (commentId: string) => {
    setComments((prev) => prev.filter((c) => c.id !== commentId));
  };

  return (
    <div className="bg-brand-paper border border-brand-border rounded-2xl overflow-hidden">
      <div className="px-6 py-4 border-b border-brand-border">
        <h3 className="font-display text-lg font-bold text-brand-text">Comments</h3>
      </div>

      <div className="px-6 py-2">
        {comments.length === 0 ? (
          <p className="text-center text-brand-text-light text-sm py-8">
            No comments yet — be the first to add one!
          </p>
        ) : (
          comments.map((comment) => (
            <Comment
              key={comment.id}
              comment={comment}
              onCommentUpdated={handleCommentUpdated}
              onCommentDeleted={handleCommentDeleted}
            />
          ))
        )}
      </div>

      <div className="px-6 py-4 border-t border-brand-border flex gap-3 items-center">
        <textarea
          value={body}
          onChange={(e) => setBody(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Add a comment... (Enter to submit)"
          rows={2}
          className="flex-1 px-4 py-2.5 rounded-xl border border-brand-border bg-brand-bg text-brand-text placeholder-brand-text-light focus:outline-none focus:ring-2 focus:ring-brand-primary text-sm resize-none"
        />
        <Button onClick={handleSubmit} disabled={isLoading || !body.trim()} size="sm">
          Send
        </Button>
      </div>
    </div>
  );
}
