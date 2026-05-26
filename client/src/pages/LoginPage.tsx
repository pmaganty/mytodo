import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import api from "../services/api";
import Button from "../components/ui/Button";
import Input from "../components/ui/Input";

export default function LoginPage() {
  const [isLogin, setIsLogin] = useState(true);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async () => {
    setError("");
    setIsLoading(true);

    try {
      const endpoint = isLogin ? "/auth/login" : "/auth/register";
      const payload = isLogin ? { email, password } : { name, email, password };
      const { data } = await api.post(endpoint, payload);
      login(data.token, data.user);
      navigate("/");
    } catch (err: any) {
      setError(err.response?.data?.error ?? "Something went wrong");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-brand-bg flex items-center justify-center p-4">
      <div className="bg-brand-paper rounded-2xl shadow-sm border border-brand-border w-full max-w-md p-8">

        {/* Header */}
        <div className="text-center mb-8">
          <h1 className="font-display text-4xl font-bold text-brand-text">mytodo</h1>
          <p className="text-brand-text-light mt-2 font-sans">your personal task space</p>
        </div>

        {/* Toggle */}
        <div className="flex bg-brand-bg rounded-xl p-1 mb-6 border border-brand-border">
          <button
            onClick={() => setIsLogin(true)}
            className={`flex-1 py-2 rounded-lg text-sm font-medium transition-all ${
              isLogin
                ? "bg-brand-paper text-brand-text shadow-sm"
                : "text-brand-text-light hover:text-brand-text"
            }`}
          >
            Log in
          </button>
          <button
            onClick={() => setIsLogin(false)}
            className={`flex-1 py-2 rounded-lg text-sm font-medium transition-all ${
              !isLogin
                ? "bg-brand-paper text-brand-text shadow-sm"
                : "text-brand-text-light hover:text-brand-text"
            }`}
          >
            Sign up
          </button>
        </div>

        {/* Form */}
        <div className="space-y-4">
          {!isLogin && (
            <Input
              label="Name"
              value={name}
              onChange={setName}
              placeholder="Your name"
            />
          )}
          <Input
            label="Email"
            value={email}
            onChange={setEmail}
            placeholder="you@example.com"
            type="email"
          />
          <Input
            label="Password"
            value={password}
            onChange={setPassword}
            placeholder="••••••••"
            type="password"
          />

          {error && (
            <p className="text-brand-error text-sm">{error}</p>
          )}

          <Button
            onClick={handleSubmit}
            disabled={isLoading}
            fullWidth
          >
            {isLoading ? "..." : isLogin ? "Log in" : "Create account"}
          </Button>
        </div>
      </div>
    </div>
  );
}
