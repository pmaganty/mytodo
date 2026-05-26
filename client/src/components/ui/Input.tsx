interface InputProps {
  label?: string;
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  type?: "text" | "email" | "password" | "date";
  error?: string;
  disabled?: boolean;
}

export default function Input({
  label,
  value,
  onChange,
  placeholder,
  type = "text",
  error,
  disabled = false,
}: InputProps) {
  return (
    <div className="flex flex-col gap-1">
      {label && (
        <label className="text-sm font-medium text-brand-text">
          {label}
        </label>
      )}
      <input
        type={type}
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={placeholder}
        disabled={disabled}
        className={`
          px-4 py-2.5 rounded-xl border transition-all outline-none
          bg-brand-bg text-brand-text placeholder-brand-text-light
          focus:ring-2 focus:ring-brand-primary
          disabled:opacity-50 disabled:cursor-not-allowed
          ${error ? "border-brand-error" : "border-brand-border"}
        `}
      />
      {error && (
        <p className="text-sm text-brand-error">{error}</p>
      )}
    </div>
  );
}
