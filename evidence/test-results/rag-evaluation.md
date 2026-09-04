# RAG evaluation

This document records an **observed** retrieval evaluation. It does not claim that generated PDFs are accessible. Report calculations stay in deterministic C#. Live OpenAI chat was not used (the provider previously returned HTTP 429).

## Method

The runner is `tests/AccessibleSchoolReports.UnitTests/Knowledge/RagEvaluationTests.cs`. This file is written to `evidence/test-results/rag-evaluation.md`.

| Piece | What ran |
|---|---|
| Corpus | Real `KnowledgeSourceCatalog` files ingested by `KnowledgeIngestionService` from this repository |
| Generated reports | Two `GeneratedReport` documents (`10701` School A, `23306` School B) with distinctive page-1 text |
| Embeddings | Deterministic lexical hashed bag-of-words (`LexicalEmbeddingService`). No network. |
| Retrieval / authz | Production `KnowledgeRetrievalService` + `KnowledgeAccess` + `IReportAuthorizationService` |
| Assistant | Production `KnowledgeAssistantService` + `KnowledgeGroundedPrompt` |
| Language model | `FakeLanguageModelService` records the exact request. Completion text is a stub, not a live answer. |
| Scoring | Top-K = 5, minimum similarity = 0.2 |
| Pass rule | An expected source or RuleId appears **somewhere in top-K**, not only as rank 1. Security cases also require School B text absent from hits and from the formatted LLM user message. |

School B chunks were embedded **before** the School A user asked questions, so a leak would have been possible if authorization failed.

## Command

```text
dotnet test tests/AccessibleSchoolReports.UnitTests/AccessibleSchoolReports.UnitTests.csproj --filter "FullyQualifiedName~RagEvaluation"
```

## Run

| Field | Value |
|---|---|
| Date | 2026-09-04 |
| Host | Windows 10 (win32 10.0.26100) |
| Project | `AccessibleSchoolReports.UnitTests` (`net8.0`) |
| Filter | `FullyQualifiedName~RagEvaluation` |
| Cases | 11 |
| Passed | 11 |
| Failed | 0 |
| Evaluation duration | 0.938 s |
| Documents indexed | 11 |
| Chunks | 721 |
| Chunks with embeddings | 721 |
| Ingestion indexed | 9 |
| Ingestion missing | 0 |
| Embedding index chunks | 721 |
| Embedding index failures | 0 |

## Results

