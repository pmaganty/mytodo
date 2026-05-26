interface CardProps {
  children: React.ReactNode;
  onClick?: () => void;
  className?: string;
  hoverable?: boolean;
}

export default function Card({
  children,
  onClick,
  className = "",
  hoverable = false,
}: CardProps) {
  return (
    <div
      onClick={onClick}
      className={`
        bg-brand-paper border border-brand-border rounded-2xl p-6
        ${hoverable ? "hover:shadow-md hover:border-brand-primary cursor-pointer transition-all" : ""}
        ${className}
      `}
    >
      {children}
    </div>
  );
}
