# Application security test results

Automated suite: `tests/AccessibleSchoolReports.UnitTests/Security/ApplicationSecuritySuiteTests.cs`. This file lives at `evidence/test-results/security-test-results.md`.

This document records **runner output**, not assumed pass. Successful cases below were observed on this machine. This document does not claim that generated PDFs are accessible. Report calculations stay in deterministic C#.

## Command

```text
dotnet test tests/AccessibleSchoolReports.UnitTests/AccessibleSchoolReports.UnitTests.csproj --filter "FullyQualifiedName~ApplicationSecuritySuite"
```

## Run

| Field | Value |
|---|---|
| Date | 2026-09-04 |
| Host | Windows 10 (win32 10.0.26100) |
| Project | `AccessibleSchoolReports.UnitTests` (`net8.0`) |
| Filter | `FullyQualifiedName~ApplicationSecuritySuite` |
| Runner summary | `Test Run Successful. Total tests: 32. Passed: 32. Failed: 0. Total time: 20.5123 Seconds` |

xUnit reported 32 tests because cases 21–23 are one `[Theory]` with three `InlineData` values (`sas`, `markdown`, `pdf`).

## Results

| # | Requirement | Test | Result | Duration |
|---|---|---|---|---|
| 1 | Anonymous user cannot access protected pages | `Case01_AnonymousUserCannotAccessProtectedPages` | **PASS** | 8 ms |
| 2 | Valid user can authenticate | `Case02_ValidUserCanAuthenticate` | **PASS** | 311 ms |
| 3 | Invalid credentials are rejected | `Case03_InvalidCredentialsAreRejected` | **PASS** | 235 ms |
| 4 | Logout invalidates authenticated access | `Case04_LogoutInvalidatesAuthenticatedAccess` | **PASS** | 4 s |
| 5 | Protected endpoints require authentication | `Case05_ProtectedEndpointsRequireAuthentication` | **PASS** | 4 ms |
| 6 | Viewer cannot access Admin functions | `Case06_ViewerCannotAccessAdminFunctions` | **PASS** | 236 ms |
| 7 | ReportUser cannot access Admin functions | `Case07_ReportUserCannotAccessAdminFunctions` | **PASS** | 200 ms |
| 8 | Admin can access Admin functions | `Case08_AdminCanAccessAdminFunctions` | **PASS** | 307 ms |
| 9 | User cannot access an unauthorized school | `Case09_UserCannotAccessAnUnauthorizedSchool` | **PASS** | 283 ms |
| 10 | User cannot access an unauthorized report | `Case10_UserCannotAccessAnUnauthorizedReport` | **PASS** | 255 ms |
| 11 | Unauthorized PDF download is denied | `Case11_UnauthorizedPdfDownloadIsDenied` | **PASS** | 207 ms |
| 12 | Path traversal is denied | `Case12_PathTraversalIsDenied` | **PASS** | 302 ms |
| 13 | Physical output directory is not publicly browsable | `Case13_PhysicalOutputDirectoryIsNotPubliclyBrowsable` | **PASS** | 1 s |
| 14 | Invalid report IDs do not reveal filesystem information | `Case14_InvalidReportIdsDoNotRevealFilesystemInformation` | **PASS** | 266 ms |
| 15 | Unauthorized knowledge chunks are never retrieved | `Case15_UnauthorizedKnowledgeChunksAreNeverRetrieved` | **PASS** | 586 ms |
| 16 | Unauthorized PDF chunks are never retrieved | `Case16_UnauthorizedPdfChunksAreNeverRetrieved` | **PASS** | 579 ms |
| 17 | Unauthorized content never reaches the LLM | `Case17_UnauthorizedContentNeverReachesTheLlm` | **PASS** | 946 ms |
| 18 | User A cannot query User B's authorized reports | `Case18_UserACannotQueryUserBAuthorizedReports` | **PASS** | 505 ms |
| 19 | School A user cannot retrieve School B content | `Case19_SchoolAUserCannotRetrieveSchoolBContent` | **PASS** | 529 ms |
| 20 | Report-specific RAG cannot be escaped by changing reportId | `Case20_ReportSpecificRagCannotBeEscapedByChangingReportId` | **PASS** | 639 ms |
| 21 | Malicious SAS text cannot override system instructions | `Case21To23_…(kind: "sas")` | **PASS** | < 1 ms |
| 22 | Malicious Markdown cannot override system instructions | `Case21To23_…(kind: "markdown")` | **PASS** | < 1 ms |
| 23 | Malicious PDF text cannot override system instructions | `Case21To23_…(kind: "pdf")` | **PASS** | 13 ms |
| 24 | API keys are not in source code | `Case24_ApiKeysAreNotInSourceCode` | **PASS** | 5 ms |
| 25 | API keys are not logged | `Case25_ApiKeysAreNotLogged` | **PASS** | 21 ms |
| 26 | API keys are not exposed to browser/client code | `Case26_ApiKeysAreNotExposedToBrowserClientCode` | **PASS** | 342 ms |
| 27 | Secret configuration is excluded from Git | `Case27_SecretConfigurationIsExcludedFromGit` | **PASS** | 218 ms |
| 28 | Empty question rejected | `Case28_EmptyQuestionIsRejected` | **PASS** | 506 ms |
| 29 | Excessively long question handled safely | `Case29_ExcessivelyLongQuestionIsHandledSafely` | **PASS** | 3 s |
| 30 | Cancellation handled | `Case30_CancellationIsHandled` | **PASS** | 538 ms |
| 31 | External API timeout handled | `Case31_ExternalApiTimeoutIsHandled` | **PASS** | 1 s |
| 32 | External API failure handled | `Case32_ExternalApiFailureIsHandled` | **PASS** | 8 ms |

