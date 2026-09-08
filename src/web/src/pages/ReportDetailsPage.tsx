import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { apiGet, ApiError } from "../api/client";
import type { ReportDetails } from "../api/types";
import { LiveStatus, StatusBadge } from "../ui/Status";

export function ReportDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [report, setReport] = useState<ReportDetails | null>(null);
  const [missing, setMissing] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!id) {
      setMissing(true);
      return;
    }

    void apiGet<ReportDetails>(`/api/reports/${id}`)
      .then(setReport)
      .catch((cause) => {
        if (cause instanceof ApiError && cause.status === 404) {
          setMissing(true);
          return;
        }

        setError(cause instanceof ApiError ? cause.message : "The report could not be loaded.");
      });
  }, [id]);

  if (missing) {
    return (
      <>
        <header className="page-header">
          <h1>Report not found</h1>
          <p className="lede">That report is not available.</p>
        </header>
        <p>
          <Link to="/runs">Return to run history</Link>
        </p>
      </>
    );
  }

  if (!report) {
    return (
      <>
        <h1>Report</h1>
        <LiveStatus tone={error ? "error" : "info"} message={error || "Loading report."} />
      </>
    );
  }

  return (
    <>
      <header className="page-header">
        <h1>Report</h1>
        <p className="lede">Authorized summary for {report.schoolLabel}.</p>
      </header>
      <section className="panel" aria-labelledby="report-details-heading">
        <h2 id="report-details-heading">Details</h2>
        <dl className="stat-list">
          <div>
            <dt>School</dt>
            <dd>{report.schoolLabel}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>
              <StatusBadge status={report.status} tone={report.statusTone} />
            </dd>
          </div>
          {report.reportYear ? (
            <div>
              <dt>Report year</dt>
              <dd>{report.reportYear}</dd>
            </div>
          ) : null}
          <div>
            <dt>Started</dt>
            <dd>{report.startedUtc}</dd>
          </div>
          <div>
            <dt>Completed</dt>
            <dd>{report.completedUtc}</dd>
          </div>
        </dl>
        <p className="actions">
          {report.downloadUrl ? (
            <a className="button" href={report.downloadUrl} download={report.downloadName} target="_blank" rel="noopener">
              Download PDF for {report.schoolLabel}
            </a>
          ) : null}
          <button type="button" className="button-secondary" onClick={() => navigate(`/knowledge-assistant?report=${report.id}`)}>
            Ask about this report
          </button>
          <Link to="/runs">Back to history</Link>
        </p>
      </section>
    </>
  );
}
