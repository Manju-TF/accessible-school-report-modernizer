# Live demo script

Run this after slide 27. Have `https://localhost:7117` signed out or ready to sign in.

```powershell
dotnet run --project src/AccessibleSchoolReports.Web --launch-profile https
```

If the React UI was edited since the last Kestrel start, run `npm run build` in `src/web` and hard-refresh.

Do **not** re-import a large dummy workbook unless you need to show duplicate-file reject. Do **not** generate all 189 schools during the demo.

---

## 1. Sign in (Admin) — 30 seconds

- Open `/signin`.
- Sign in as the seeded Admin.
- Point out **Meridian Test Client** chrome and Class of 2025 footer.
- Say: cookie Identity, not Entra / JWT login.

## 2. Dashboard — 90 seconds

- Show school and graduate headcounts from **stored imports**, not printed report totals.
- Switch **Class year** (All years / Class of 2025 / prior years).
- Point at year comparison and largest schools.
- Say: duplicate uploads do not change these counts.

## 3. Import (Admin only) — 60 seconds

- Open **Import data**.
- Show required `.xlsx` and validation copy.
- If a file was already imported, show last successful import and mention content-hash reject.
- Do not write anything back to `/legacy`.

## 4. Generate one school — 2 minutes

- Open **Generate report**.
- Pick a stored school and year **2025**.
- Generate. Open the PDF.
- Show: seven letter pages, gray headers, `Total Reported` on page 1, note then footer, `.` for missing values.
- Say: layout follows the SAS / GrayscalePrinter baseline. Accessibility is **tags**, not a new look. **Not certified.**

## 5. History and download — 60 seconds

- Open **History**.
- Show run status, school count, duration.
- Download through `/downloads/reports/{id}` (authorized).
- Open **Report details**.

## 6. Knowledge Assistant (this report) — 90 seconds

- From report details, **Ask about this report**.
- Use a suggested question: *What is Total Reported in this report?*
- Show the answer and sources.
- Say: retrieval is limited to that authorized PDF. The assistant does **not** recalculate SAS.

## 7. Knowledge Assistant (all reports) — 90 seconds

- Open **Assistant** (global).
- Show suggested groups: All generated reports, Compare reports, General. No Business-rules prompts that do not answer.
- Ask a sum or difference question only if ingest is idle and you have time.
- If the model is unavailable, show **Insufficient evidence** or the user-facing error. Do not invent numbers.

## 8. Authorization (Viewer) — 90 seconds

- Sign out. Sign in as Viewer (or a user with no school grants).
- Dashboard is empty or limited to granted schools.
- **Import** and **Generate all** are denied.
- An unauthorized report id returns **404**, not 403 with metadata.

## 9. Evidence (optional, 60 seconds)

- Show `.github/workflows/quality.yml`.
- Show `docs/decisions/rejected-ai-proposals.md` (`n < 5` salaries rejected).
- Show `docs/capstone/business-rules.md` Rule ID `CF-S-00`.

## Close

Totals come from `SchoolReportCalculator`. PDFs target tagged PDF / PDF-UA. Validation is separate. `/legacy` was never edited.
