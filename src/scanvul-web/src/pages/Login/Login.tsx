import React, { useEffect, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { useAuthStore } from "../../store/authStore";
import { useNavigate } from "react-router-dom";
import { ShieldCheck } from "lucide-react";
import { authApi } from "../../api/authApi";

export const LoginPage = () => {
  const [creds, setCreds] = useState({ name: "", password: "" });
  const [initData, setInitData] = useState<{
    name: string;
    password?: string;
  } | null>(null);
  const setToken = useAuthStore((s) => s.setToken);
  const navigate = useNavigate();

  const initMutation = useMutation({
    mutationFn: authApi.init,
    onSuccess: (res) => {
      setInitData(res.data);
      setCreds({ name: res.data.name, password: res.data.password ?? "" });
    },
    onError: () => {
      console.log("Admin already initialized");
    },
  });

  const loginMutation = useMutation({
    mutationFn: authApi.login,
    onSuccess: (res) => {
      setToken(res.data.token);
      navigate("/");
    },
  });

  useEffect(() => {
    initMutation.mutate();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []); 

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    loginMutation.mutate(creds);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <div className="w-full max-w-md bg-card border border-gray-200 dark:border-gray-800 rounded-xl shadow-lg p-8">
        <div className="text-center mb-8">
          <ShieldCheck className="mx-auto h-12 w-12 text-primary mb-2" />
          <h1 className="text-2xl font-bold">ScanVul</h1>
        </div>

        {initData && (
          <div className="mb-6 p-4 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg">
            <p className="text-green-800 dark:text-green-200 font-semibold mb-2">
              Initial Admin Created!
            </p>
            <p className="text-sm">
              Username: <strong>{initData.name}</strong>
            </p>
            <p className="text-sm">
              Password: <strong>{initData.password}</strong>
            </p>
            <p className="text-xs mt-2 opacity-75">
              Please save these credentials.
            </p>
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">Username</label>
            <input
              type="text"
              value={creds.name}
              onChange={(e) => setCreds({ ...creds, name: e.target.value })}
              className="w-full p-2 rounded-md border border-gray-300 dark:border-gray-700 bg-background"
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Password</label>
            <input
              type="password"
              value={creds.password}
              onChange={(e) => setCreds({ ...creds, password: e.target.value })}
              className="w-full p-2 rounded-md border border-gray-300 dark:border-gray-700 bg-background"
            />
          </div>

          {loginMutation.isError && (
            <div className="text-red-500 text-sm text-center">
              Invalid credentials
            </div>
          )}

          <button
            type="submit"
            disabled={loginMutation.isPending}
            className="w-full bg-primary text-white py-2 rounded-md bg-blue-800 hover:bg-blue-600 transition disabled:opacity-50"
          >
            {loginMutation.isPending ? "Logging in..." : "Login"}
          </button>
        </form>
      </div>
    </div>
  );
};
