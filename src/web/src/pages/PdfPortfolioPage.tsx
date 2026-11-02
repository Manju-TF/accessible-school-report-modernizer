import { ChangeEvent, useEffect, useRef, useState } from "react";
import { getDocument, GlobalWorkerOptions, type PDFDocumentLoadingTask } from "pdfjs-dist";
import pdfWorkerUrl from "pdfjs-dist/build/pdf.worker.min.mjs?url";

interface PortfolioAttachment {
  id: string;
  fileName: string;
  content: Uint8Array | null;
}

const maxPortfolioSize = 25 * 1024 * 1024;
const maxAttachmentCount = 50;
const maxAttachmentSize = 25 * 1024 * 1024;
const maxTotalAttachmentSize = 50 * 1024 * 1024;
GlobalWorkerOptions.workerSrc = pdfWorkerUrl;

function isPdf(content: Uint8Array) {
  return content.length >= 5
    && content[0] === 0x25
    && content[1] === 0x50
    && content[2] === 0x44
    && content[3] === 0x46
    && content[4] === 0x2d;
}

function safeFileName(fileName: string) {
  const basename = fileName.split(/[\\/]/).pop() ?? "Embedded document";
  return basename.replace(/[\u0000-\u001f\u007f]/g, "").slice(0, 180) || "Embedded document";
}

function copyToArrayBuffer(content: Uint8Array) {
  const buffer = new ArrayBuffer(content.byteLength);
  new Uint8Array(buffer).set(content);
  return buffer;
}

