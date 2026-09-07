---
name: security-auth
description: >-
  Change Identity sign-in, roles, school grants, report downloads, or RAG
  authorization. Use when editing AppPolicies, UserSchoolAccess, SignIn,
  ReportAuthorization, download endpoints, KnowledgeAccess, or when the user
  mentions auth, roles, Admin, Viewer, cookies, or unauthorized reports.
---

# Security and Authorization

Standalone Identity cookie auth. No Entra, OAuth, or custom JWT.

## Roles and policies

| Role | Typical access |
|---|---|
| Admin | All schools; import; generate; RAG |
| ReportUser | Generate/view granted schools; RAG on granted knowledge |
| Viewer | View granted schools; RAG on granted knowledge |

Policies live in `AppPolicies` (`RequireAdmin`, `RequireReportGeneration`, `RequireReportAccess`, `RequireRagAccess`). Pages are authenticated by default.

School grants: `UserSchoolAccess` rows. Admin does not need a row. Unauthorized report ids return **404** (downloads) or “not available” (assistant). Do not leak school names.

## Workflow

1. State files, rules, tests, and risks. Wait if access behavior changes.
2. Enforce authorization in the service, not only the UI.
3. Keep passwords hashed by Identity. Seed users only via `Identity:SeedUserName` / `Identity:SeedPassword` user secrets.
4. Never commit `.env`, connection strings with credentials, or API keys.
5. Do not log passwords, cookies, or bearer tokens.

## Code map

| Concern | Path |
|---|---|
| Policies / roles | `src/AccessibleSchoolReports.Application/Security/` |
| School/report checks | `src/AccessibleSchoolReports.Infrastructure/Security/ReportAuthorizationService.cs` |
| Dev seed (config-only) | `src/AccessibleSchoolReports.Infrastructure/Security/IdentityDevelopmentSeed.cs` |
| Cookie / Identity wiring | `src/AccessibleSchoolReports.Web/Security/` |
| Downloads | `src/AccessibleSchoolReports.Web/Downloads/ReportDownloadEndpoints.cs` |
| Knowledge filters | `src/AccessibleSchoolReports.Application/Knowledge/KnowledgeAccess.cs` |
| Docs | `docs/capstone/authentication.md`, `docs/capstone/authorization-model.md`, `docs/capstone/security-architecture.md` |

## Tests

```powershell
dotnet test tests/AccessibleSchoolReports.UnitTests --filter "FullyQualifiedName~Security"
```

Cover: anonymous redirect, role denial, school grant allow/deny, download 404, assistant report-scope denial. Use isolated test SQLite. Do not point tests at `data/schoolreports.db`.

## Secrets

User secrets on `src/AccessibleSchoolReports.Web` only:

- `Identity:SeedUserName` / `Identity:SeedPassword` / `Identity:SeedRole`
- `LanguageModel:ApiKey` (and endpoint/model)
- `Embeddings:ApiKey` only if the embedding provider is not Lexical

Committed `appsettings.json` keeps those values empty.
