import { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { apiGet, ApiError } from "../api/client";
import type { DashboardResponse, DashboardSchoolRow } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { LiveStatus, StatusBadge } from "../ui/Status";
import { PageLinks } from "../ui/PageLinks";

const allYearsKey = "all";
const refreshMs = 20000;

function schoolCountForYear(school: DashboardSchoolRow, yearKey: string) {
  return yearKey === allYearsKey ? school.graduateCount : (school.yearCounts[yearKey] ?? 0);
}

function formatChange(change?: number | null) {
  if (change == null) {
    return "First recorded year";
  }

  if (change === 0) {
    return "No change from the previous year";
  }

  return `${change > 0 ? "+" : ""}${change} graduates from the previous year`;
}

export function DashboardPage() {
  const { user } = useAuth();
  const [data, setData] = useState<DashboardResponse | null>(null);
  const [error, setError] = useState("");
  const [yearKey, setYearKey] = useState(allYearsKey);
  const [query, setQuery] = useState("");
  const [sort, setSort] = useState<"graduates" | "school">("graduates");
  const [refreshing, setRefreshing] = useState(false);
  const capabilities = user?.capabilities;

  const load = useCallback(async (silent = false) => {
    if (!silent) {
      setRefreshing(true);
    }

    try {
      const next = await apiGet<DashboardResponse>("/api/dashboard");
      setData(next);
      setError("");
      setYearKey((current) => {
        if (current === allYearsKey) {
          return current;
        }

        return next.years.some((year) => year.key === current) ? current : allYearsKey;
      });
    } catch (cause) {
      setError(cause instanceof ApiError ? cause.message : "The dashboard could not be loaded.");
    } finally {
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    const onVisible = () => {
      if (document.visibilityState === "visible") {
        void load(true);
      }
    };

    const timer = window.setInterval(() => {
      if (document.visibilityState === "visible") {
        void load(true);
      }
    }, refreshMs);

    window.addEventListener("focus", onVisible);
    document.addEventListener("visibilitychange", onVisible);
    return () => {
      window.clearInterval(timer);
      window.removeEventListener("focus", onVisible);
      document.removeEventListener("visibilitychange", onVisible);
    };
  }, [load]);

  const selectedYear = data?.years.find((year) => year.key === yearKey);
  const filteredSchools = useMemo(() => {
    if (!data) {
      return [];
    }

    const needle = query.trim().toLowerCase();
    const rows = data.schools
      .map((school) => ({ school, count: schoolCountForYear(school, yearKey) }))
      .filter((row) => row.count > 0 || yearKey === allYearsKey)
      .filter((row) => !needle || row.school.label.toLowerCase().includes(needle) || row.school.code.toLowerCase().includes(needle));

    rows.sort((left, right) => {
      if (sort === "school") {
        return left.school.code.localeCompare(right.school.code);
      }

      return right.count - left.count || left.school.code.localeCompare(right.school.code);
    });
    return rows;
  }, [data, query, sort, yearKey]);

  const visibleSchools = filteredSchools.filter((row) => row.count > 0);
  const visibleGraduates = visibleSchools.reduce((sum, row) => sum + row.count, 0);
  const average = visibleSchools.length === 0 ? 0 : Math.round(visibleGraduates / visibleSchools.length);
  const yearMax = Math.max(1, ...(data?.years.map((year) => year.graduateCount) ?? [1]));
  const schoolMax = Math.max(1, ...filteredSchools.slice(0, 10).map((row) => row.count));

  if (error && !data) {
    return (
      <>
        <header className="page-header">
          <h1>Dashboard</h1>
        </header>
        <LiveStatus tone="error" message={error} />
      </>
    );
  }

  if (!data) {
    return (
      <>
        <header className="page-header">
          <h1>Dashboard</h1>
        </header>
        <LiveStatus tone="info" message="Loading dashboard." />
      </>
    );
  }

  return (
    <>
      <header className="page-header page-hero">
        <p className="eyebrow">Organization analytics</p>
        <h1>Dashboard</h1>
        <p className="lede">
          Live school and graduate headcounts from stored imports. Duplicate uploads do not change these numbers. These
          counts are not printed report totals.
        </p>
        <p className="hint" aria-live="polite">
          Updated {data.generatedAtUtc}.{refreshing ? " Refreshing." : ""}
        </p>
        <div className="dashboard-toolbar">
          <PageLinks
            label="Common tasks"
            links={[
              ...(capabilities?.canGenerate ? [{ to: "/generate", label: "Generate report", primary: true }] : []),
              ...(capabilities?.canGenerateAll ? [{ to: "/generate-all", label: "Generate all reports" }] : []),
              ...(capabilities?.canImport ? [{ to: "/import", label: "Import data" }] : []),
              ...(capabilities?.canAsk ? [{ to: "/knowledge-assistant", label: "Ask the assistant" }] : []),
            ]}
          />
          <button type="button" className="button-secondary" onClick={() => void load()} disabled={refreshing}>
            {refreshing ? "Refreshing analytics" : "Refresh analytics"}
          </button>
        </div>
      </header>
      {error ? <LiveStatus tone="error" message={error} /> : null}

      <fieldset className="year-filter">
        <legend>Class year</legend>
        <div className="year-filter-options">
          <label className={yearKey === allYearsKey ? "is-selected" : undefined}>
            <input type="radio" name="dashboard-year" checked={yearKey === allYearsKey} onChange={() => setYearKey(allYearsKey)} />
            All years
          </label>
          {data.years.map((year) => (
            <label key={year.key} className={yearKey === year.key ? "is-selected" : undefined}>
              <input type="radio" name="dashboard-year" checked={yearKey === year.key} onChange={() => setYearKey(year.key)} />
              {year.label}
            </label>
          ))}
        </div>
      </fieldset>

      <div className="card-grid">
        <section className="card" aria-labelledby="school-count-heading">
          <h2 id="school-count-heading">Schools with records</h2>
          <p className="metric">{visibleSchools.length}</p>
          <p className="metric-note">
            {yearKey === allYearsKey ? `${data.schoolCount} schools in scope` : selectedYear?.label}
          </p>
        </section>
        <section className="card" aria-labelledby="graduate-count-heading">
          <h2 id="graduate-count-heading">Graduates</h2>
          <p className="metric">{visibleGraduates}</p>
          <p className="metric-note">{yearKey === allYearsKey ? "All stored class years" : selectedYear?.label}</p>
        </section>
        <section className="card" aria-labelledby="average-heading">
          <h2 id="average-heading">Average per school</h2>
          <p className="metric">{average}</p>
          <p className="metric-note">Among schools that have records in this view</p>
        </section>
        <section className="card" aria-labelledby="db-status-heading">
          <h2 id="db-status-heading">Database status</h2>
          <p className="metric">{data.databaseStatus}</p>
          <p className="metric-note">{data.databaseDetail}</p>
        </section>
      </div>

      <div className="dashboard-split">
      <section className="panel" aria-labelledby="year-compare-heading">
        <h2 id="year-compare-heading">Year comparison</h2>
        {data.years.length === 0 ? (
          <p>No graduate records are stored yet.</p>
        ) : (
          <div className="table-wrap">
            <table>
              <caption>Graduate headcounts by class year</caption>
              <thead>
                <tr>
                  <th scope="col">Year</th>
                  <th scope="col">Schools</th>
                  <th scope="col">Graduates</th>
                  <th scope="col">Share</th>
                  <th scope="col">Change</th>
                  <th scope="col">Distribution</th>
                </tr>
              </thead>
              <tbody>
                {data.years.map((year) => (
                  <tr key={year.key}>
                    <th scope="row">{year.label}</th>
                    <td>{year.schoolCount}</td>
                    <td>{year.graduateCount}</td>
                    <td>{year.sharePercent}%</td>
                    <td>{formatChange(year.graduateChange)}</td>
                    <td>
                      <div className="bar-track" aria-hidden="true">
                        <span className="bar-fill" style={{ width: `${Math.max(4, (year.graduateCount / yearMax) * 100)}%` }} />
                      </div>
                      <span className="visually-hidden">
                        {year.graduateCount} of {yearMax} in the largest year
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <section className="panel" aria-labelledby="top-schools-heading">
        <h2 id="top-schools-heading">Largest schools</h2>
        {visibleSchools.length === 0 ? (
          <p>No schools have graduate records in this view.</p>
        ) : (
          <ol className="rank-list">
            {filteredSchools.slice(0, 10).map((row) => (
              <li key={row.school.id}>
                <div className="rank-row">
                  <span>{row.school.label}</span>
                  <strong>{row.count}</strong>
                </div>
                <div className="bar-track" aria-hidden="true">
                  <span className="bar-fill" style={{ width: `${Math.max(4, (row.count / schoolMax) * 100)}%` }} />
                </div>
              </li>
            ))}
          </ol>
        )}
      </section>
      </div>

      <section className="panel" aria-labelledby="school-compare-heading">
        <div className="panel-head">
          <h2 id="school-compare-heading">Compare schools</h2>
          <div className="table-controls">
            <div className="field">
              <label htmlFor="school-filter">Find a school</label>
              <input
                id="school-filter"
                value={query}
                onChange={(event) => setQuery(event.target.value)}
                autoComplete="off"
              />
            </div>
            <fieldset className="sort-filter">
              <legend>Sort</legend>
              <label>
                <input type="radio" name="school-sort" checked={sort === "graduates"} onChange={() => setSort("graduates")} />
                Most graduates
              </label>
              <label>
                <input type="radio" name="school-sort" checked={sort === "school"} onChange={() => setSort("school")} />
                School code
              </label>
            </fieldset>
          </div>
        </div>
        {filteredSchools.length === 0 ? (
          <p>No schools match that search.</p>
        ) : (
          <div className="table-wrap table-wrap-tall">
            <table>
              <caption>
                {yearKey === allYearsKey
                  ? "Graduate records by school and class year"
                  : `Graduate records for ${selectedYear?.label ?? "the selected year"}`}
              </caption>
              <thead>
                <tr>
                  <th scope="col">School</th>
                  {yearKey === allYearsKey
                    ? data.years.map((year) => (
                        <th key={year.key} scope="col">
                          {year.label}
                        </th>
                      ))
                    : null}
                  <th scope="col">{yearKey === allYearsKey ? "Total" : "Graduates"}</th>
                </tr>
              </thead>
              <tbody>
                {filteredSchools.map((row) => (
                  <tr key={row.school.id}>
                    <th scope="row">{row.school.label}</th>
                    {yearKey === allYearsKey
                      ? data.years.map((year) => <td key={year.key}>{row.school.yearCounts[year.key] ?? 0}</td>)
                      : null}
                    <td>{row.count}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <div className="activity-grid">
        <section className="panel" aria-labelledby="last-import-heading">
          <h2 id="last-import-heading">Last successful import</h2>
          {data.lastImport ? (
            <>
              <p>
                <StatusBadge status={data.lastImport.status} tone={data.lastImport.statusTone} /> {data.lastImport.fileName}
              </p>
              <p>
                Started {data.lastImport.startedUtc}. Imported {data.lastImport.importedRowCount} rows. Invalid {data.lastImport.invalidRowCount}.
                Blank {data.lastImport.blankRowCount}. A repeat upload of the same file does not add rows.
              </p>
            </>
          ) : (
            <p>
              No import has been run yet. Use <Link to="/import">Import data</Link> to load an Excel workbook.
            </p>
          )}
        </section>
        <section className="panel" aria-labelledby="last-run-heading">
          <h2 id="last-run-heading">Last report run</h2>
          {data.lastRun ? (
            <>
              <p>
                <StatusBadge status={data.lastRun.status} tone={data.lastRun.statusTone} /> {data.lastRun.mode}
              </p>
              <p>
                Started {data.lastRun.startedUtc}. Total {data.lastRun.totalCount}. Successful {data.lastRun.successfulCount}. Failed {data.lastRun.failedCount}.
                Duration {data.lastRun.duration}.
              </p>
              <p>
                <Link to="/runs">View run history</Link>
              </p>
            </>
          ) : (
            <p>
              No report has been generated yet. Use <Link to="/generate">Generate report</Link> or <Link to="/generate-all">Generate all reports</Link>.
            </p>
          )}
        </section>
      </div>
    </>
  );
}
