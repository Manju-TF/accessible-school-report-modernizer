# Security and accessibility UI review

Playwright MCP end-to-end review of the Blazor Server UI. **No source code was changed.** Findings below are for human review. Do not treat this document as a fix list that has already been applied.

This review does **not** claim that generated PDFs are accessible (PDF/UA). It is a web UI review only. Report calculations were not exercised.

## Method

| Item | Value |
|---|---|
| Date | 2026-09-04 |
| Tool | Playwright MCP (`plugin-playwright-playwright`) |
| Host | `http://127.0.0.1:5199` (https profile not used; cookie `Secure` was therefore not required) |
| Database | Isolated temp SQLite. The repo working file `data/schoolreports.db` was **not** used. That file still has the older `UserSchoolAccess(EntraObjectId, SchoolCode)` shape and was left untouched. |
| Accounts | Local Identity users created only in the temp database: `review.viewer` (Viewer), `review.report` (ReportUser), `review.admin` (Admin). Passwords are not recorded here. |
| Grants | Viewer and ReportUser: school **10701** only. Admin: no grant row. |
| Reports | Report id **1** = 10701 Quinnipiac University School of Law. Report id **2** = 23306 Hofstra University Maurice A. Deane School of Law. |
| RAG providers | `Embeddings:ApiKey` and `LanguageModel:ApiKey` were empty. Live grounded answers and source cards could not be produced. |

Observed results are from the accessibility tree (`browser_snapshot`), DOM inspection (`browser_evaluate`), keyboard (`Tab` / `Enter`), and same-origin `fetch` of download URLs (cookies included).

## Summary

| Area | Observed |
|---|---|
| Authentication | Login, failed login, logout, and anonymous challenge behaved as specified. |
| Authorization | Role nav and page gates matched policies. Unauthorized report details and PDF downloads did not reveal School B. |
| Reports | Authorized PDF **200** `application/pdf`. Unauthorized / static `output/` / traversal **404**. |
| RAG | Assistant page loads. Tampered `reportId` did not bind School B. A real answer and source list were **not** observed (LLM not configured). |
| Accessibility | Landmarks, skip link, labels, and live errors are present. Two UI issues and one unconfirmed loading-state observation are listed under Findings. |

## Authentication

### Protected page without login

`GET /` as anonymous redirected to `/signin?returnUrl=%2F`. Title: **Sign in**. After logout, `GET /knowledge-assistant` redirected to `/signin?returnUrl=%2Fknowledge-assistant`.

### Login

Valid Viewer credentials opened `/` (**Dashboard**) and showed `Signed in as review.viewer`.

Valid ReportUser credentials honored the local `returnUrl` and opened `/knowledge-assistant`.

Valid Admin credentials opened `/`.

### Invalid credentials

Wrong password redirected to `/signin?error=1`. Live region (`role="status"`, `aria-live="polite"`): **Error: The user name or password is not correct.** Message does not say whether the user name exists. Session did not open a protected page.

### Logout

**Sign out** posted and landed on `/signin`. Subsequent protected navigation required sign-in again.

## Authorization

### Viewer

Main nav: Dashboard, Run History, Knowledge Assistant, Sign out. **No** Import Data, Generate Report, or Generate All.

| URL | Result |
|---|---|
| `/import` | `/denied?returnUrl=%2Fimport` — Access denied |
| `/generate` | `/denied?returnUrl=%2Fgenerate` — Access denied |
| `/generate-all` | `/denied?returnUrl=%2Fgenerate-all` — Access denied |
| `/reports/1` | Report details for 10701 — Quinnipiac University School of Law |
| `/reports/2` | Title **Report not found**. Copy: **That report is not available.** No Hofstra, no `23306`, no output path |

Denied page copy: “Your account does not have permission to use that page.” It does not name the required role.

### ReportUser

Main nav: Dashboard, **Generate Report**, Run History, Knowledge Assistant. **No** Import Data or Generate All.

