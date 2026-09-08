import { FormEvent, useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { apiGet, apiPost, ApiError } from "../api/client";
import type { GenerateResult } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { LiveStatus, StatusBadge } from "../ui/Status";
import { PageLinks } from "../ui/PageLinks";

interface GenerateAllResult {
  runId: number;
  status: string;
  statusTone: "success" | "warning" | "error" | "info";
  total: number;
  successful: number;
  failed: number;
  duration: string;
  message?: string;
  items: GenerateResult[];
}

function pickYear(preferred: string, years: string[]) {
  return years.includes(preferred) ? preferred : (years[0] ?? "2025");
}

export function GenerateAllPage() {
  const { user } = useAuth();
  const [eligibleCount, setEligibleCount] = useState(0);
  const [classYears, setClassYears] = useState<string[]>(["2025"]);
  const [classYear, setClassYear] = useState("2025");
  const [mode, setMode] = useState("sequential");
  const [maxParallelism, setMaxParallelism] = useState(4);
  const [limits, setLimits] = useState({ min: 1, max: 8 });
  const [busy, setBusy] = useState(false);
  const [tone, setTone] = useState<"info" | "error" | "success" | "warning">("info");
  const [message, setMessage] = useState("");
  const [result, setResult] = useState<GenerateAllResult | null>(null);
  const abort = useRef<AbortController | null>(null);

  useEffect(() => {
    void apiGet<{
      eligibleCount: number;
      classYear: string;
      classYears: string[];
      minParallelism: number;
      maxParallelism: number;
      defaultParallelism: number;
    }>("/api/reports/all")
      .then((data) => {
        const years = data.classYears?.length ? data.classYears : [data.classYear || "2025"];
        setEligibleCount(data.eligibleCount);
        setClassYears(years);
        setClassYear(pickYear(data.classYear, years));
        setMaxParallelism(data.defaultParallelism);
        setLimits({ min: data.minParallelism, max: data.maxParallelism });
        if (data.eligibleCount === 0) {
          setTone("warning");
          setMessage("No schools with graduate records are available. Import a workbook first.");
        }
      })
      .catch((cause) => {
        setTone("error");
        setMessage(cause instanceof ApiError ? cause.message : "Generate all could not be loaded.");
      });
  }, []);

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    abort.current?.abort();
    abort.current = new AbortController();
    setBusy(true);
    setResult(null);
    setTone("info");
    setMessage(mode === "parallel" ? `Generating all reports in parallel with maximum parallelism ${maxParallelism}.` : "Generating all reports sequentially.");
    try {
      const generated = await apiPost<GenerateAllResult>(
        "/api/reports/all",
        { mode, maxParallelism, classYear },
        abort.current.signal,
      );
      setResult(generated);
      setTone(generated.statusTone);
      setMessage(`Generate all ${generated.status.toLowerCase()}. Successful ${generated.successful} of ${generated.total}. Failed ${generated.failed}. Duration ${generated.duration}.`);
    } catch (cause) {
      if (cause instanceof DOMException && cause.name === "AbortError") {
        setTone("info");
        setMessage("Generate all was cancelled.");
        return;
      }

      setTone("error");
      setMessage(cause instanceof ApiError ? cause.message : "Generate all failed.");
    } finally {
      setBusy(false);
    }
  };

  const related = [
    ...(user?.capabilities.canGenerate ? [{ to: "/generate", label: "Generate one report" }] : []),
    { to: "/runs", label: "View history" },
    ...(user?.capabilities.canImport ? [{ to: "/import", label: "Import data" }] : []),
  ];

  return (
    <>
      <header className="page-header page-hero">
        <p className="eyebrow">Bulk run</p>
        <h1>Generate all reports</h1>
        <p className="lede">Create a summary PDF for every school that has graduate records. One failed school does not stop the others.</p>
        <PageLinks label="Related pages" links={related} />
      </header>
      {eligibleCount === 0 ? (
        <section className="panel empty-panel" aria-labelledby="generate-all-empty-heading">
          <h2 id="generate-all-empty-heading">No schools ready</h2>
          <p>Import a graduate workbook before generating reports.</p>
          <PageLinks
            label="Next steps"
            links={[
              ...(user?.capabilities.canImport ? [{ to: "/import", label: "Import data", primary: true }] : []),
              { to: "/", label: "Back to dashboard" },
            ]}
          />
        </section>
      ) : (
        <form className="workbench" aria-label="Generate all school reports" aria-busy={busy} onSubmit={(event) => void onSubmit(event)}>
          <div className="field-grid">
            <div className="field">
              <label htmlFor="all-report-year">Class year</label>
              <select id="all-report-year" required disabled={busy} value={classYear} onChange={(event) => setClassYear(event.target.value)}>
                {classYears.map((year) => (
                  <option key={year} value={year}>
                    Class of {year}
                  </option>
                ))}
              </select>
              <p className="hint">Years come from imported school records and earlier reports. The printed PDF title stays Class of 2025.</p>
            </div>
            <div className="field">
              <label htmlFor="max-parallelism">Maximum parallelism</label>
              <input
                id="max-parallelism"
                type="number"
                min={limits.min}
                max={limits.max}
                disabled={busy || mode !== "parallel"}
                value={maxParallelism}
                onChange={(event) => setMaxParallelism(Number(event.target.value))}
              />
              <p className="hint">
                Allowed range is {limits.min} to {limits.max}. Sequential mode always uses 1.
              </p>
            </div>
          </div>
          <fieldset>
            <legend>Generation mode</legend>
            <div className="choice-grid">
              <label className={`choice-card${mode === "sequential" ? " is-selected" : ""}`}>
                <input type="radio" name="generation-mode" value="sequential" checked={mode === "sequential"} disabled={busy} onChange={() => setMode("sequential")} />
                <span>
                  <strong>Sequential</strong>
                  <span>One school at a time. Easier to watch and cancel.</span>
                </span>
              </label>
              <label className={`choice-card${mode === "parallel" ? " is-selected" : ""}`}>
                <input type="radio" name="generation-mode" value="parallel" checked={mode === "parallel"} disabled={busy} onChange={() => setMode("parallel")} />
                <span>
                  <strong>Parallel</strong>
                  <span>Bounded concurrency. Faster for many schools.</span>
                </span>
              </label>
            </div>
          </fieldset>
          <div className="actions">
            <button type="submit" disabled={busy}>
              {busy ? "Generating all reports" : "Generate all reports"}
            </button>
            <button type="button" className="button-secondary" disabled={!busy} onClick={() => abort.current?.abort()}>
              Cancel generation
            </button>
          </div>
          <p className="hint">
            {eligibleCount} eligible {eligibleCount === 1 ? "school" : "schools"} for Class of {classYear}. This can take several minutes. Cancel stops the request from this browser.
          </p>
        </form>
      )}
      <LiveStatus id="generate-all-status" tone={tone} message={message} />
      {result ? (
        <section className="panel" aria-labelledby="generate-all-result-heading">
          <h2 id="generate-all-result-heading">Run result</h2>
          <p>
            <StatusBadge status={result.status} tone={result.statusTone} />
          </p>
          <div className="card-grid">
            <div className="card">
              <h3>Total</h3>
              <p className="metric">{result.total}</p>
            </div>
            <div className="card">
              <h3>Successful</h3>
              <p className="metric">{result.successful}</p>
            </div>
            <div className="card">
              <h3>Failed</h3>
              <p className="metric">{result.failed}</p>
            </div>
            <div className="card">
              <h3>Duration</h3>
              <p className="metric">{result.duration}</p>
            </div>
          </div>
          {result.message ? <p>{result.message}</p> : null}
          <p className="actions">
            <Link className="button" to={`/runs?run=${result.runId}`}>
              View this run in history
            </Link>
            {user?.capabilities.canGenerate ? (
              <Link className="button-secondary" to="/generate">
                Generate one report
              </Link>
            ) : null}
          </p>
          {result.items.length > 0 ? (
            <div className="table-wrap">
              <table>
                <caption>School results ({result.items.length})</caption>
                <thead>
                  <tr>
                    <th scope="col">School</th>
                    <th scope="col">Status</th>
                    <th scope="col">Links</th>
                  </tr>
                </thead>
                <tbody>
                  {result.items.map((item) => (
                    <tr key={`${item.schoolId}-${item.reportRunItemId ?? item.runId}`}>
                      <td>{item.schoolLabel}</td>
                      <td>
                        <StatusBadge status={item.status} tone={item.statusTone} />
                      </td>
                      <td>
                        <span className="inline-links">
                          {item.downloadUrl ? (
                            <a href={item.downloadUrl} download target="_blank" rel="noopener">
                              Download PDF for {item.schoolLabel}
                            </a>
                          ) : null}
                          {item.reportRunItemId ? (
                            <Link to={`/reports/${item.reportRunItemId}`}>Report details for {item.schoolLabel}</Link>
                          ) : null}
                          {item.reportRunItemId ? (
                            <Link to={`/knowledge-assistant?report=${item.reportRunItemId}`}>Ask about {item.schoolLabel}</Link>
                          ) : null}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : null}
        </section>
      ) : null}
    </>
  );
}