## What each case asserted

### Authentication

1. Anonymous `GET` of `/`, `/import`, `/generate`, `/generate-all`, `/runs`, `/knowledge-assistant`, `/reports/1`, and `/downloads/reports/1` returned **302** to `/signin`.
2. Valid seed credentials set cookie `.asr.auth` and opened `/` as **200**.
3. Wrong password redirected to `/signin?error=1` and `/` still required sign-in.
4. After `POST /account/signout`, `/` redirected to `/signin`.
5. Anonymous `GET /downloads/reports/{authorizedReportA}` redirected to `/signin`.

### Authorization

6. Viewer `GET /import` and `/generate-all` redirected to `/denied`.
7. ReportUser `GET /import` and `/generate-all` redirected to `/denied`.
8. Admin `GET /import` and `/generate-all` returned **200**.
9. Viewer with School A only: `GET /reports/{schoolB}` returned the “Report not found” / “That report is not available.” page. School B name and code `23306` were absent.
10. ReportUser with School A only: `GET /downloads/reports/{schoolB}` returned **404** without School B name or `OutputRoot`.

### PDF security

11. Viewer download of School B returned **404** and was not `application/pdf`.
12. Encoded `../` URLs and a stored path outside `OutputRoot` returned **404** without the physical root.
13. Authenticated `GET /output/2025/10701/summary-report.pdf` was not a PDF (`%PDF` prefix absent; content type not `application/pdf`).
14. Admin `GET /downloads/reports/99999` returned **404** without `OutputRoot`, `summary-report.pdf`, or School A name.

### RAG security

15. ReportUser retrieval did not include the Admin-only chunk or `ADMIN-ONLY-SECRET-TEXT`.
16. ReportUser retrieval did not include the School B PDF chunk or `SCHOOL-B-SECRET-TEXT`.
17. The fake LLM request context did not contain School B or Admin secrets.
18. ReportUser asking with `ReportId` = School B report: empty sources, **0** embedding calls, School B secret not sent to the LLM.
19. ReportUser retrieval hits had no School B `SchoolId` and no School B secret.
20. Session `TrySelectReportAsync` accepted School A then rejected School B (context cleared). Retrieval with tampered School B `ReportId` returned no hits and did not embed.

### Prompt injection

21–23. Retrieved SAS comment, Markdown, and PDF injection strings stayed in the untrusted context document. `SystemInstructions` stayed equal to `KnowledgeGroundedPrompt.SystemInstructions` and did not contain the injection markers.

### Secrets

