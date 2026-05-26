import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Button from "../ui/Button";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <nav className="bg-brand-paper border-b border-brand-border px-6 py-4">
      <div className="max-w-5xl mx-auto flex items-center justify-between">
        <h1 className="font-display text-2xl font-bold text-brand-text">
          mytodo
        </h1>
        <div className="flex items-center gap-4">
          <span className="text-brand-text-light text-sm font-sans">
            Hey, {user?.name} 👋
          </span>
          <Button variant="ghost" size="sm" onClick={handleLogout}>
            Log out
          </Button>
        </div>
      </div>
    </nav>
  );
}