| URL | Result |
|---|---|
| `/import` | Access denied |
| `/generate` | **Generate Report** (200) |
| `/generate-all` | Access denied |
| `/reports/2` | Report not found; no Hofstra / `23306` |

Generate Report had no school picker because the temp database has no graduate rows. School B names were not shown.

### Admin

Main nav: Dashboard, **Import Data**, **Generate Report**, **Generate All**, Run History, Knowledge Assistant.

| URL | Result |
|---|---|
| `/import` | Import Data (200) |
| `/generate-all` | Generate All (200) |
| `/reports/2` | Authorized details for 23306 — Hofstra University Maurice A. Deane School of Law |

### Admin-only navigation

Nav `AuthorizeView` hid Import / Generate All from Viewer and ReportUser. Admin saw both.

The Dashboard (Viewer) and Generate Report (ReportUser) pages still contained an **Import Data** link to `/import`. Following it as those roles reached Access denied. See Finding F-01.

### Unauthorized report access

Viewer and ReportUser on `/reports/2`: not-found page only. Admin on `/reports/2`: School B details and download link.

## Reports (PDF)

Same-origin `fetch` while signed in as Viewer:

| Request | Status | Notes |
|---|---|---|
| `/downloads/reports/1/10701-summary-report.pdf` | **200** | `application/pdf`; `filename=10701-summary-report.pdf`; body starts `%PDF` |
| `/downloads/reports/2/23306-summary-report.pdf` | **404** | Empty body; not PDF; no Hofstra / `23306` / `output` |
| `/output/2025/10701/summary-report.pdf` | **404** | Physical output directory is not statically served |
| `/downloads/reports/..%2F..%2Fsecret.pdf` | **404** | Traversal URL did not return a file |

Admin `GET /downloads/reports/2/23306-summary-report.pdf`: **200** `application/pdf`, `filename=23306-summary-report.pdf`.

Run History as Viewer listed **Show schools in run 1 (1)** and, when expanded, only 10701. Hofstra / `23306` were absent. The run header still said **Total 2. Successful 2.** See Finding F-02.

## RAG

### Knowledge Assistant

`/knowledge-assistant` is available to Viewer, ReportUser, and Admin. Accessibility tree: heading **Legacy Knowledge Assistant**, form **Ask the knowledge assistant**, labelled **Question** textarea (`required`, `maxlength=4000`, `aria-describedby` help, `aria-errormessage` status), fieldset **Suggested questions**, **Ask**, **Cancel** (disabled when idle), empty `role="status"` live region.

Page HTML did not contain `ApiKey`, `sk-`, or `Bearer `.

### Ask question

Suggested question **How is salary suppression handled?** then **Ask**.

Live status: **Error: The language model is not configured.**

No answer section, no Sources list, no Hofstra / `23306`. The error does not include a key value.

Empty **Ask** (blank textarea): the browser native `required` constraint focused `#assistant-question`. The custom LiveStatus message and `aria-invalid="true"` were **not** observed. See Finding F-03.

### Source display

**Not observed.** There was no successful grounded answer, so source cards (Rule ID, document name, location) were never rendered. This is an environment gap, not a pass.

### Report-specific RAG

From `/reports/1`, **Ask about this report** navigated to `/knowledge-assistant?report=1`.

Banner **Asking about this report** limited retrieval to **10701 — Quinnipiac University School of Law**. Help text: “Questions are answered only from this authorized report. Other schools and global documents are not searched.” No Hofstra / `23306`.

### Change reportId

1. Direct Viewer open of `/knowledge-assistant?report=2`: no report banner. Status **Error: That report is not available.** No Hofstra / `23306`.
2. After a valid `report=1` context, navigate to `/knowledge-assistant?report=2`: banner cleared. Same error. Help text returned to the global authorized-knowledge wording.

Unauthorized information was not displayed.

### Unauthorized information

Across Viewer/ReportUser report details, downloads, assistant, and tampered `reportId`, School B name and code did not appear. Admin could see School B, as intended.

## Accessibility

