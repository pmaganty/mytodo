import { createContext, useContext, useState, useEffect } from "react";
import type { ReactNode } from "react";
import type { User } from "../types";

interface AuthContextType {
  user: User | null;
  token: string | null;
  login: (token: string, user: User) => void;
  logout: () => void;
  isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

/**
 * AuthProvider wraps the entire app and provides global auth state.
 * Any component can access the current user, token, login, and logout
 * via the useAuth() hook without prop drilling.
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);

  /**
   * isLoading starts as true and is set to false after localStorage has been read.
   * This is critical — without it, ProtectedRoute would see user=null on first render
   * and redirect to /login before we've had a chance to restore the session from localStorage.
   * Components that depend on auth state should wait for isLoading=false before rendering.
   */
  const [isLoading, setIsLoading] = useState(true);


  /**
   * On mount, check localStorage for an existing session.
   * This is what keeps users logged in across page refreshes and browser restarts.
   * Runs once on app load — the empty dependency array ensures it never runs again.
   */
  useEffect(() => {
    const storedToken = localStorage.getItem("token");
    const storedUser = localStorage.getItem("user");

    if (storedToken && storedUser) {
      setToken(storedToken);
      setUser(JSON.parse(storedUser));
    }

    setIsLoading(false);
  }, []);


  /**
   * Persists the token and user to both localStorage (survives refresh)
   * and React state (triggers re-renders). Called after successful login or register.
   */
  const login = (token: string, user: User) => {
    localStorage.setItem("token", token);
    localStorage.setItem("user", JSON.stringify(user));
    setToken(token);
    setUser(user);
  };

  /**
   * Clears auth state from both localStorage and React state.
   * After this, ProtectedRoute will redirect the user to /login.
   */
  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, logout, isLoading }}>
      {children}
    </AuthContext.Provider>
  );
}

/**
 * Custom hook for consuming auth context.
 * Throws if used outside of AuthProvider — prevents silent failures
 * where a component gets null instead of the auth state it expects.
 */
export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within an AuthProvider");
  return context;
}
