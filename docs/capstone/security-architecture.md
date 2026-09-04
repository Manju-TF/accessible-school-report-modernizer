# Security architecture

Authentication is documented in `docs/capstone/authentication.md`.

This app uses **ASP.NET Core Identity** and **cookie** authentication on the existing SQLite database. Custom JWT authentication, plaintext passwords, and external identity providers are not used.

## Policies

Role-to-policy mapping lives only in `AppAuthorizationPolicies`. Pages and endpoints use `AppPolicies` names, not role strings.

Authentication is required by default (`FallbackPolicy`). Anonymous access is explicit (`[AllowAnonymous]`).

| Policy | Roles | Surfaces |
|---|---|---|
| `RequireAdmin` | Admin | Import Data, Generate All |
| `RequireReportGeneration` | Admin, ReportUser | Generate Report |
| `RequireReportAccess` | Admin, ReportUser, Viewer | Dashboard, Run History, PDF downloads |
| `RequireRagAccess` | Admin, ReportUser, Viewer | Knowledge Assistant (`/knowledge-assistant`) |

School-level grants are documented in `docs/capstone/authorization-model.md`. Generated PDFs are not served as static files; `IReportDownloadService` authorizes the report id, then reads the stored path under `OutputRoot`. Knowledge retrieval applies `KnowledgeAccess` before scoring or calling the language model. Observed security and RAG runs are in `evidence/test-results/`.

This document does not claim that generated PDFs are accessible. Report calculations stay in deterministic C#. Observed suite output: `evidence/test-results/security-test-results.md`.
