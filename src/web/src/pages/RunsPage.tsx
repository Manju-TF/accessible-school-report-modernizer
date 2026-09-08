import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { apiGet, ApiError } from "../api/client";
import type { RunSummary } from "../api/types";
import { LiveStatus, StatusBadge } from "../ui/Status";
import { PageLinks } from "../ui/PageLinks";
import { useAuth } from "../auth/AuthContext";

export function RunsPage() {
  const { user } = useAuth();
  const [params] = useSearchParams();
  const requestedRun = Number(params.get("run"));
  const [runs, setRuns] = useState<RunSummary[] | null>(null);
  const [error, setError] = useState("");
  const [selectedId, setSelectedId] = useState<number | null>(Number.isInteger(requestedRun) && requestedRun > 0 ? requestedRun : null);

  useEffect(() => {
    void apiGet<{ runs: RunSummary[] }>("/api/runs")
      .then((data) => {
        setRuns(data.runs);
        setSelectedId((current) => {
          if (current && data.runs.some((run) => run.id === current)) {
            return current;
          }

          if (requestedRun > 0 && data.runs.some((run) => run.id === requestedRun)) {
            return requestedRun;
          }

          return data.runs[0]?.id ?? null;
        });
      })
      .catch((cause) => setError(cause instanceof ApiError ? cause.message : "Run history could not be loaded."));
  }, [requestedRun]);

  const selected = runs?.find((run) => run.id === selectedId) ?? null;

  return (
    <>
      <header className="page-header page-hero">
        <p className="eyebrow">Runs</p>
        <h1>Run history</h1>
        <p className="lede">Select a run to see school results, downloads, and report details beside the list.</p>
        <PageLinks
          label="Related pages"
          links={[
            ...(user?.capabilities.canGenerate ? [{ to: "/generate", label: "Generate report" }] : []),
            ...(user?.capabilities.canGenerateAll ? [{ to: "/generate-all", label: "Generate all reports" }] : []),
          ]}
        />
      </header>
      <LiveStatus tone={error ? "error" : "info"} message={error || (runs ? null : "Loading run history.")} />
      {runs && runs.length === 0 ? <p>No report runs have been recorded yet.</p> : null}
      {runs && runs.length > 0 ? (
        <div className="history-split">
          <ul className="run-list" aria-label="Report runs">
            {runs.map((run) => {
              const current = selected?.id === run.id;
              return (
                <li key={run.id}>
                  <button
                    type="button"
                    className={`run-select${current ? " is-selected" : ""}`}
                    aria-current={current ? "true" : undefined}
                    aria-controls="run-detail"
                    onClick={() => setSelectedId(run.id)}
                  >
                    <span className="run-select-title">Run {run.id}</span>
                    <span className="run-chips">
                      <StatusBadge status={run.status} tone={run.statusTone} />
                      <span className="meta-chip">{run.mode}</span>
                    </span>
                    <span className="hint">
                      {run.startedUtc} · {run.successfulCount}/{run.totalCount} · {run.duration}
                    </span>
                  </button>
                </li>
              );
            })}
          </ul>
          <section id="run-detail" className="panel history-detail" aria-live="polite">
            {selected ? (
              <>
                <div className="run-head">
                  <h2>Run {selected.id}</h2>
                  <div className="run-chips">
                    <StatusBadge status={selected.status} tone={selected.statusTone} />
                    <span className="meta-chip">{selected.mode}</span>
                    <span className="meta-chip">{selected.duration}</span>
                  </div>
                </div>
                <p>
                  Started {selected.startedUtc}. Total {selected.totalCount}. Successful {selected.successfulCount}. Failed {selected.failedCount}.
                </p>
                <div className="table-wrap">
                  <table>
                    <caption>School results for run {selected.id}</caption>
                    <thead>
                      <tr>
                        <th scope="col">School</th>
                        <th scope="col">Status</th>
                        <th scope="col">Message</th>
                        <th scope="col">Download</th>
                        <th scope="col">Report</th>
                      </tr>
                    </thead>
                    <tbody>
                      {selected.items.map((item) => (
                        <tr key={item.id}>
                          <td>{item.schoolLabel}</td>
                          <td>
                            <StatusBadge status={item.status} tone={item.statusTone} />
                          </td>
                          <td>{item.message}</td>
                          <td>
                            {item.downloadUrl ? (
                              <a href={item.downloadUrl} download={item.downloadName} target="_blank" rel="noopener">
                                Download PDF for {item.schoolLabel}
                              </a>
                            ) : (
                              "Not available"
                            )}
                          </td>
                          <td>
                            <Link to={`/reports/${item.id}`}>Report details for {item.schoolLabel}</Link>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            ) : (
              <p>Select a run to see school details.</p>
            )}
          </section>
        </div>
      ) : null}
    </>
  );
}
