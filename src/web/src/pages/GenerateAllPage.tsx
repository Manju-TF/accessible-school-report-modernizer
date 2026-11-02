import { FormEvent, useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { apiGet, apiPostStream, ApiError } from "../api/client";
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

interface GenerationProgress {
  total: number;
  completed: number;
  successful: number;
  failed: number;
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
  const [generationProgress, setGenerationProgress] = useState<GenerationProgress | null>(null);
  const [tone, setTone] = useState<"info" | "error" | "success" | "warning">("info");
  const [message, setMessage] = useState("");
  const [result, setResult] = useState<GenerateAllResult | null>(null);
  const [downloadedIds, setDownloadedIds] = useState<string[]>([]);
  const [downloadErrors, setDownloadErrors] = useState<Record<string, string>>({});
  const [downloadStatus, setDownloadStatus] = useState("");
  const [activeDownloadId, setActiveDownloadId] = useState<string | null>(null);
  const abort = useRef<AbortController | null>(null);
  const downloadInProgress = useRef(false);

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
    setGenerationProgress({ total: eligibleCount, completed: 0, successful: 0, failed: 0 });
    setResult(null);
    setDownloadedIds([]);
    setDownloadErrors({});
    setDownloadStatus("");
    setTone("info");
    setMessage(mode === "parallel" ? `Generating all reports in parallel with maximum parallelism ${maxParallelism}.` : "Generating all reports sequentially.");
    try {
      const generated = await apiPostStream<GenerateAllResult, GenerationProgress>(
        "/api/reports/all/progress",
        { mode, maxParallelism, classYear },
        setGenerationProgress,
        abort.current.signal,
      );
      setResult(generated);
      setDownloadedIds([]);
      setDownloadErrors({});
      const downloadableCount = generated.items.filter((item) => item.downloadUrl).length;
      setDownloadStatus(`Downloaded 0 of ${downloadableCount} reports. ${downloadableCount} pending.`);
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

  const downloadReport = async (item: GenerateResult, itemKey: string) => {
    if (!item.downloadUrl || downloadInProgress.current) {
      return;
    }

    downloadInProgress.current = true;
    setActiveDownloadId(itemKey);
    setDownloadErrors((current) => {
      const next = { ...current };
      delete next[itemKey];
      return next;
    });
    setDownloadStatus(
      `Downloading PDF for ${item.schoolLabel}. ${downloadedIds.length} of ${result?.items.filter((report) => report.downloadUrl).length ?? 0} downloaded; ${Math.max(0, (result?.items.filter((report) => report.downloadUrl).length ?? 0) - downloadedIds.length)} pending.`,
    );

    try {
      const response = await fetch(item.downloadUrl, {
        credentials: "include",
        headers: { Accept: "application/pdf" },
      });
      if (!response.ok) {
        throw new ApiError(
          response.status === 401
            ? "Your session has expired. Sign in again to download this report."
            : response.status === 403
              ? "You do not have permission to download this report."
              : "The report download failed.",
          response.status,
        );
      }

      const contentType = response.headers.get("Content-Type")?.split(";")[0].trim().toLowerCase();
      if (contentType !== "application/pdf") {
        throw new Error("The server did not return a PDF. Sign in again and retry the download.");
      }

      const pdf = await response.blob();
      const fileName = decodeURIComponent(new URL(item.downloadUrl, window.location.origin).pathname.split("/").pop() || "report.pdf");
      const downloadUrl = URL.createObjectURL(pdf);
      const link = document.createElement("a");
      link.href = downloadUrl;
      link.download = fileName;
      link.hidden = true;
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.setTimeout(() => URL.revokeObjectURL(downloadUrl), 1000);

      const nextDownloadedIds = downloadedIds.includes(itemKey)
        ? downloadedIds
        : [...downloadedIds, itemKey];
      setDownloadedIds(nextDownloadedIds);
      const totalDownloads = result?.items.filter((report) => report.downloadUrl).length ?? 0;
      setDownloadStatus(
        `Downloaded ${nextDownloadedIds.length} of ${totalDownloads} reports. ${totalDownloads - nextDownloadedIds.length} pending.`,
      );
    } catch (cause) {
      const downloadError = cause instanceof Error ? cause.message : "The report download failed.";
      setDownloadErrors((current) => ({ ...current, [itemKey]: downloadError }));
      const totalDownloads = result?.items.filter((report) => report.downloadUrl).length ?? 0;
      setDownloadStatus(
        `Could not download ${item.schoolLabel}. ${downloadedIds.length} of ${totalDownloads} downloaded; ${totalDownloads - downloadedIds.length} pending. ${downloadError}`,
      );
    } finally {
      downloadInProgress.current = false;
      setActiveDownloadId(null);
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
        <form className="workbench" aria-label="Generate all school reports" aria-busy={busy || activeDownloadId !== null} onSubmit={(event) => void onSubmit(event)}>
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
            <button type="submit" disabled={busy || activeDownloadId !== null}>
              {busy ? "Generating all reports" : "Generate all reports"}
            </button>
            <button type="button" className="button-secondary" disabled={!busy} onClick={() => abort.current?.abort()}>
              Cancel generation
            </button>
          </div>
          {busy && generationProgress ? (
            <section className="download-progress" aria-labelledby="generate-all-progress-heading">
              <h3 id="generate-all-progress-heading">Report generation progress</h3>
              <p role="status" aria-live="polite">
                {generationProgress.completed} of {generationProgress.total} reports completed.
                {" "}{Math.max(0, generationProgress.total - generationProgress.completed)} remaining.
                {" "}Successful {generationProgress.successful}; failed {generationProgress.failed}.
              </p>
              <progress
                value={generationProgress.completed}
                max={Math.max(1, generationProgress.total)}
                aria-label={`${generationProgress.completed} of ${generationProgress.total} reports generated`}
              />
            </section>
          ) : null}
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
          {result.items.some((item) => item.downloadUrl) ? (
            <section className="download-progress" aria-labelledby="generate-all-download-heading">
              <h3 id="generate-all-download-heading">Report downloads</h3>
              <p role="status" aria-live="polite">{downloadStatus}</p>
              <progress
                value={downloadedIds.length}
                max={result.items.filter((item) => item.downloadUrl).length}
                aria-label={`${downloadedIds.length} of ${result.items.filter((item) => item.downloadUrl).length} reports downloaded`}
              />
            </section>
          ) : null}
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
                    <th scope="col">Download status</th>
                    <th scope="col">Links</th>
                  </tr>
                </thead>
                <tbody>
                  {result.items.map((item) => {
                    const itemKey = `${item.schoolId}-${item.reportRunItemId ?? item.runId}`;
                    const isDownloaded = downloadedIds.includes(itemKey);
                    const isDownloading = activeDownloadId === itemKey;
                    return (
                      <tr key={itemKey}>
                        <td>{item.schoolLabel}</td>
                        <td>
                          <StatusBadge status={item.status} tone={item.statusTone} />
                        </td>
                        <td>
                          {item.downloadUrl
                            ? downloadErrors[itemKey]
                              ? "Failed; pending retry"
                              : isDownloading
                                ? "Downloading"
                                : isDownloaded
                                  ? "Downloaded"
                                  : "Pending"
                            : "Not available"}
                        </td>
                        <td>
                          <span className="inline-links">
                            {item.downloadUrl ? (
                              <a
                                href={item.downloadUrl}
                                download
                                aria-disabled={activeDownloadId !== null}
                                onClick={(event) => {
                                  event.preventDefault();
                                  void downloadReport(item, itemKey);
                                }}
                              >
                                {isDownloading ? "Downloading..." : isDownloaded ? "Download again" : `Download PDF for ${item.schoolLabel}`}
                              </a>
                            ) : null}
                            {downloadErrors[itemKey] ? <span className="download-error">{downloadErrors[itemKey]}</span> : null}
                            {item.reportRunItemId ? (
                              <Link to={`/reports/${item.reportRunItemId}`}>Report details for {item.schoolLabel}</Link>
                            ) : null}
                            {item.reportRunItemId ? (
                              <Link to={`/knowledge-assistant?report=${item.reportRunItemId}`}>Ask about {item.schoolLabel}</Link>
                            ) : null}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          ) : null}
        </section>
      ) : null}
    </>
  );
}