24. Committed `src/AccessibleSchoolReports.Web/appsettings.json` has empty `Embeddings:ApiKey` and `LanguageModel:ApiKey`. `appsettings.Development.json` has no `ApiKey`. `wwwroot` files contain neither `ApiKey` nor `sk-`.
25. `EmbeddingOptions.ToString()` and `LanguageModelOptions.ToString()` omit `super-secret-key`. A scripted successful LLM completion logged messages that did not contain the key or `Bearer `.
26. Signed-in HTML/CSS for `/`, `/knowledge-assistant`, `/signin`, and `/app.css` did not contain `LanguageModel:ApiKey`, `Embeddings:ApiKey`, or `Bearer `.
27. `.gitignore` includes `.env`, `secrets.json`, and `appsettings.Local.json`. Those files are not present in the repo tree. `git check-ignore -q` returned exit code 0 for `.env`, `secrets.json`, and `appsettings.Local.json`.

### Input

28. Whitespace-only question: `LanguageModelInvoked = false`, **0** LLM calls, **0** embedding calls, empty sources.
29. Question of `MaxQuestionLength + 1` (4001) characters: same as 28 — no LLM, no embed.
30. Pre-cancelled token on `AskAsync` threw `OperationCanceledException` with **0** LLM and **0** embedding calls.
31. Scripted LLM delay of 5 s with `TimeoutSeconds = 1` threw `LanguageModelTimeoutException`.
32. Scripted HTTP **400** threw `LanguageModelProviderException` with `StatusCode = 400` after one request (`MaxRetries = 0`).

## Scope and limits

- HTTP cases use `WebApplicationFactory` (`AllowAutoRedirect = false`), not a live browser or Playwright.
- RAG/LLM cases 15–23, 28–30 use in-process fakes (`FakeEmbeddingService`, `FakeLanguageModelService`) plus seeded School A / School B / Admin documents. No live OpenAI call was made.
- Cases 31–32 use a scripted `HttpMessageHandler`. They prove timeout and non-retryable failure mapping, not a live provider outage.
- Case 24 scans committed Web `appsettings` and `wwwroot` only. It does not prove user-secrets files on disk are empty.
- Case 26 checks rendered HTML/CSS from the test host. It does not inspect a production CDN or browser DevTools on a deployed site.
- Case 27 proves Git ignore rules and that those files are not in the working tree. It does not scan Git history.

## Other test runs on the same machine

These are **not** part of the 32-case suite. Recorded so the security result is not confused with the rest of the repo.

| Project | Command | Runner summary |
|---|---|---|
| Remaining unit tests (suite excluded) | `dotnet test tests/AccessibleSchoolReports.UnitTests/AccessibleSchoolReports.UnitTests.csproj --filter "FullyQualifiedName!~ApplicationSecuritySuite"` | `Passed! Failed: 0, Passed: 291, Skipped: 1, Total: 292, Duration: 29 s`. Skip: `LegacyRecodesTests.NormalizeJobFtPt_UndefinedJobcat1Format`. |
| Integration tests | `dotnet test tests/AccessibleSchoolReports.IntegrationTests/AccessibleSchoolReports.IntegrationTests.csproj` | `Failed! Failed: 1, Passed: 39, Skipped: 0, Total: 40, Duration: 29 s`. Fail: `LegacyModernParityTests.CharacterizedMetrics_MatchBetweenLegacyPdfAndModernCalculator` (320 of 560 characterized metrics; documented in `evidence/test-results/parity-results.md`). **Not a security failure.** |
| Characterization tests | `dotnet test tests/AccessibleSchoolReports.CharacterizationTests/AccessibleSchoolReports.CharacterizationTests.csproj` | `Passed! Failed: 0, Passed: 172, Skipped: 18, Total: 190, Duration: 332 ms`. |

## Supporting production changes made for this suite

- `KnowledgeAssistantService.AskAsync` does not call the LLM for unauthenticated users, whitespace questions, or questions longer than `KnowledgeRetrievalOptions.MaxQuestionLength` (4000).
- `KnowledgeRetrievalService.RetrieveAsync` already returned empty (no embed) for those same inputs.
- Knowledge Assistant textarea `maxlength` is 4000; the page shows a length error before Ask.
- `.gitignore` now ignores `.env`, `*.env`, `**/.env.*`, `secrets.json`, `**/secrets.json`, `appsettings.Local.json`, and `**/appsettings.*.local.json`.