| # | Category | Question | Expected source | Expected RuleId | Expected authorization scope | Retrieved source | Result | Pass/Fail |
|---|---|---|---|---|---|---|---|---|
| 1 | Legacy SAS logic | What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5? | legacy/sas/createschrptfiles2025.sas, docs/capstone/createschrptfiles-analysis.md, or docs/capstone/business-rules.md | CF-S-00 | Authenticated | createschrptfiles-analysis.md (lines 488-501, RuleId CF-S-00, Authenticated, sim 0.398); schreptsummary-analysis.md (lines 245-253, RuleId SS-CALC-02, Authenticated, sim 0.368); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.361); report-map.md (lines 181-197, RuleId CF-C-05, Authenticated, sim 0.355); createschrptfiles-analysis.md (lines 1-6, RuleId (none), Authenticated, sim 0.354) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 2 | Business rules | What does rule CF-C-08 say about mapping empgen ACAD GOVT CLERK PUBINT to PUBLIC and BUS FIRM to PRIVATE? | docs/capstone/business-rules.md | CF-C-08 | Authenticated | business-rules.md (lines 47-47, RuleId CF-C-08, Authenticated, sim 0.419); business-rules.md (lines 68-68, RuleId CF-S-07, Authenticated, sim 0.401); schreptsummary_2025.sas (lines 85-134, RuleId (none), Authenticated, sim 0.379); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.371); schreptsummary-analysis.md (lines 127-157, RuleId SS-FMT-04, Authenticated, sim 0.367) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 3 | Salary rules | When are salary statistics omitted because n ge 5 on salftperm? | docs/capstone/business-rules.md or docs/capstone/createschrptfiles-analysis.md | CF-S-00 | Authenticated | schreptsummary-analysis.md (lines 287-287, RuleId SS-RPT-03, Authenticated, sim 0.316); business-rules.md (lines 100-100, RuleId CF-AMB-09, Authenticated, sim 0.309); createschrptfiles2025.sas (lines 468-468, RuleId (none), Authenticated, sim 0.292); business-rules.md (lines 61-61, RuleId CF-S-00, Authenticated, sim 0.280); README.md (lines 176-192, RuleId (none), Authenticated, sim 0.273) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 4 | Employment rules | How is employment status counted when jobcat1 is UNKN for analvar D? | docs/capstone/business-rules.md | CF-C-05 | Authenticated | createschrptfiles-analysis.md (lines 7-17, RuleId (none), Authenticated, sim 0.377); createschrptfiles2025.sas (lines 198-210, RuleId (none), Authenticated, sim 0.320); createschrptfiles2025.sas (lines 377-389, RuleId (none), Authenticated, sim 0.314); createschrptfiles2025.sas (lines 1526-1539, RuleId (none), Authenticated, sim 0.313); business-rules.md (lines 44-44, RuleId CF-C-05, Authenticated, sim 0.308) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 5 | Accessibility requirements | What does the PDF accessibility strategy say about veraPDF, PAC, SemanticArticle, PDFUA_1, and the rule Do not add a green test named PDF is accessible? | docs/accessibility/pdf-accessibility-strategy.md | (none) | Authenticated | pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.485); README.md (lines 222-235, RuleId (none), Authenticated, sim 0.479); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.449); pdf-accessibility-strategy.md (lines 1-6, RuleId (none), Authenticated, sim 0.443); report-map.md (lines 162-180, RuleId CF-C-04, Authenticated, sim 0.424) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 6 | Modern implementation traceability | Where does the modern SchoolReportCalculator apply characterized SAS salary suppression CF-S-00? | README.md | CF-S-00 | Authenticated | createschrptfiles2025.sas (lines 1370-1370, RuleId (none), Authenticated, sim 0.314); README.md (lines 193-207, RuleId CF-S-00, Authenticated, sim 0.307); corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.305); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.295); business-rules.md (lines 81-81, RuleId CF-S-20, Authenticated, sim 0.290) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 7 | Generated PDF content | What is Total Reported on the School A Class of 2025 summary report for school 10701? | 10701-summary-report.pdf (generated report, page 1) | (none) | Report | 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.536); schreptsummary-analysis.md (lines 410-410, RuleId SS-HDR-02, Authenticated, sim 0.388); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.374); schreptsummary-analysis.md (lines 411-411, RuleId SS-HDR-03, Authenticated, sim 0.364); schreptsummary-analysis.md (lines 118-126, RuleId SS-FMT-03, Authenticated, sim 0.353) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 8 | Report-specific questions | What employment figures appear on this report? | 10701-summary-report.pdf only (report-scoped) | (none) | Report | 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.216) | hits=1; candidates=1; LLM invoked=True; LLM context docs=1; School B in LLM context=False | **PASS** |
| 9 | Insufficient evidence | What is the cafeteria lunch menu for next Tuesday at the student union? | (none that answer the question) | (none) | (none that answer the question) | README.md (lines 46-64, RuleId (none), Authenticated, sim 0.334); corrected-plan.md (lines 17-28, RuleId (none), Authenticated, sim 0.319); README.md (lines 176-192, RuleId (none), Authenticated, sim 0.309); createschrptfiles-analysis.md (lines 452-465, RuleId CF-C-20, Authenticated, sim 0.307); pdf-accessibility-strategy.md (lines 70-85, RuleId (none), Authenticated, sim 0.306) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 10 | Unauthorized report access | What does the School B report say about employment outcomes for school 23306? | No School B generated report. Authenticated project docs may appear. School B secret must not reach the LLM. | (none) | Authenticated only if any hit; never Report/23306 | corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.375); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.355); schreptsummary_2025.sas (lines 1747-1796, RuleId (none), Authenticated, sim 0.343); schreptsummary_2025.sas (lines 847-896, RuleId (none), Authenticated, sim 0.333); corrected-plan.md (lines 126-130, RuleId (none), Authenticated, sim 0.332) | hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False | **PASS** |
| 11 | Unauthorized report access | What employment figures appear on this report? | (none) — unauthorized School B reportId is ignored as empty | (none) | (none) | (none) | hits=0; candidates=0; LLM invoked=True; LLM context docs=0; School B in LLM context=False; LLM received empty authorized context | **PASS** |

