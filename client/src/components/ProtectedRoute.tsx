import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

/**
 * Wraps protected routes to ensure only authenticated users can access them.
 * Used in App.tsx around every route except /login.
 *
 * The isLoading check is critical — on first render, AuthContext hasn't yet
 * read localStorage to restore the session. Without this check, user would
 * briefly be null and the user would be redirected to /login even if they
 * have a valid stored session. We wait until the session check is complete
 * before making any routing decision.
 */
export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { user, isLoading } = useAuth();

  if (isLoading) return <div>Loading...</div>;
  if (!user) return <Navigate to="/login" replace />;

  return <>{children}</>;
}
