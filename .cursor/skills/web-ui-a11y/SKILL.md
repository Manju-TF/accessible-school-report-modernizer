---
name: web-ui-a11y
description: >-
  Change Blazor Server UI while keeping WCAG 2.2 AA, Meridian Test Client
  branding, and verified browser behavior. Use when editing .razor pages, app.css,
  NavMenu, SignIn, Knowledge Assistant, LiveStatus, or when the user mentions
  layout, accessibility, suggested questions, or chrome.
---

# Web UI and Accessibility

Blazor Server app branded **Meridian Test Client**. Target WCAG 2.2 AA in the UI. Do not claim generated PDFs are accessible from a UI change.

## Hard rules

- Visible name and labels: **Meridian Test Client**. Do not print professional-association names in chrome.
- Year chrome in UI copy that describes the report: **Class of 2025**, July **2026**.
- Do not invent school names. Use stored school code + name.
- Missing printed values stay `.` with accessible name **Not displayed**.
- Keep focus, names, and status messages on interactive controls. Prefer existing `LiveStatus` for results.
- Assistant answers: render through `AssistantAnswerHtml` (Markdig, HTML disabled). Do not bind raw model Markdown into a `<p>`.

## Workflow

1. State files, rules, tests, and risks. UI-only work still needs a plan if behavior or access copy changes.
2. Edit the smallest page/CSS surface. Bump the cache query on `app.css` in `Components/App.razor` when styles change (`app.css?v=…`).
3. Add or update page tests under `tests/AccessibleSchoolReports.UnitTests/Security/` or `Ui/` when chrome, suggestions, or authz copy changes.
4. **Verify in the browser** before declaring UI work done: sign in, exercise the changed flow, and check other routes that share the same state. A screenshot is not enough.
5. If the app is already running, stop `AccessibleSchoolReports.Web` before rebuild (file lock / MSB3027). HTTPS profile: `https://localhost:7117`.

## Common pages

| Page | Route |
|---|---|
| Dashboard | `/` |
| Sign in | `/signin` |
| Import | `/import` (Admin) |
| Generate | `/generate` |
| History | `/runs` |
| Report details | `/reports/{id}` |
| Assistant | `/knowledge-assistant` and `?report={id}` |

Suggested questions: `src/AccessibleSchoolReports.Web/Ui/AssistantSuggestions.cs`. Groups are collapsible `<details>`. Each group must have at least five questions. Global = General + Business rules + All generated reports. Report-scoped = This report only.

## Tests

```powershell
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~PageTests|FullyQualifiedName~Assistant|FullyQualifiedName~UiFormat"
```

## Stop and ask

Do not add a new SPA, CSS framework, or client-side auth. Do not put connection strings or API keys in the UI.