## Security proof: School B is not passed to the LLM

Caller: ReportUser `user-a`, grant on School A (`10701`) only.

School B marker that must never appear in retrieval hits or LLM context: `SCHOOL-B-SECRET-TEXT`.

| Case | Hits contain School B | LLM context documents | School B secret in formatted LLM user message | Pass |
|---|---|---|---|---|
| 10 | no | 5 (no School B) | no | **PASS** |
| 11 | no | 0 (no School B) | no | **PASS** |

Case 10 asks about School B without a report id. Authenticated catalog chunks may be retrieved. Generated School B text must not be.

Case 11 sends School B `reportId`. `CanViewReportAsync` fails, retrieval returns empty **before** query embedding, and the grounded prompt contains `(no authorized context documents)`.

## Case notes

### 1. Legacy SAS logic

- Question: What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5?
- Expected source: legacy/sas/createschrptfiles2025.sas, docs/capstone/createschrptfiles-analysis.md, or docs/capstone/business-rules.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: createschrptfiles-analysis.md (lines 488-501, RuleId CF-S-00, Authenticated, sim 0.398); schreptsummary-analysis.md (lines 245-253, RuleId SS-CALC-02, Authenticated, sim 0.368); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.361); report-map.md (lines 181-197, RuleId CF-C-05, Authenticated, sim 0.355); createschrptfiles-analysis.md (lines 1-6, RuleId (none), Authenticated, sim 0.354)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 2. Business rules

- Question: What does rule CF-C-08 say about mapping empgen ACAD GOVT CLERK PUBINT to PUBLIC and BUS FIRM to PRIVATE?
- Expected source: docs/capstone/business-rules.md
- Expected RuleId: CF-C-08
- Expected scope: Authenticated
- Retrieved: business-rules.md (lines 47-47, RuleId CF-C-08, Authenticated, sim 0.419); business-rules.md (lines 68-68, RuleId CF-S-07, Authenticated, sim 0.401); schreptsummary_2025.sas (lines 85-134, RuleId (none), Authenticated, sim 0.379); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.371); schreptsummary-analysis.md (lines 127-157, RuleId SS-FMT-04, Authenticated, sim 0.367)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 3. Salary rules

- Question: When are salary statistics omitted because n ge 5 on salftperm?
- Expected source: docs/capstone/business-rules.md or docs/capstone/createschrptfiles-analysis.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: schreptsummary-analysis.md (lines 287-287, RuleId SS-RPT-03, Authenticated, sim 0.316); business-rules.md (lines 100-100, RuleId CF-AMB-09, Authenticated, sim 0.309); createschrptfiles2025.sas (lines 468-468, RuleId (none), Authenticated, sim 0.292); business-rules.md (lines 61-61, RuleId CF-S-00, Authenticated, sim 0.280); README.md (lines 176-192, RuleId (none), Authenticated, sim 0.273)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 4. Employment rules

- Question: How is employment status counted when jobcat1 is UNKN for analvar D?
- Expected source: docs/capstone/business-rules.md
- Expected RuleId: CF-C-05
- Expected scope: Authenticated
- Retrieved: createschrptfiles-analysis.md (lines 7-17, RuleId (none), Authenticated, sim 0.377); createschrptfiles2025.sas (lines 198-210, RuleId (none), Authenticated, sim 0.320); createschrptfiles2025.sas (lines 377-389, RuleId (none), Authenticated, sim 0.314); createschrptfiles2025.sas (lines 1526-1539, RuleId (none), Authenticated, sim 0.313); business-rules.md (lines 44-44, RuleId CF-C-05, Authenticated, sim 0.308)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 5. Accessibility requirements

- Question: What does the PDF accessibility strategy say about veraPDF, PAC, SemanticArticle, PDFUA_1, and the rule Do not add a green test named PDF is accessible?
- Expected source: docs/accessibility/pdf-accessibility-strategy.md
- Expected RuleId: (none)
- Expected scope: Authenticated
- Retrieved: pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.485); README.md (lines 222-235, RuleId (none), Authenticated, sim 0.479); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.449); pdf-accessibility-strategy.md (lines 1-6, RuleId (none), Authenticated, sim 0.443); report-map.md (lines 162-180, RuleId CF-C-04, Authenticated, sim 0.424)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 6. Modern implementation traceability

