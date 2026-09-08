import { FormEvent, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { apiGet, apiPost, ApiError } from "../api/client";
import type { GenerateResult, SchoolOption } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { LiveStatus, StatusBadge } from "../ui/Status";
import { PageLinks } from "../ui/PageLinks";

interface SchoolsResponse {
  classYear: string;
  classYears: string[];
  schools: SchoolOption[];
}

function pickYear(preferred: string, years: string[]) {
  return years.includes(preferred) ? preferred : (years[0] ?? "2025");
}

export function GeneratePage() {
  const { user } = useAuth();
  const [schools, setSchools] = useState<SchoolOption[]>([]);
  const [allYears, setAllYears] = useState<string[]>(["2025"]);
  const [classYear, setClassYear] = useState("2025");
  const [schoolId, setSchoolId] = useState(0);
  const [busy, setBusy] = useState(false);
  const [tone, setTone] = useState<"info" | "error" | "success" | "warning">("info");
  const [message, setMessage] = useState("");
  const [result, setResult] = useState<GenerateResult | null>(null);

  useEffect(() => {
    void apiGet<SchoolsResponse>("/api/schools")
      .then((data) => {
        const years = data.classYears?.length ? data.classYears : [data.classYear || "2025"];
        setSchools(data.schools);
        setAllYears(years);
        setClassYear(pickYear(data.classYear, years));
        if (data.schools.length === 0) {
          setTone("warning");
          setMessage("No schools with graduate records are available. Import a workbook first.");
        }
      })
      .catch((cause) => {
        setTone("error");
        setMessage(cause instanceof ApiError ? cause.message : "Schools could not be loaded.");
      });
  }, []);

  const selectedSchool = schools.find((school) => school.id === schoolId);
  const yearChoices = useMemo(() => {
    const schoolYears = selectedSchool?.classYears ?? [];
    return schoolYears.length > 0 ? schoolYears : allYears;
  }, [allYears, selectedSchool]);

  useEffect(() => {
    setClassYear((current) => pickYear(current, yearChoices));
  }, [yearChoices]);

  const onSchoolChange = (nextId: number) => {
    setSchoolId(nextId);
    const next = schools.find((school) => school.id === nextId);
    const years = next?.classYears?.length ? next.classYears : allYears;
    setClassYear((current) => pickYear(current, years));
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (schoolId <= 0) {
      setTone("error");
      setMessage("Select a school before generating a report.");
      return;
    }

    setBusy(true);
    setResult(null);
    setTone("info");
    setMessage("Generating the school report.");
    try {
      const generated = await apiPost<GenerateResult>("/api/reports", { schoolId, classYear });
      setResult(generated);
      setTone(generated.statusTone);
      setMessage(`Generation ${generated.status.toLowerCase()} for ${generated.schoolLabel}.`);
    } catch (cause) {
      setTone("error");
      setMessage(cause instanceof ApiError ? cause.message : "Generation failed.");
    } finally {
      setBusy(false);
    }
  };

  const related = [
    ...(user?.capabilities.canGenerateAll ? [{ to: "/generate-all", label: "Generate all reports" }] : []),
    { to: "/runs", label: "View history" },
    ...(user?.capabilities.canImport ? [{ to: "/import", label: "Import data" }] : []),
  ];

  return (
    <>
      <header className="page-header page-hero">
        <p className="eyebrow">Single school</p>
        <h1>Generate report</h1>
        <p className="lede">Create one school summary PDF from stored graduate records. Calculator rules are unchanged.</p>
        <PageLinks label="Related pages" links={related} />
      </header>
      {schools.length === 0 ? (
        <section className="panel empty-panel" aria-labelledby="generate-empty-heading">
          <h2 id="generate-empty-heading">No schools ready</h2>
          <p>Import a graduate workbook before generating a report.</p>
          <PageLinks
            label="Next steps"
            links={[
              ...(user?.capabilities.canImport ? [{ to: "/import", label: "Import data", primary: true }] : []),
              { to: "/", label: "Back to dashboard" },
            ]}
          />
        </section>
      ) : (
        <form className="workbench" aria-label="Generate one school report" aria-busy={busy} onSubmit={(event) => void onSubmit(event)}>
          <div className="field-grid">
            <div className="field">
              <label htmlFor="school">School</label>
              <select id="school" required value={schoolId} disabled={busy} onChange={(event) => onSchoolChange(Number(event.target.value))}>
                <option value={0}>Select a school</option>
                {schools.map((school) => (
                  <option key={school.id} value={school.id}>
                    {school.label}
                  </option>
                ))}
              </select>
              <p className="hint">Only assigned schools with graduate records are listed. Admin sees every school.</p>
            </div>
            <div className="field">
              <label htmlFor="report-year">Class year</label>
              <select id="report-year" required disabled={busy || yearChoices.length === 0} value={classYear} onChange={(event) => setClassYear(event.target.value)}>
                {yearChoices.map((year) => (
                  <option key={year} value={year}>
                    Class of {year}
                  </option>
                ))}
              </select>
              <p className="hint">
                {selectedSchool
                  ? "Years for this school come from imported records and earlier reports."
                  : "Years come from imported school records and earlier reports."}{" "}
                The printed PDF title stays Class of 2025.
              </p>
            </div>
          </div>
          <div className="actions">
            <button type="submit" disabled={busy}>
              {busy ? "Generating school report" : "Generate school report"}
            </button>
          </div>
        </form>
      )}
      <LiveStatus id="generate-status" tone={tone} message={message} />
      {result ? (
        <section className="panel" aria-labelledby="generate-result-heading">
          <h2 id="generate-result-heading">Result</h2>
          <p>
            <StatusBadge status={result.status} tone={result.statusTone} />
          </p>
          <p>
            {result.schoolLabel}. Graduates {result.graduateCount}. Duration {result.duration}.
          </p>
          {result.message ? <p>{result.message}</p> : null}
          {result.downloadUrl && result.reportRunItemId ? (
            <p className="actions">
              <a className="button" href={result.downloadUrl} download target="_blank" rel="noopener">
                Download PDF for {result.schoolLabel}
              </a>
              <Link className="button-secondary" to={`/reports/${result.reportRunItemId}`}>
                Report details
              </Link>
              <Link className="button-secondary" to={`/knowledge-assistant?report=${result.reportRunItemId}`}>
                Ask about this report
              </Link>
              <Link className="button-secondary" to={`/runs?run=${result.runId}`}>
                View in history
              </Link>
            </p>
          ) : null}
        </section>
      ) : null}
    </>
  );
}
