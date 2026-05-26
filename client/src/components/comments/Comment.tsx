import type { Comment as CommentType } from "../../types";

interface CommentProps {
  comment: CommentType;
}

export default function Comment({ comment }: CommentProps) {
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
        <div className="flex items-center gap-2 mb-1">
          <span className="text-sm font-medium text-brand-text">
            {comment.authorName}
          </span>
          <span className="text-xs text-brand-text-light">
            {new Date(comment.createdAt).toLocaleDateString()}
          </span>
        </div>
        <p className="text-sm text-brand-text-light font-sans">
          {comment.body}
        </p>
      </div>
    </div>
  );
}
