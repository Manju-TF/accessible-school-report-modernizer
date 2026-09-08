import { Link } from "react-router-dom";

export interface PageLinkItem {
  to: string;
  label: string;
  primary?: boolean;
}

export function PageLinks({ label, links }: { label: string; links: PageLinkItem[] }) {
  if (links.length === 0) {
    return null;
  }

  return (
    <nav className="quick-actions" aria-label={label}>
      {links.map((link) => (
        <Link key={link.to} className={link.primary ? "button" : "button-secondary"} to={link.to}>
          {link.label}
        </Link>
      ))}
    </nav>
  );
}
