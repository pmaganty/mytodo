interface ButtonProps {
  children: React.ReactNode;
  onClick?: () => void;
  type?: "button" | "submit" | "reset";
  variant?: "primary" | "secondary" | "ghost" | "danger";
  size?: "sm" | "md" | "lg";
  disabled?: boolean;
  fullWidth?: boolean;
}

const variants = {
  primary: "bg-brand-primary hover:bg-brand-primary-hover text-white",
  secondary: "bg-brand-secondary hover:bg-brand-border text-brand-muted",
  ghost: "bg-transparent hover:bg-brand-secondary text-brand-text-light hover:text-brand-text",
  danger: "bg-brand-error hover:opacity-90 text-white",
};

const sizes = {
  sm: "px-3 py-1.5 text-sm",
  md: "px-4 py-2.5 text-sm",
  lg: "px-6 py-3 text-base",
};

export default function Button({
  children,
  onClick,
  type = "button",
  variant = "primary",
  size = "md",
  disabled = false,
  fullWidth = false,
}: ButtonProps) {
  return (
    <button
      type={type}
      onClick={onClick}
      disabled={disabled}
      className={`
        ${variants[variant]}
        ${sizes[size]}
        ${fullWidth ? "w-full" : ""}
        font-medium rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed
      `}
    >
      {children}
    </button>
  );
}
