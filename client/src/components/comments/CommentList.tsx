import { useState } from "react";
import type { Comment as CommentType } from "../../types";
import Comment from "./Comment";
import Button from "../ui/Button";

interface CommentListProps {
  comments: CommentType[];
  onAddComment: (body: string) => Promise<void>;
}

export default function CommentList({ comments, onAddComment }: CommentListProps) {
  const [body, setBody] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async () => {
    if (!body.trim()) return;
    setIsLoading(true);
    try {
      await onAddComment(body.trim());
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

  return (
    <div className="bg-brand-paper border border-brand-border rounded-2xl overflow-hidden">
      {/* Header */}
      <div className="px-6 py-4 border-b border-brand-border">
        <h3 className="font-display text-lg font-bold text-brand-text">
          Comments
        </h3>
      </div>

      {/* Comments */}
      <div className="px-6 py-2">
        {comments.length === 0 ? (
          <p className="text-center text-brand-text-light text-sm py-8">
            No comments yet — be the first to add one!
          </p>
        ) : (
          comments.map((comment) => (
            <Comment key={comment.id} comment={comment} />
          ))
        )}
      </div>

      {/* Input */}
      <div className="px-6 py-4 border-t border-brand-border flex gap-3">
        <textarea
          value={body}
          onChange={(e) => setBody(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Add a comment... (Enter to submit)"
          rows={2}
          className="flex-1 px-4 py-2.5 rounded-xl border border-brand-border bg-brand-bg text-brand-text placeholder-brand-text-light focus:outline-none focus:ring-2 focus:ring-brand-primary text-sm resize-none"
        />
        <Button
          onClick={handleSubmit}
          disabled={isLoading || !body.trim()}
          size="sm"
        >
          Send
        </Button>
      </div>
    </div>
  );
}
