import { Link } from "react-router-dom";

export function DeniedPage() {
  return (
    <div className="auth-page">
      <div className="auth-split">
        <section className="auth-intro" aria-hidden="true">
          <p className="eyebrow">Meridian Test Client</p>
          <h1>This page is reserved</h1>
          <p className="lede">Your account can still open the screens you are allowed to use.</p>
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
        <h1>Access denied</h1>
        <p className="lede">Your account does not have permission for that page.</p>
        <p>
          <Link className="button" to="/">
            Return to the dashboard
          </Link>
        </p>
      </section>
      </div>
    </div>
  );
}
