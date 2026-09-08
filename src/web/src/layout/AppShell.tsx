import { useState } from "react";
import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { RobotMark } from "../ui/RobotMark";

type Capabilities = {
  canViewReports: boolean;
  canGenerate: boolean;
  canImport: boolean;
  canGenerateAll: boolean;
  canAsk: boolean;
};

const sections = [
  {
    heading: "Workspace",
    links: [
      { to: "/", label: "Dashboard", icon: "dashboard", show: (c: Capabilities) => c.canViewReports },
      { to: "/import", label: "Import data", icon: "import", show: (c: Capabilities) => c.canImport },
    ],
  },
  {
    heading: "Reports",
    links: [
      { to: "/generate", label: "Generate report", icon: "generate", show: (c: Capabilities) => c.canGenerate },
      { to: "/generate-all", label: "Generate all", icon: "generate-all", show: (c: Capabilities) => c.canGenerateAll },
      { to: "/runs", label: "History", icon: "history", show: (c: Capabilities) => c.canViewReports },
    ],
  },
  {
    heading: "Ask",
    links: [{ to: "/knowledge-assistant", label: "Assistant", icon: "assistant", show: (c: Capabilities) => c.canAsk }],
  },
];

function pageClass(pathname: string) {
  if (pathname === "/") {
    return "page-dashboard";
  }

  if (pathname.startsWith("/reports/")) {
    return "page-report";
  }

  return `page-${pathname.replace(/^\//, "").replaceAll("/", "-")}`;
}

function readCollapsed() {
  try {
    return sessionStorage.getItem("asr.navCollapsed") === "1";
  } catch {
    return false;
  }
}

function NavIcon({ name }: { name: string }) {
  const paths: Record<string, string> = {
    dashboard: "M4 4h7v7H4V4zm9 0h7v7h-7V4zM4 13h7v7H4v-7zm9 0h7v7h-7v-7z",
    import: "M12 3v12m0 0-4-4m4 4 4-4M5 19h14",
    generate: "M7 3h7l5 5v13H7V3zm7 0v5h5",
    "generate-all": "M6 7h12M6 12h12M6 17h8",
    history: "M12 7v5l3 2m6-2a9 9 0 1 1-18 0 9 9 0 0 1 18 0z",
  };

  if (name === "assistant") {
    return <RobotMark size={22} className="nav-icon nav-robot" />;
  }

  return (
    <svg className="nav-icon" viewBox="0 0 24 24" aria-hidden="true">
      <path d={paths[name] ?? paths.dashboard} fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

export function AppShell() {
  const { user, signOut } = useAuth();
  const location = useLocation();
  const capabilities = user?.capabilities;
  const [collapsed, setCollapsed] = useState(readCollapsed);

  const toggleNav = () => {
    setCollapsed((current) => {
      const next = !current;
      try {
        sessionStorage.setItem("asr.navCollapsed", next ? "1" : "0");
      } catch {
        /* ignore */
      }
      return next;
    });
  };

  return (
    <div className={`app-shell${collapsed ? " is-nav-collapsed" : ""}`}>
      <a className="skip-link" href="#main-content">
        Skip to main content
      </a>
      <header className="site-header">
        <div className="brand">
          <span className="brand-mark" aria-hidden="true">
            M
          </span>
          <div>
            <p className="brand-name">Meridian Test Client</p>
            <p className="brand-tag">School employment reports</p>
          </div>
        </div>
        {user ? (
          <div className="header-account">
            <div className="account-chip">
              <span className="account-avatar" aria-hidden="true">
                {user.userName.slice(0, 1).toUpperCase()}
              </span>
              <p>
                Signed in as <strong>{user.userName}</strong>
              </p>
            </div>
            <button type="button" className="button-secondary" onClick={() => void signOut()}>
              Sign out
            </button>
          </div>
        ) : null}
      </header>
      <div className="shell-body">
        <nav id="workspace-nav" className="side-nav" aria-label="Main">
          <div className="nav-toolbar">
            {collapsed ? null : <p className="nav-heading">Navigation</p>}
            <button
              type="button"
              className="nav-collapse"
              aria-expanded={!collapsed}
              aria-controls="workspace-nav"
              aria-label={collapsed ? "Open menu" : "Collapse menu"}
              title={collapsed ? "Open menu" : "Back"}
              onClick={toggleNav}
            >
              <svg className="nav-icon" viewBox="0 0 24 24" aria-hidden="true">
                <path
                  d={collapsed ? "M9 6l6 6-6 6" : "M15 6 9 12l6 6"}
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
              </svg>
              <span className="nav-collapse-label">{collapsed ? "Open" : "Back"}</span>
            </button>
          </div>
          {sections.map((section) => {
            const visible = section.links.filter((link) => !capabilities || link.show(capabilities));
            if (visible.length === 0) {
              return null;
            }

            return (
              <div className="nav-section" key={section.heading}>
                {collapsed ? null : <p className="nav-heading">{section.heading}</p>}
                <ul>
                  {visible.map((link) => {
                    const current = location.pathname === link.to;
                    return (
                      <li key={link.to}>
                        <NavLink to={link.to} end title={link.label} aria-label={link.label}>
                          <NavIcon name={link.icon} />
                          <span className="nav-label">
                            {link.label}
                            {current ? <span className="visually-hidden"> (current page)</span> : null}
                          </span>
                        </NavLink>
                      </li>
                    );
                  })}
                </ul>
              </div>
            );
          })}
        </nav>
        <main id="main-content" className={`page ${pageClass(location.pathname)}`} tabIndex={-1}>
          <Outlet />
        </main>
      </div>
      <footer className="site-footer">
        <p>Meridian Test Client · School employment reports · Class of 2025</p>
      </footer>
    </div>
  );
}
