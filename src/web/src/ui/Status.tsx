import type { StatusTone } from "../api/types";

export function LiveStatus({
  id,
  tone,
  message,
}: {
  id?: string;
  tone: StatusTone | "info";
  message?: string | null;
}) {
  if (!message) {
    return <div id={id} className={`live-status tone-${tone}`} role="status" aria-live="polite" />;
  }

  return (
    <p id={id} className={`live-status tone-${tone}`} role="status" aria-live="polite">
      {message}
    </p>
  );
}

export function StatusBadge({ status, tone }: { status: string; tone: StatusTone }) {
  return (
    <span className={`status-badge tone-${tone}`}>
      <span className="visually-hidden">Status: </span>
      {status}
    </span>
  );
}
