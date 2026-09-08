import { Navigate, Outlet, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/AuthContext";
import { AppShell } from "./layout/AppShell";
import { AssistantPage } from "./pages/AssistantPage";
import { DashboardPage } from "./pages/DashboardPage";
import { DeniedPage } from "./pages/DeniedPage";
import { GenerateAllPage } from "./pages/GenerateAllPage";
import { GeneratePage } from "./pages/GeneratePage";
import { ImportPage } from "./pages/ImportPage";
import { ReportDetailsPage } from "./pages/ReportDetailsPage";
import { RunsPage } from "./pages/RunsPage";
import { SignInPage } from "./pages/SignInPage";

function RequireAuth() {
  const { ready, user } = useAuth();
  if (!ready) {
    return <p className="lede">Loading.</p>;
  }

  if (!user) {
    return <Navigate to="/signin" replace />;
  }

  return <Outlet />;
}

export function App() {
  return (
    <Routes>
      <Route path="/signin" element={<SignInPage />} />
      <Route path="/denied" element={<DeniedPage />} />
      <Route element={<RequireAuth />}>
        <Route element={<AppShell />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/import" element={<ImportPage />} />
          <Route path="/generate" element={<GeneratePage />} />
          <Route path="/generate-all" element={<GenerateAllPage />} />
          <Route path="/runs" element={<RunsPage />} />
          <Route path="/reports/:id" element={<ReportDetailsPage />} />
          <Route path="/knowledge-assistant" element={<AssistantPage />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
