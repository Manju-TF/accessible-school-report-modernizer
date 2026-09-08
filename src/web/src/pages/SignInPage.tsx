import { FormEvent, useState } from "react";
import { Navigate, useSearchParams } from "react-router-dom";
import { ApiError } from "../api/client";
import { useAuth } from "../auth/AuthContext";
import { LiveStatus } from "../ui/Status";

export function SignInPage() {
  const { user, ready, signIn } = useAuth();
  const [params] = useSearchParams();
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(params.get("error") === "1" ? "The user name or password is not correct." : "");
  const [busy, setBusy] = useState(false);

  if (ready && user) {
    return <Navigate to="/" replace />;
  }

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setBusy(true);
    setError("");
    try {
      await signIn(userName, password);
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : "The user name or password is not correct.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="auth-page">
      <div className="auth-split">
        <section className="auth-intro" aria-hidden="true">
          <p className="eyebrow">Meridian Test Client</p>
          <h1>School employment reports</h1>
          <p className="lede">Import graduate data, generate Class of 2025 school PDFs, and ask authorized questions from one workspace.</p>
        </section>
        <section className="panel auth-card">
          <div className="auth-brand">
            <span className="brand-mark" aria-hidden="true">
              M
            </span>
            <div>
              <p className="brand-name">Meridian Test Client</p>
              <p className="brand-tag">School employment reports</p>
            </div>
          </div>
          <h1>Sign in</h1>
          <p className="lede">Use your local account to open school reports and the knowledge assistant.</p>
          <LiveStatus tone="error" message={error} />
          <form aria-label="Sign in" onSubmit={(event) => void onSubmit(event)}>
            <div className="field">
              <label htmlFor="username">User name</label>
              <input id="username" name="username" autoComplete="username" required value={userName} onChange={(event) => setUserName(event.target.value)} />
            </div>
            <div className="field">
              <label htmlFor="password">Password</label>
              <input id="password" name="password" type="password" autoComplete="current-password" required value={password} onChange={(event) => setPassword(event.target.value)} />
            </div>
            <div className="actions">
              <button type="submit" disabled={busy}>
                {busy ? "Signing in" : "Sign in"}
              </button>
            </div>
          </form>
        </section>
      </div>
    </div>
  );
}
