---
name: web-ui-a11y
description: >-
  Change the React SPA UI while keeping WCAG 2.2 AA, Meridian Test Client
  branding, and verified browser behavior. Use when editing src/web pages,
  styles, routing, or when the user mentions layout, accessibility, suggested
  questions, or chrome.
---

# Web UI and Accessibility

Same-origin React + Vite SPA in `src/web`, hosted by ASP.NET Core. Target WCAG 2.2 AA in the UI. Do not claim generated PDFs are accessible from a UI change.

## Hard rules

- Visible name and labels: **Meridian Test Client**. Do not print professional-association names in chrome.
- Year chrome in UI copy that describes the report: **Class of 2025**, July **2026**.
- Do not invent school names. Use stored school code + name.
- Missing printed values stay `.` with accessible name **Not displayed**.
- Keep focus, names, and status messages on interactive controls. Use `LiveStatus` for results.
- Assistant answers: render Markdown through `assistant/markdown.ts` (DOMPurify). Do not inject raw model HTML.

## Workflow

1. State files, rules, tests, and risks. UI-only work still needs a plan if behavior or access copy changes.
2. Edit `src/web`. JSON APIs live in `src/AccessibleSchoolReports.Web/Api/`.
3. Dev: run Kestrel (`https://localhost:7117`) and `npm run dev` in `src/web` (Vite proxies `/api`, `/account`, `/downloads`).
4. **Verify in the browser** before declaring UI work done.
5. Authorization is enforced on APIs and on the SPA HTML routes. Do not hide a page in React only.

## Common pages

| Page | Route |
|---|---|
| Dashboard | `/` |
| Sign in | `/signin` |
| Import | `/import` (Admin) |
| Generate | `/generate` |
| History | `/runs` |
| Report details | `/reports/:id` |
| Assistant | `/knowledge-assistant` and `?report=` |

Suggested questions: `src/web/src/assistant/suggestions.ts`. Groups are collapsible. Each group must have at least five questions.

## Tests

```powershell
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~PageTests|FullyQualifiedName~SpaApi|FullyQualifiedName~Assistant"
```
