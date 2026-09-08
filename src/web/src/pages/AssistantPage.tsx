import { FormEvent, useEffect, useLayoutEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { Link, useSearchParams } from "react-router-dom";
import { apiGet, apiPost, ApiError } from "../api/client";
import type { AssistantAnswer } from "../api/types";
import { renderAnswer } from "../assistant/markdown";
import { suggestionGroups } from "../assistant/suggestions";
import { useAuth } from "../auth/AuthContext";
import { RobotMark } from "../ui/RobotMark";
import { LiveStatus } from "../ui/Status";

interface ReportContext {
  reportId: number;
  schoolLabel: string;
  reportYear?: number;
}

function groupKey(title: string) {
  return title.toLowerCase().replace(/[^a-z0-9]+/g, "-");
}

export function AssistantPage() {
  const { user } = useAuth();
  const [params] = useSearchParams();
  const reportId = Number(params.get("report") ?? "") || undefined;
  const [context, setContext] = useState<ReportContext | null>(null);
  const [question, setQuestion] = useState("");
  const [asked, setAsked] = useState("");
  const [busy, setBusy] = useState(false);
  const [pickerOpen, setPickerOpen] = useState(true);
  const [openGroup, setOpenGroup] = useState("All generated reports");
  const [tone, setTone] = useState<"info" | "error" | "success" | "warning">("info");
  const [message, setMessage] = useState("");
  const [answer, setAnswer] = useState<AssistantAnswer | null>(null);
  const abort = useRef<AbortController | null>(null);
  const thread = useRef<HTMLDivElement | null>(null);
  const fab = useRef<HTMLButtonElement | null>(null);
  const dialog = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    if (!reportId) {
      setContext(null);
      return;
    }

    void apiGet<{ report: ReportContext }>(`/api/assistant/context?report=${reportId}`)
      .then((data) => setContext(data.report))
      .catch((cause) => {
        setContext(null);
        setTone("error");
        setMessage(cause instanceof ApiError ? cause.message : "That report is not available.");
      });
  }, [reportId]);

  useEffect(() => {
    thread.current?.lastElementChild?.scrollIntoView({ block: "nearest" });
  }, [asked, busy, answer]);

  useEffect(() => {
    if (!pickerOpen) {
      return;
    }

    const root = dialog.current;
    const closeButton = root?.querySelector<HTMLElement>(".question-dialog-close");
    closeButton?.focus();

    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        event.preventDefault();
        setPickerOpen(false);
      }
    };

    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("keydown", onKey);
      fab.current?.focus();
    };
  }, [pickerOpen]);

  useLayoutEffect(() => {
    if (!pickerOpen) {
      return;
    }

    const place = () => {
      const root = dialog.current;
      const anchor = fab.current;
      if (!root || !anchor) {
        return;
      }

      const page = document.getElementById("main-content");
      const hero = document.querySelector(".assistant-hero");
      const box = anchor.getBoundingClientRect();
      const ceiling = Math.max(
        12,
        (hero?.getBoundingClientRect().bottom ?? page?.getBoundingClientRect().top ?? 0) + 8,
      );
      const floor = box.top - 8;
      root.style.position = "fixed";
      root.style.right = `${Math.max(12, window.innerWidth - box.right)}px`;
      root.style.bottom = `${window.innerHeight - box.top + 8}px`;
      root.style.left = "auto";
      root.style.top = "auto";
      root.style.maxHeight = `${Math.max(240, floor - ceiling)}px`;
    };

    place();
    const frame = requestAnimationFrame(place);
    window.addEventListener("resize", place);
    return () => {
      cancelAnimationFrame(frame);
      window.removeEventListener("resize", place);
    };
  }, [pickerOpen, openGroup]);

  const groups = suggestionGroups(Boolean(context));

  useEffect(() => {
    setOpenGroup(suggestionGroups(Boolean(context))[0]?.title ?? "General");
  }, [context]);
  const initial = user?.userName.slice(0, 1).toUpperCase() ?? "Y";
  const hasThread = Boolean(asked || busy || answer);

  const closePicker = () => setPickerOpen(false);

  const send = async (event?: FormEvent, text?: string) => {
    event?.preventDefault();
    const next = (text ?? question).trim();
    if (!next) {
      return;
    }

    setPickerOpen(false);
    abort.current?.abort();
    abort.current = new AbortController();
    setQuestion("");
    setAsked(next);
    setBusy(true);
    setAnswer(null);
    setTone("info");
    setMessage("Asking the knowledge assistant.");
    try {
      const result = await apiPost<AssistantAnswer>(
        "/api/assistant/ask",
        { question: next, reportId: context?.reportId },
        abort.current.signal,
      );
      setAnswer(result);
      setTone(result.insufficient ? "warning" : "success");
      setMessage(result.message);
    } catch (cause) {
      if (cause instanceof DOMException && cause.name === "AbortError") {
        setTone("info");
        setMessage("The question was cancelled.");
        return;
      }

      setTone("error");
      setMessage(cause instanceof ApiError ? cause.message : "The assistant could not answer. Try again.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="assistant-app">
      <header className="assistant-hero">
        <span className="assistant-hero-mark" aria-hidden="true">
          <RobotMark size={40} />
        </span>
        <div>
          <h1>Knowledge Assistant</h1>
          <p className="lede">Authorized documentation and generated reports only. Does not calculate totals.</p>
        </div>
      </header>
      {context ? (
        <section className="assistant-scope" aria-labelledby="report-context-heading">
          <h2 id="report-context-heading">Asking about this report</h2>
          <p>
            Retrieval is limited to the authorized report for {context.schoolLabel}.
            {context.reportYear ? ` Report year ${context.reportYear}.` : ""}
          </p>
          <p>
            <Link to="/knowledge-assistant">Ask about all authorized knowledge</Link>
          </p>
        </section>
      ) : null}
      <section className="assistant-chat" aria-label="Conversation" aria-busy={busy}>
        <div className="chat-scroll" ref={thread}>
          {hasThread ? (
            <div className="chat-thread">
              {asked ? (
                <article className="chat-turn chat-turn-user">
                  <span className="chat-face" aria-hidden="true">
                    {initial}
                  </span>
                  <div className="chat-bubble">
                    <h2 className="visually-hidden">Your question</h2>
                    <p>{asked}</p>
                  </div>
                </article>
              ) : null}
              {busy ? (
                <article className="chat-turn chat-turn-bot">
                  <span className="chat-face chat-face-bot" aria-hidden="true">
                    <RobotMark size={22} />
                  </span>
                  <div className="chat-bubble">
                    <p className="typing" aria-hidden="true">
                      <span />
                      <span />
                      <span />
                    </p>
                    <p className="visually-hidden">The assistant is preparing an answer.</p>
                  </div>
                </article>
              ) : null}
              {answer?.insufficient ? (
                <article className="chat-turn chat-turn-bot">
                  <span className="chat-face chat-face-bot" aria-hidden="true">
                    <RobotMark size={22} />
                  </span>
                  <div className="chat-bubble">
                    <h2>Insufficient evidence</h2>
                    <p>There is not enough authorized project context to answer this question.</p>
                  </div>
                </article>
              ) : null}
              {answer && !answer.insufficient ? (
                <article className="chat-turn chat-turn-bot">
                  <span className="chat-face chat-face-bot" aria-hidden="true">
                    <RobotMark size={22} />
                  </span>
                  <div className="chat-bubble">
                    <h2 className="visually-hidden">Answer</h2>
                    <div className="assistant-answer" dangerouslySetInnerHTML={{ __html: renderAnswer(answer.answer ?? "") }} />
                    {answer.sources.length > 0 ? (
                      <details className="source-drawer">
                        <summary>Sources ({answer.sources.length})</summary>
                        <ol className="source-list">
                          {answer.sources.map((source) => (
                            <li key={`${source.documentName}-${source.sourceLocation}`} className="source-card">
                              <h3>{source.documentName}</h3>
                              <p className="hint">{source.documentKind}</p>
                              <dl className="stat-list">
                                <div>
                                  <dt>Rule ID</dt>
                                  <dd>{source.ruleId}</dd>
                                </div>
                                <div>
                                  <dt>Source location</dt>
                                  <dd>{source.sourceLocation}</dd>
                                </div>
                                {source.schoolCode ? (
                                  <div>
                                    <dt>School</dt>
                                    <dd>{source.schoolCode}</dd>
                                  </div>
                                ) : null}
                                {source.reportYear ? (
                                  <div>
                                    <dt>Report year</dt>
                                    <dd>{source.reportYear}</dd>
                                  </div>
                                ) : null}
                              </dl>
                            </li>
                          ))}
                        </ol>
                      </details>
                    ) : null}
                  </div>
                </article>
              ) : null}
            </div>
          ) : (
            <div className="chat-empty">
              <span className="chat-empty-mark" aria-hidden="true">
                <RobotMark size={72} />
              </span>
              <p>Ask about authorized reports or project documentation.</p>
            </div>
          )}
          <LiveStatus id="assistant-status" tone={tone} message={message} />
        </div>
        <form
          className="composer"
          aria-label="Ask the knowledge assistant"
          onSubmit={(event) => void send(event)}
        >
          <label htmlFor="assistant-question" className="visually-hidden">
            Message
          </label>
          <textarea
            id="assistant-question"
            rows={1}
            maxLength={4000}
            required
            disabled={busy}
            value={question}
            placeholder="Message the assistant…"
            onChange={(event) => setQuestion(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter" && !event.shiftKey) {
                event.preventDefault();
                void send();
              }
            }}
          />
          <button type="submit" disabled={busy}>
            {busy ? "Asking" : "Ask"}
          </button>
          <div className="composer-end">
            <button
              ref={fab}
              type="button"
              className="question-fab"
              aria-expanded={pickerOpen}
              aria-controls="question-dialog"
              onClick={() => setPickerOpen((open) => !open)}
            >
              <RobotMark size={52} />
              <span className="visually-hidden">{pickerOpen ? "Close suggested questions" : "Open suggested questions"}</span>
            </button>
            <button type="button" className="button-secondary" disabled={!busy} onClick={() => abort.current?.abort()}>
              Cancel
            </button>
          </div>
          <p className="visually-hidden">
            {context
              ? "Answers use only this authorized report. Other schools and global documents are not searched."
              : "Answers use knowledge you are allowed to see. Unauthorized school or admin-only text is not retrieved."}
          </p>
        </form>
      </section>
      {pickerOpen
        ? createPortal(
            <>
              <button type="button" className="question-dialog-backdrop" aria-label="Close suggested questions" onClick={closePicker} />
              <div
                ref={dialog}
                id="question-dialog"
                className="question-dialog"
                role="dialog"
                aria-modal="true"
                aria-labelledby="question-dialog-title"
              >
                <div className="question-dialog-head">
                  <span className="question-dialog-face" aria-hidden="true">
                    <RobotMark size={18} />
                  </span>
                  <h2 id="question-dialog-title">Suggested questions</h2>
                  <button type="button" className="question-dialog-close" onClick={closePicker} aria-label="Close">
                    ×
                  </button>
                </div>
                <div className="suggested-questions">
                  <div
                    className="suggestion-nav"
                    role="tablist"
                    aria-label="Question groups"
                    onKeyDown={(event) => {
                      if (event.key !== "ArrowDown" && event.key !== "ArrowUp") {
                        return;
                      }

                      event.preventDefault();
                      const index = groups.findIndex((group) => group.title === openGroup);
                      const next = event.key === "ArrowDown"
                        ? (index + 1) % groups.length
                        : (index - 1 + groups.length) % groups.length;
                      const title = groups[next]?.title ?? openGroup;
                      setOpenGroup(title);
                      requestAnimationFrame(() => {
                        dialog.current?.querySelector<HTMLElement>(`#suggestion-tab-${groupKey(title)}`)?.focus();
                      });
                    }}
                  >
                    {groups.map((group) => {
                      const selected = openGroup === group.title;
                      return (
                        <button
                          key={group.title}
                          type="button"
                          role="tab"
                          id={`suggestion-tab-${groupKey(group.title)}`}
                          aria-selected={selected}
                          aria-controls="suggestion-panel"
                          tabIndex={selected ? 0 : -1}
                          onClick={() => setOpenGroup(group.title)}
                        >
                          <span>{group.title}</span>
                          <span className="suggestion-count">
                            {group.questions.length}
                            <span className="visually-hidden"> questions</span>
                          </span>
                        </button>
                      );
                    })}
                  </div>
                  {groups
                    .filter((group) => group.title === openGroup)
                    .map((group) => (
                      <div
                        key={group.title}
                        className="suggestion-panel"
                        role="tabpanel"
                        id="suggestion-panel"
                        aria-labelledby={`suggestion-tab-${groupKey(group.title)}`}
                      >
                        <h3>{group.title}</h3>
                        <p className="suggestion-hint">{group.hint}</p>
                        <ul className="suggested-question-list">
                          {group.questions.map((item) => (
                            <li key={item}>
                              <button
                                type="button"
                                className="suggested-question"
                                disabled={busy}
                                onClick={() => void send(undefined, item)}
                              >
                                {item}
                              </button>
                            </li>
                          ))}
                        </ul>
                      </div>
                    ))}
                </div>
              </div>
            </>,
            document.body,
          )
        : null}
    </div>
  );
}
