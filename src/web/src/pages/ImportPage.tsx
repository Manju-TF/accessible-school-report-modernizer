import { DragEvent, FormEvent, useRef, useState } from "react";
import { apiPostForm, ApiError } from "../api/client";
import { LiveStatus, StatusBadge } from "../ui/Status";

const maxFileBytes = 10 * 1024 * 1024;

interface ImportResult {
  status: string;
  statusTone: "success" | "warning" | "error" | "info";
  importedRowCount: number;
  invalidRowCount: number;
  blankRowCount: number;
  message: string;
  issues: { rowNumber: number; reason: string }[];
  issueCount: number;
}

function isExcelWorkbook(file: File) {
  return file.name.toLowerCase().endsWith(".xlsx");
}

export function ImportPage() {
  const [file, setFile] = useState<File | null>(null);
  const [busy, setBusy] = useState(false);
  const [dragging, setDragging] = useState(false);
  const [tone, setTone] = useState<"info" | "error" | "success" | "warning">("info");
  const [message, setMessage] = useState("");
  const [result, setResult] = useState<ImportResult | null>(null);
  const input = useRef<HTMLInputElement | null>(null);
  const chooseButton = useRef<HTMLButtonElement | null>(null);
  const dragDepth = useRef(0);

  const acceptFile = (next: File | null) => {
    if (!next) {
      setFile(null);
      return;
    }

    if (!isExcelWorkbook(next)) {
      setTone("error");
      setMessage("The selected file must be an .xlsx workbook.");
      return;
    }

    if (next.size > maxFileBytes) {
      setTone("error");
      setMessage("The file is larger than 10 MB or could not be read.");
      return;
    }

    setFile(next);
    setTone("info");
    setMessage(`Selected file: ${next.name}.`);
  };

  const onDragEnter = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
    if (busy) {
      return;
    }

    dragDepth.current += 1;
    setDragging(true);
  };

  const onDragOver = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
    if (!busy) {
      event.dataTransfer.dropEffect = "copy";
    }
  };

  const onDragLeave = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
    dragDepth.current = Math.max(0, dragDepth.current - 1);
    if (dragDepth.current === 0) {
      setDragging(false);
    }
  };

  const onDrop = (event: DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
    dragDepth.current = 0;
    setDragging(false);
    if (!busy) {
      acceptFile(event.dataTransfer.files.item(0));
    }
  };

  const onSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!file) {
      setTone("error");
      setMessage("Choose an Excel workbook before importing.");
      chooseButton.current?.focus();
      return;
    }

    setBusy(true);
    setMessage(`Importing ${file.name}.`);
    setTone("info");
    setResult(null);
    try {
      const form = new FormData();
      form.set("file", file);
      const imported = await apiPostForm<ImportResult>("/api/imports", form);
      setResult(imported);
      setTone(imported.statusTone);
      setMessage(imported.message);
    } catch (cause) {
      setTone("error");
      setMessage(cause instanceof ApiError ? cause.message : "Import failed.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <header className="page-header">
        <p className="eyebrow">Workbook</p>
        <h1>Import Data</h1>
        <p className="lede">Upload a graduate Excel workbook. The importer validates required columns and stores accepted rows in SQLite.</p>
      </header>
      <form className="workbench" aria-label="Import graduate workbook" aria-busy={busy} onSubmit={(event) => void onSubmit(event)}>
        <div className="field">
          <span id="import-file-label">Excel workbook</span>
          <div
            className={`file-drop${file ? " has-file" : ""}${dragging ? " is-dragging" : ""}${busy ? " is-disabled" : ""}`}
            role="group"
            aria-labelledby="import-file-label"
            onDragEnter={onDragEnter}
            onDragOver={onDragOver}
            onDragLeave={onDragLeave}
            onDrop={onDrop}
          >
            <input
              ref={input}
              id="import-file"
              className="visually-hidden"
              type="file"
              accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
              tabIndex={-1}
              disabled={busy}
              aria-hidden="true"
              onChange={(event) => acceptFile(event.target.files?.[0] ?? null)}
            />
            <p className="file-drop-prompt">{dragging ? "Drop to select" : "Drag and drop an .xlsx file"}</p>
            <p className="file-drop-or">or</p>
            <button
              ref={chooseButton}
              type="button"
              className="button-secondary"
              disabled={busy}
              aria-describedby="import-file-help"
              onClick={() => input.current?.click()}
            >
              Choose file
            </button>
            <p id="import-file-help" className="hint">
              {file ? file.name : ".xlsx, up to 10 MB"}
            </p>
          </div>
        </div>
        <div className="actions">
          <button type="submit" disabled={busy}>
            {busy ? "Importing" : "Import"}
          </button>
        </div>
      </form>
      <LiveStatus id="import-status" tone={tone} message={message} />
      {result ? (
        <section className="panel" aria-labelledby="import-result-heading">
          <h2 id="import-result-heading">Import result</h2>
          <p>
            <StatusBadge status={result.status} tone={result.statusTone} />
          </p>
          <dl className="stat-list">
            <div>
              <dt>Imported rows</dt>
              <dd>{result.importedRowCount}</dd>
            </div>
            <div>
              <dt>Invalid rows</dt>
              <dd>{result.invalidRowCount}</dd>
            </div>
            <div>
              <dt>Blank rows</dt>
              <dd>{result.blankRowCount}</dd>
            </div>
          </dl>
          {result.issues.length > 0 ? (
            <div className="table-wrap">
              <table>
                <caption>Validation issues ({result.issueCount})</caption>
                <thead>
                  <tr>
                    <th scope="col">Row</th>
                    <th scope="col">Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {result.issues.map((issue) => (
                    <tr key={`${issue.rowNumber}-${issue.reason}`}>
                      <td>{issue.rowNumber}</td>
                      <td>{issue.reason}</td>
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