export function PdfPortfolioPage() {
  const [attachments, setAttachments] = useState<PortfolioAttachment[]>([]);
  const [portfolioName, setPortfolioName] = useState("");
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [selectedAttachmentId, setSelectedAttachmentId] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [status, setStatus] = useState("");
  const [error, setError] = useState("");
  const portfolioTask = useRef<PDFDocumentLoadingTask | null>(null);
  const previewUrlRef = useRef<string | null>(null);
  const requestId = useRef(0);

  const clearPreview = () => {
    if (previewUrlRef.current) {
      URL.revokeObjectURL(previewUrlRef.current);
      previewUrlRef.current = null;
    }

    setPreviewUrl(null);
  };

  const showPdf = (content: BlobPart) => {
    clearPreview();
    const url = URL.createObjectURL(new Blob([content], { type: "application/pdf" }));
    previewUrlRef.current = url;
    setPreviewUrl(url);
  };

  const loadPortfolio = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    event.target.value = "";
    if (!file) {
      return;
    }

    const thisRequest = ++requestId.current;
    void portfolioTask.current?.destroy();
    portfolioTask.current = null;
    clearPreview();
    setAttachments([]);
    setPortfolioName("");
    setSelectedAttachmentId(null);
    setStatus("");
    setError("");

    if (file.size > maxPortfolioSize) {
      setBusy(false);
      setError("This PDF is larger than the 25 MB preview limit.");
      return;
    }

    setBusy(true);
    setStatus("Reading the PDF Portfolio in this browser.");
    try {
      const bytes = new Uint8Array(await file.arrayBuffer());
      if (!isPdf(bytes)) {
        throw new Error("The selected file is not a valid PDF.");
      }

      const task = getDocument({ data: bytes.slice() });
      portfolioTask.current = task;
      const pdf = await task.promise;
      const embeddedFiles = await pdf.getAttachments();
      if (thisRequest !== requestId.current) {
        await task.destroy();
        return;
      }

      const entries = [...(embeddedFiles ?? new Map())];
      if (entries.length > maxAttachmentCount) {
        throw new Error(`This Portfolio contains more than ${maxAttachmentCount} embedded files, above the preview limit.`);
      }

      let totalAttachmentSize = 0;
      const found: PortfolioAttachment[] = [];
      for (const [id, attachment] of entries) {
        const content = await pdf.getAttachmentContent(id);
        const allowed = content !== null
          && content.length <= maxAttachmentSize
          && totalAttachmentSize + content.length <= maxTotalAttachmentSize
          && isPdf(content);

        if (content) {
          totalAttachmentSize += content.length;
        }

        found.push({
          id,
          fileName: safeFileName(attachment.filename || id),
          content: allowed ? content : null,
        });
      }

      if (thisRequest !== requestId.current) {
        await task.destroy();
        return;
      }

      await task.destroy();
      portfolioTask.current = null;
      setPortfolioName(file.name);
      setAttachments(found);
      setSelectedAttachmentId(null);
      showPdf(file);
      setStatus(
        found.length === 0
          ? "PDF loaded. No embedded files were found."
          : `PDF loaded. Found ${found.length} embedded ${found.length === 1 ? "file" : "files"}.`,
      );
    } catch (cause) {
      if (thisRequest !== requestId.current) {
        return;
      }

      setError(cause instanceof Error ? cause.message : "The PDF Portfolio could not be opened.");
      setStatus("");
    } finally {
      if (thisRequest === requestId.current) {
        setBusy(false);
      }
    }
  };

  const previewAttachment = (attachment: PortfolioAttachment) => {
    if (!attachment.content) {
      return;
    }

    setSelectedAttachmentId(attachment.id);
    showPdf(copyToArrayBuffer(attachment.content));
    setStatus(`Previewing ${attachment.fileName}. The file remains in this browser.`);
  };

  useEffect(() => () => {
    requestId.current += 1;
    void portfolioTask.current?.destroy();
    if (previewUrlRef.current) {
      URL.revokeObjectURL(previewUrlRef.current);
    }
  }, []);

  return (
    <>
      <header className="page-header page-hero">
        <p className="eyebrow">Local document preview</p>
        <h1>PDF Portfolio preview</h1>
        <p className="lede">
          Select a PDF Portfolio to preview its pages and embedded PDFs. The file is processed in this browser only;
          it is not uploaded, saved by the application, or sent to the AI provider.
        </p>
      </header>

      <section className="workbench" aria-busy={busy}>
        <div className="field">
          <label htmlFor="portfolio-file">PDF Portfolio</label>
          <input
            id="portfolio-file"
            type="file"
            accept=".pdf,application/pdf"
            disabled={busy}
            onChange={(event) => void loadPortfolio(event)}
          />
          <p className="hint">Maximum file size: 25 MB. Only embedded PDF files can be previewed.</p>
        </div>
        {busy ? <p role="status" aria-live="polite">{status}</p> : null}
        {error ? <p className="live-status tone-error" role="alert">{error}</p> : null}

        {portfolioName ? (
          <div className="portfolio-layout">
            <section className="panel portfolio-files" aria-labelledby="portfolio-files-heading">
              <h2 id="portfolio-files-heading">Embedded files ({attachments.length})</h2>
              {attachments.length === 0 ? <p>No embedded files were found in this PDF.</p> : null}
              {attachments.length > 0 ? (
                <ul>
                  {attachments.map((attachment) => (
                    <li key={attachment.id}>
                      <span>{attachment.fileName}</span>
                      {attachment.content ? (
                        <button
                          type="button"
                          className="button-secondary"
                          aria-pressed={selectedAttachmentId === attachment.id}
                          onClick={() => previewAttachment(attachment)}
                        >
                          Preview PDF
                        </button>
                      ) : (
                        <span className="meta-chip">Not a supported PDF or exceeds the preview limit</span>
                      )}
                    </li>
                  ))}
                </ul>
              ) : null}
            </section>
            <section className="panel portfolio-preview" aria-labelledby="portfolio-preview-heading">
              <h2 id="portfolio-preview-heading">
                {selectedAttachmentId
                  ? attachments.find((attachment) => attachment.id === selectedAttachmentId)?.fileName
                  : portfolioName}
              </h2>
              {previewUrl ? (
                <iframe
                  title={`PDF preview: ${selectedAttachmentId
                    ? attachments.find((attachment) => attachment.id === selectedAttachmentId)?.fileName ?? "embedded document"
                    : portfolioName}`}
                  src={previewUrl}
                />
              ) : (
                <p>Select a PDF to preview.</p>
              )}
            </section>
          </div>
        ) : null}
        <p className="hint" role="status" aria-live="polite">{status}</p>
      </section>
    </>
  );
}