- Question: Where does the modern SchoolReportCalculator apply characterized SAS salary suppression CF-S-00?
- Expected source: README.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: createschrptfiles2025.sas (lines 1370-1370, RuleId (none), Authenticated, sim 0.314); README.md (lines 193-207, RuleId CF-S-00, Authenticated, sim 0.307); corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.305); README.md (lines 11-25, RuleId SS-PAGE-05, Authenticated, sim 0.295); business-rules.md (lines 81-81, RuleId CF-S-20, Authenticated, sim 0.290)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 7. Generated PDF content

- Question: What is Total Reported on the School A Class of 2025 summary report for school 10701?
- Expected source: 10701-summary-report.pdf (generated report, page 1)
- Expected RuleId: (none)
- Expected scope: Report
- Retrieved: 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.536); schreptsummary-analysis.md (lines 410-410, RuleId SS-HDR-02, Authenticated, sim 0.388); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.374); schreptsummary-analysis.md (lines 411-411, RuleId SS-HDR-03, Authenticated, sim 0.364); schreptsummary-analysis.md (lines 118-126, RuleId SS-FMT-03, Authenticated, sim 0.353)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 8. Report-specific questions

- Question: What employment figures appear on this report?
- Expected source: 10701-summary-report.pdf only (report-scoped)
- Expected RuleId: (none)
- Expected scope: Report
- Retrieved: 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.216)
- Result: hits=1; candidates=1; LLM invoked=True; LLM context docs=1; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 9. Insufficient evidence

- Question: What is the cafeteria lunch menu for next Tuesday at the student union?
- Expected source: (none that answer the question)
- Expected RuleId: (none)
- Expected scope: (none that answer the question)
- Retrieved: README.md (lines 46-64, RuleId (none), Authenticated, sim 0.334); corrected-plan.md (lines 17-28, RuleId (none), Authenticated, sim 0.319); README.md (lines 176-192, RuleId (none), Authenticated, sim 0.309); createschrptfiles-analysis.md (lines 452-465, RuleId CF-C-20, Authenticated, sim 0.307); pdf-accessibility-strategy.md (lines 70-85, RuleId (none), Authenticated, sim 0.306)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 10. Unauthorized report access

- Question: What does the School B report say about employment outcomes for school 23306?
- Expected source: No School B generated report. Authenticated project docs may appear. School B secret must not reach the LLM.
- Expected RuleId: (none)
- Expected scope: Authenticated only if any hit; never Report/23306
- Retrieved: corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.375); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.355); schreptsummary_2025.sas (lines 1747-1796, RuleId (none), Authenticated, sim 0.343); schreptsummary_2025.sas (lines 847-896, RuleId (none), Authenticated, sim 0.333); corrected-plan.md (lines 126-130, RuleId (none), Authenticated, sim 0.332)
- Result: hits=5; candidates=720; LLM invoked=True; LLM context docs=5; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 11. Unauthorized report access

- Question: What employment figures appear on this report?
- Expected source: (none) — unauthorized School B reportId is ignored as empty
- Expected RuleId: (none)
- Expected scope: (none)
- Retrieved: (none)
- Result: hits=0; candidates=0; LLM invoked=True; LLM context docs=0; School B in LLM context=False; LLM received empty authorized context
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

## Limitations

- Lexical embeddings are not `text-embedding-3-small`. Rankings can differ from a live embedding provider.
- The language-model **answer text** is a test stub. This evaluation scores retrieval, authorization, and the grounded prompt payload.
- Case 9 (insufficient evidence) uses the production 0.2 similarity floor. Hashed bag-of-words can still return weakly related catalog chunks. The case passes only when none of those chunks contain cafeteria / lunch-menu evidence.
- Generated-report chunks are evaluation fixtures with the same `GeneratedReport` / `Report` shape as production PDF ingestion. They are not a live OpenAI-indexed working-database snapshot.
- Do not treat this file as PDF/UA validation.

