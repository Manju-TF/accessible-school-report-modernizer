# Authentication foundation

This document describes local sign-in for the capstone web app. It does not claim that generated PDFs are accessible. Report calculations stay in deterministic C# and are not part of this design.

Role authorization, school-level report access, and the Knowledge Assistant are implemented. School grants and retrieval filters are in [`authorization-model.md`](authorization-model.md).

## ASP.NET Core Identity

The app uses **ASP.NET Core Identity** with the existing EF Core SQLite database (`data/schoolreports.db`).

- Store: `SchoolReportsDbContext` inherits `IdentityDbContext<IdentityUser>`.
- User type: `IdentityUser`. No custom `ApplicationUser` was added.
- Tables: standard Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, claims, logins, tokens).
- There is no custom `Users` table and no application-written password column.

Identity is configured in `AccessibleSchoolReports.Web/Security/IdentityAuthenticationExtensions.cs`.

The app does **not** use JWT, OAuth, Microsoft Entra ID, or any external identity provider.

## Cookie authentication

After a successful `SignInManager.PasswordSignInAsync`, Identity writes the authentication ticket to cookie `.asr.auth`.

| Setting | Value |
|---|---|
| Name | `.asr.auth` |
| HttpOnly | true |
| SameSite | Lax |
| SecurePolicy | `SameAsRequest` (Secure on HTTPS) |
| IsEssential | true |
| Sliding expiration | 8 hours |
| Persistent | false (browser session; ticket still expires) |
| Login path | `/signin` |
| Logout path | `/account/signout` |

Anonymous callers are challenged to `/signin`. PDF downloads (`/downloads/reports/{id}`) require an authenticated user.

## Password handling

- Passwords are hashed by ASP.NET Core Identity (`PasswordHasher<IdentityUser>`). The application never stores or compares raw passwords.
- Committed `appsettings.json` keeps `Identity:SeedUserName` and `Identity:SeedPassword` empty.
- Source code does not contain user passwords.
- A development user is created **only** when both seed values are supplied through user secrets or environment variables.

```powershell
dotnet user-secrets set Identity:SeedUserName "dev.user" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedPassword "Replace-This-1!" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedRole "Admin" --project src/AccessibleSchoolReports.Web
```

Identity password rules: at least 8 characters, with upper, lower, digit, and non-alphanumeric. Failed attempts use lockout (5 failures, 5 minutes). The sign-in page does not reveal whether the account is locked.

## Session behavior

- Sign-in is a full-page POST to `/account/signin` with an antiforgery token.
- `isPersistent` is false. Closing the browser ends the cookie; sliding expiration also ends an idle ticket after 8 hours.
- `returnUrl` is accepted only when it is a local path (`/`…, not `//` or an absolute URL).
- Blazor Server circuits use the same cookie. `AuthorizeRouteView` sends unauthenticated users to `/signin`.

## Logout

Sign-out is a POST to `/account/signout` with an antiforgery token. `SignInManager.SignOutAsync` clears the Identity cookie and redirects to `/signin`.

## Development configuration

Prerequisites: .NET 8 SDK. From the repository root:

```powershell
dotnet user-secrets set Identity:SeedUserName "dev.user" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedPassword "Replace-This-1!" --project src/AccessibleSchoolReports.Web
dotnet user-secrets set Identity:SeedRole "Admin" --project src/AccessibleSchoolReports.Web
dotnet run --project src/AccessibleSchoolReports.Web --launch-profile https
```

Use the **https** profile (`https://localhost:7117`) so the cookie is marked Secure. The `http` profile still signs in; `SameAsRequest` will omit Secure on HTTP.

If a local database was created before Identity tables existed, delete the gitignored `data/schoolreports.db` and restart so `MigrateAsync` can apply `AddAspNetIdentity`.

The `Testing` environment skips HTTPS redirection and development user seed. Roles are still created at startup.

## Roles

Startup always ensures Identity roles `Admin`, `ReportUser`, and `Viewer` exist. That seed does not create users or passwords. Pages use `AppPolicies` (`RequireAdmin`, `RequireReportAccess`, `RequireReportGeneration`, `RequireRagAccess`). Role strings are mapped only in `AppAuthorizationPolicies`.

| Role | Dashboard | Import | Generate | Generate all | History / download | Knowledge Assistant |
|---|---|---|---|---|---|---|
| Admin | yes | yes | yes | yes | yes | yes (all authorized scopes, including Admin) |
| ReportUser | yes | no | yes | no | yes | yes (authenticated catalog plus assigned schools) |
| Viewer | yes | no | no | no | yes | yes (authenticated catalog plus assigned schools) |

School-level filtering is enforced in `IReportAuthorizationService` and `UserSchoolAccess`. A ReportUser or Viewer sees only assigned schools. Downloads for an unauthorized report return **404**. The Knowledge Assistant applies `KnowledgeAccess` before retrieval or any language-model call. Details: [`authorization-model.md`](authorization-model.md).

Optional development user: `Identity:SeedRole` must be `Admin`, `ReportUser`, or `Viewer`. Empty defaults to Admin. Development-only.

## Security assumptions

- This is a **standalone local MVP**. Anyone who can open the SQLite file can read password hashes. That is accepted for this capstone.
- Authentication is required by default. `/signin`, `/account/signin`, `/account/signout`, `/Error`, and static files are anonymous.
- Role policies gate pages and downloads. School-level authorization is enforced for generate, history, report details, and downloads.
- Language-model and embedding API keys must stay in user secrets or environment variables, never in the browser or git.
- Do not commit `.env`, user secrets, or a filled-in `Identity:SeedPassword`.
- Do not log passwords or seed secrets.