### Accessibility tree

Consistent structure: banner, skip link, `navigation "Main"`, `main#main-content`, contentinfo. `html lang="en"`. Page titles: Sign in, Dashboard, Access denied, Report / Report not found, Legacy Knowledge Assistant, Import Data, Generate Report, Generate All, Run History.

Sign-in and assistant forms are named. Question and credential fields appear as labelled textboxes in the tree.

### Keyboard navigation

Sign-in tab order: **Skip to main content** → Sign in (nav) → User name → Password → Sign in (submit).

**Enter** on the skip link set `location.hash` to `#main-content` and moved focus to `main` (`tabindex="-1"`). Observed.

### Form labels

Sign-in: `label for="username"` / `for="password"`; `autocomplete="username"` and `current-password`; password `type="password"`.

Assistant: `label for="assistant-question"`; suggested questions in a named group.

### Focus

Skip-link target works. Empty native-required Ask moved focus to the textarea. Custom invalid-question focus + `aria-invalid` was not reached (see F-03).

### Loading state

Markup supports `aria-busy` on the form, Ask label **Asking**, and enabling **Cancel**. The LLM configuration error returned immediately, so the busy transition was **not** captured in this pass. Not recorded as a defect.

### Error state

Failed sign-in and assistant errors use `LiveStatus` (`role="status"`, `aria-live="polite"`, `aria-atomic="true"`) with an **Error:** prefix. Generic credential message. Unauthorized reportId uses the same pattern.

### Answer status

After a failed Ask, status was the configuration error. No **Answer** / **Sources** / **Insufficient evidence** headings appeared. Answer-status behavior after a successful LLM call was **not** reviewed.

## Findings

Do not fix these in this pass. Waiting for human review.

### F-01 — Non-admin pages link to Admin-only Import

**Where:** Viewer Dashboard (“Use Import Data to load…”); ReportUser Generate Report (“Go to Import Data”).

**Observed:** Those roles have no Import nav item, but in-page links go to `/import` and then `/denied`.

**Why it matters:** Extra hops to an error page. The link implies the action is available.

### F-02 — Run totals include unauthorized schools

**Where:** Viewer Run History, run 1.

**Observed:** Header **Total 2. Successful 2.** Expand control **Show schools in run 1 (1)** listed only 10701. School B was not named.

**Why it matters:** A Viewer can infer that another school exists on the same run.

### F-03 — Empty Ask relies on native validation only

**Where:** Knowledge Assistant, empty Question, **Ask**.

**Observed:** Browser required-field handling focused the textarea. Live status stayed empty. `aria-invalid` stayed `false`. The in-page “Enter a question before asking.” path did not run.

**Why it matters:** Users who do not get a native tooltip may hear no error. The custom status region is unused for this case.

### F-04 — Physical database path in the UI

**Where:** Every page footer (“Working database: …”) and Dashboard “Database location”.

**Observed:** Full filesystem path of the SQLite file, including the temp review path on this host.

**Why it matters:** Path disclosure on a shared or demo machine. Not a credential leak. Confirm whether this is acceptable for the capstone demo.

### F-05 — Console parse error on first load

**Where:** Anonymous `/signin` (and later navigations).

**Observed:** Browser console: `Unexpected token '<'` (1 error). Network list for that session showed `app.css`, Blazor, and `download-report.js` as 200/304.

**Why it matters:** Unconfirmed. Could be a missing script returning HTML. Needs a stack trace before treating as a product defect.

## Not verified

- Live embedding / LLM Ask, source cards, insufficient-evidence heading, Cancel-during-request, and focus move to the Answer heading.
- HTTPS / `Secure` cookie on `:7117`.
- The operator’s working `data/schoolreports.db` (older grant table).
- Screen reader (NVDA/JAWS) or automated axe/PAC.
- PDF/UA or visual parity of generated PDFs.
- Mobile viewport (window was not resized for a second pass).

## Wait

No code changes were made for these findings. Review F-01 through F-05 before any fix work.
