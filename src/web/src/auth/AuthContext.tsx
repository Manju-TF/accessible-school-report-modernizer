import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import { apiGet, apiPost, ApiError } from "../api/client";
import type { MeResponse } from "../api/types";

interface AuthState {
  ready: boolean;
  user: MeResponse | null;
  refresh: () => Promise<void>;
  signIn: (userName: string, password: string) => Promise<void>;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [ready, setReady] = useState(false);
  const [user, setUser] = useState<MeResponse | null>(null);

  const refresh = async () => {
    try {
      setUser(await apiGet<MeResponse>("/api/me"));
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setUser(null);
        return;
      }

      setUser(null);
    }
  };

  useEffect(() => {
    void refresh().finally(() => setReady(true));
  }, []);

  const value = useMemo<AuthState>(
    () => ({
      ready,
      user,
      refresh,
      signIn: async (userName, password) => {
        await apiPost("/account/signin", { userName, password, returnUrl: "/" });
        await refresh();
      },
      signOut: async () => {
        await apiPost("/account/signout");
        setUser(null);
      },
    }),
    [ready, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const value = useContext(AuthContext);
  if (!value) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return value;
}
