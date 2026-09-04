# UI accessibility review

**Date:** 4 September 2026  
**Method:** Playwright MCP accessibility snapshots and targeted DOM checks against the running app at `http://localhost:5017`.  
**Scope:** Dashboard, Import Data, Generate Report, Generate All, Run History, plus the import → single-school generate → parallel generate-all path.  
**This document lists problems only.** It is not a WCAG audit, not a PDF-UA review, and it does not claim the UI is accessible.

Flow observed: sample `sample-export.xlsx` imported (duplicate of ImportRun 1); school `23306` generated successfully; Generate All started in Parallel (max 4). Progress was observed in-flight; the 189-school run was not waited to completion.

---

## Cross-page

1. **Status live regions are created only after the first message.** On idle Import / Generate / Generate All pages, the accessibility tree has no `role="status"` / `aria-live` node. Some screen readers will miss the first announcement when the region is inserted at the same time as the text.

2. **Validation errors are not bound to the control.** After submitting Import with no file, the tree exposes `status` “Error: Choose an Excel workbook before importing.” The file input has no `aria-invalid`, no `aria-errormessage`, and `aria-describedby` still points only at the format hint. Focus stays on **Import**, not the file control or the error.

3. **Required fields are not marked required.** School, report year, and the Excel file are required by the UI but `required` is false in the DOM.

4. **`<form>` is not a form landmark.** Each form is a `generic` in the tree (no accessible name on the form).

5. **Help text is not the control’s accessible description in the tree.** Hints such as “Accepted format: .xlsx…” and “Only schools with at least one graduate record are listed.” appear as sibling paragraphs. Playwright does not attach them as descriptions on the combobox / textbox / file button.

6. **After navigation, focus is not at the start of the tab order.** `FocusOnNavigate` lands on `h1`. Forward Tab from there never reaches **Skip to main content**. The skip link is also off-screen (`transform` translated up) until focused, so it cannot be pointed at with a mouse.

7. **Current page is not exposed in the accessibility tree.** `aria-current="page"` is present in the DOM on the active `NavLink`, but the snapshot shows a plain `link` with no current-page state.

---

## Import Data

8. **The file control is a `button` named “Excel workbook.”** That name matches the visible label, so the tree also has a separate `generic` “Excel workbook” next to the button. The selected filename (“Selected file: sample-export.xlsx”) is an unbound paragraph: not part of the control name, not `aria-live`, not `aria-describedby`.

9. **Duplicate-import success text is repeated.** After re-importing the sample, the status was: “This file was already imported. This file was already imported as ImportRun 1.” The result region then repeats “Duplicate of import 1.” and the same ImportRun sentence again.

---

## Generate Report

10. **School options are codes only, with broken pluralization.** Combobox options look like `51012 (1 graduates)`. Names are missing from the accessible name. The list is 189 options with no grouping or filter.

11. **“Generate” does not say what will be generated.** The same button name is used on Generate All.

---

## Generate All

12. **Radio labels are announced twice.** Each radio already has the name “Sequential — one school at a time” / “Parallel — bounded concurrency”; the same string is also a sibling `generic`.

13. **Progress is not a progressbar and does not update counts.** During the parallel run the tree had two `status` nodes and no `role="progressbar"`:
    - “Status: Generating all reports in parallel with maximum parallelism 4.”
    - “Progress: generating reports. Counts will appear when the run finishes.”
    `aria-busy="true"` is set on the form. There is no completed / remaining / elapsed update, and no cancel control for a multi-minute job.

14. **Focus is lost when Generate becomes disabled.** After starting the run, `document.activeElement` was `BODY`. The submit button changes to a disabled **Generating** control, so keyboard focus has no place to go.

---

## Run History

15. **`details` / `summary` is not a button in the tree.** “Schools in run 2 (1)” is a `generic` inside a `group`, not a button. Expand/collapse is easy to miss for AT and keyboard users who look for a button role.

16. **The history table inserts a full-width extras row per run.** Each run is followed by a single `cell` with `colspan="7"` wrapping the school `details`. Column headers (Started, Mode, Status, …) do not apply to that row. Nested school tables exist in the DOM even when the details are closed.

---

## Dashboard

17. **Run/import status is a bare `generic` “Completed.”** There is no status role and no name such as “Status: Completed.” The word is visible, so this is not color-only, but the tree does not mark it as a status.

---

## Flow notes (not extra defects)

- Sample import completed as a duplicate (3534 rows / ImportRun 1).
- School `23306` generation completed; download name was “Download PDF for school 23306.”
- Parallel generate-all started; in-progress UI matched items 13–14.
