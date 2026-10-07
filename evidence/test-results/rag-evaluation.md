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
| Scoring | Top-K = 8 (21 for unscoped printed-report / comparison questions), minimum score = 0.2; cosine similarity plus exact-term overlap |
| Pass rule | An expected source or RuleId appears **somewhere in top-K**, not only as rank 1. Security cases also require School B text absent from hits and from the formatted LLM user message. |

School B chunks were embedded **before** the School A user asked questions, so a leak would have been possible if authorization failed.

## Command

```text
dotnet test tests/AccessibleSchoolReports.UnitTests/AccessibleSchoolReports.UnitTests.csproj --filter "FullyQualifiedName~RagEvaluation"
```

## Run

| Field | Value |
|---|---|
| Date | 2026-10-07 |
| Host | Windows 10 (win32 10.0.26100) |
| Project | `AccessibleSchoolReports.UnitTests` (`net8.0`) |
| Filter | `FullyQualifiedName~RagEvaluation` |
| Cases | 12 |
| Passed | 12 |
| Failed | 0 |
| Evaluation duration | 5.762 s |
| Documents indexed | 92 |
| Chunks | 2677 |
| Chunks with embeddings | 2677 |
| Ingestion indexed | 90 |
| Ingestion missing | 0 |
| Embedding index chunks | 2677 |
| Embedding index failures | 0 |

## Results

| # | Category | Question | Expected source | Expected RuleId | Expected authorization scope | Retrieved source | Result | Pass/Fail |
|---|---|---|---|---|---|---|---|---|
| 1 | Legacy SAS logic | What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5? | legacy/sas/createschrptfiles2025.sas, docs/capstone/createschrptfiles-analysis.md, or docs/capstone/business-rules.md | CF-S-00 | Authenticated | rejected-ai-proposals.md (lines 26-33, RuleId CF-S-00, Authenticated, sim 0.500); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.468); business-rules.md (lines 58-58, RuleId CF-C-19, Authenticated, sim 0.457); createschrptfiles-analysis.md (lines 496-496, RuleId (none), Authenticated, sim 0.449); README.md (lines 298-298, RuleId CF-S-00, Authenticated, sim 0.436); implementation-plan.md (lines 202-226, RuleId CF-S-01, Authenticated, sim 0.435); SasUnivariate.cs (lines 1-50, RuleId CF-S-00, Authenticated, sim 0.423); business-rules.md (lines 52-52, RuleId CF-C-13, Authenticated, sim 0.419) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 2 | Business rules | What does rule CF-C-08 say about mapping empgen ACAD GOVT CLERK PUBINT to PUBLIC and BUS FIRM to PRIVATE? | docs/capstone/business-rules.md | CF-C-08 | Authenticated | business-rules.md (lines 47-47, RuleId CF-C-08, Authenticated, sim 0.516); schreptsummary_2025.sas (lines 85-134, RuleId (none), Authenticated, sim 0.479); schreptsummary-analysis.md (lines 134-157, RuleId (none), Authenticated, sim 0.477); SchoolReportPresentation.cs (lines 143-190, RuleId (none), Authenticated, sim 0.474); business-rules.md (lines 68-68, RuleId CF-S-07, Authenticated, sim 0.441); createschrptfiles-analysis.md (lines 293-293, RuleId (none), Authenticated, sim 0.434); LegacyRecodes.cs (lines 97-146, RuleId CF-PREP-06, Authenticated, sim 0.428); createschrptfiles-analysis.md (lines 595-595, RuleId (none), Authenticated, sim 0.426) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 3 | Salary rules | When are salary statistics omitted because n ge 5 on salftperm? | docs/capstone/business-rules.md or docs/capstone/createschrptfiles-analysis.md | CF-S-00 | Authenticated | rejected-ai-proposals.md (lines 7-10, RuleId (none), Authenticated, sim 0.432); README.md (lines 24-30, RuleId CF-S-00, Authenticated, sim 0.403); EVALUATOR-PACKET.md (lines 51-51, RuleId CF-S-00, Authenticated, sim 0.399); createschrptfiles-analysis.md (lines 482-487, RuleId (none), Authenticated, sim 0.397); schreptsummary-analysis.md (lines 287-287, RuleId SS-RPT-03, Authenticated, sim 0.379); PrintedReportArithmetic.cs (lines 44-91, RuleId (none), Authenticated, sim 0.368); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.359); report-map.md (lines 713-713, RuleId CF-S-00, Authenticated, sim 0.341) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 4 | Employment rules | How is employment status counted when jobcat1 is UNKN for analvar D? | docs/capstone/business-rules.md | CF-C-05 | Authenticated | createschrptfiles2025.sas (lines 198-210, RuleId (none), Authenticated, sim 0.420); schreptsummary-analysis.md (lines 93-93, RuleId (none), Authenticated, sim 0.408); business-rules.md (lines 44-44, RuleId CF-C-05, Authenticated, sim 0.399); corrected-plan.md (lines 182-182, RuleId (none), Authenticated, sim 0.389); createschrptfiles2025.sas (lines 1526-1539, RuleId (none), Authenticated, sim 0.380); createschrptfiles-analysis.md (lines 7-17, RuleId (none), Authenticated, sim 0.377); implementation-plan.md (lines 227-252, RuleId (none), Authenticated, sim 0.356); LegacyRecodes.cs (lines 1-50, RuleId CF-PREP-00, Authenticated, sim 0.353) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 5 | Accessibility requirements | What does the PDF accessibility strategy say about veraPDF, PAC, SemanticArticle, PDFUA_1, and the rule Do not add a green test named PDF is accessible? | docs/accessibility/pdf-accessibility-strategy.md | (none) | Authenticated | pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.608); README.md (lines 407-428, RuleId (none), Authenticated, sim 0.546); pdf-accessibility-strategy.md (lines 1-6, RuleId (none), Authenticated, sim 0.510); pdf-accessibility-strategy.md (lines 70-85, RuleId (none), Authenticated, sim 0.500); README.md (lines 119-133, RuleId SS-PAGE-05, Authenticated, sim 0.488); implementation-plan.md (lines 32-47, RuleId CF-S-00, Authenticated, sim 0.469); EVALUATOR-PACKET.md (lines 79-87, RuleId (none), Authenticated, sim 0.468); business-rules.md (lines 183-191, RuleId CF-S-00, Authenticated, sim 0.465) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 6 | Modern implementation traceability | Where does the modern SchoolReportCalculator apply characterized SAS salary suppression CF-S-00? | README.md | CF-S-00 | Authenticated | business-rules.md (lines 150-150, RuleId SS-SUP-01, Authenticated, sim 0.427); pdf-accessibility-strategy.md (lines 31-41, RuleId CF-S-00, Authenticated, sim 0.415); README.md (lines 961-995, RuleId CF-S-00, Authenticated, sim 0.401); README.md (lines 119-133, RuleId SS-PAGE-05, Authenticated, sim 0.401); corrected-plan.md (lines 17-28, RuleId (none), Authenticated, sim 0.377); README.md (lines 298-298, RuleId CF-S-00, Authenticated, sim 0.367); business-rules.md (lines 81-81, RuleId CF-S-20, Authenticated, sim 0.365); createschrptfiles-analysis.md (lines 488-489, RuleId CF-S-00, Authenticated, sim 0.364) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 7 | Generated PDF content | What is Total Reported on the School A Class of 2025 summary report for school 10701? | 10701-summary-report.pdf (generated report, page 1) | (none) | Report | 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.714); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.459); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.457); pdf-accessibility-strategy.md (lines 19-19, RuleId (none), Authenticated, sim 0.457); schreptsummary-analysis.md (lines 410-410, RuleId SS-HDR-02, Authenticated, sim 0.444); createschrptfiles2025.sas (lines 1356-1360, RuleId (none), Authenticated, sim 0.441); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.440); createschrptfiles-analysis.md (lines 944-944, RuleId CF-AMB-03, Authenticated, sim 0.427); report-map.md (lines 665-665, RuleId (none), Authenticated, sim 0.420); README.md (lines 134-144, RuleId (none), Authenticated, sim 0.412); KnowledgeGroundedPrompt.cs (lines 1-31, RuleId (none), Authenticated, sim 0.408); LegacySchoolNames.cs (lines 1-39, RuleId SS-PREP-03, Authenticated, sim 0.408); ui-accessibility-review.md (lines 12-29, RuleId (none), Authenticated, sim 0.401); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.398); DEMO-SCRIPT.md (lines 36-43, RuleId (none), Authenticated, sim 0.392); KnowledgeAccess.cs (lines 1-50, RuleId (none), Authenticated, sim 0.382); legacy-baseline.md (lines 16-22, RuleId (none), Authenticated, sim 0.365); business-rules.md (lines 155-155, RuleId SS-HDR-02, Authenticated, sim 0.356); generated-pdf-rag.md (lines 29-29, RuleId (none), Authenticated, sim 0.355); KnowledgeTextChunker.cs (lines 49-98, RuleId (none), Authenticated, sim 0.349); SPEAKER-NOTES.md (lines 13-16, RuleId (none), Authenticated, sim 0.336) | hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False | **PASS** |
| 8 | Report-specific questions | What employment figures appear on this report? | 10701-summary-report.pdf only (report-scoped) | (none) | Report | 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.296) | hits=1; candidates=1; LLM invoked=True; LLM context docs=1; School B in LLM context=False | **PASS** |
| 9 | Insufficient evidence | What is the cafeteria lunch menu for next Tuesday at the student union? | (none that answer the question) | (none) | (none that answer the question) | ui-accessibility-review.md (lines 59-66, RuleId (none), Authenticated, sim 0.487); ui-accessibility-review.md (lines 12-29, RuleId (none), Authenticated, sim 0.399); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.390); README.md (lines 863-866, RuleId (none), Authenticated, sim 0.379); EVALUATOR-PACKET.md (lines 79-87, RuleId (none), Authenticated, sim 0.366); SPEAKER-NOTES.md (lines 29-32, RuleId (none), Authenticated, sim 0.352); external-rag-api.md (lines 1-12, RuleId (none), Authenticated, sim 0.351); ui-accessibility-review.md (lines 30-37, RuleId (none), Authenticated, sim 0.348) | hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False | **PASS** |
| 10 | Unauthorized report access | What does the School B report say about employment outcomes for school 23306? | No School B generated report. Authenticated project docs may appear. School B secret must not reach the LLM. | (none) | Authenticated only if any hit; never Report/23306 | KnowledgeGroundedPrompt.cs (lines 1-31, RuleId (none), Authenticated, sim 0.450); ui-accessibility-review.md (lines 73-78, RuleId (none), Authenticated, sim 0.409); legacy-baseline.md (lines 16-22, RuleId (none), Authenticated, sim 0.408); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.403); corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.400); schreptsummary_2025.sas (lines 1747-1796, RuleId (none), Authenticated, sim 0.393); README.md (lines 683-701, RuleId (none), Authenticated, sim 0.385); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.375); report-map.md (lines 1-16, RuleId (none), Authenticated, sim 0.371); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.364); external-rag-api.md (lines 54-64, RuleId (none), Authenticated, sim 0.353); createschrptfiles-analysis.md (lines 944-944, RuleId CF-AMB-03, Authenticated, sim 0.352); generated-pdf-rag.md (lines 29-29, RuleId (none), Authenticated, sim 0.345); DEMO-SCRIPT.md (lines 51-57, RuleId (none), Authenticated, sim 0.341); rejected-ai-proposals.md (lines 26-33, RuleId CF-S-00, Authenticated, sim 0.336); 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.333); schreptsummary-analysis.md (lines 252-252, RuleId (none), Authenticated, sim 0.324); createschrptfiles2025.sas (lines 838-850, RuleId (none), Authenticated, sim 0.323); LegacySchoolNames.cs (lines 1-39, RuleId SS-PREP-03, Authenticated, sim 0.317); authorization-model.md (lines 37-46, RuleId (none), Authenticated, sim 0.311); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.311) | hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False | **PASS** |
| 11 | Unauthorized report access | What employment figures appear on this report? | (none) — unauthorized School B reportId is ignored as empty | (none) | (none) | (none) | hits=0; candidates=0; LLM invoked=True; LLM context docs=0; School B in LLM context=False; LLM received empty authorized context | **PASS** |
| 12 | School-wise comparison | Compare Total Reported across generated reports I can view. | 10701-summary-report.pdf (authorized generated reports only) | (none) | Report for School A; never School B | pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.416); report-map.md (lines 115-115, RuleId (none), Authenticated, sim 0.394); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.344); schreptsummary-analysis.md (lines 453-453, RuleId SS-NOTE-06, Authenticated, sim 0.340); README.md (lines 301-301, RuleId CF-C-01, Authenticated, sim 0.335); createschrptfiles-analysis.md (lines 146-146, RuleId (none), Authenticated, sim 0.333); SPEAKER-NOTES.md (lines 41-44, RuleId (none), Authenticated, sim 0.322); external-rag-api.md (lines 93-93, RuleId (none), Authenticated, sim 0.321); EVALUATOR-PACKET.md (lines 54-54, RuleId CF-C-01, Authenticated, sim 0.318); rejected-ai-proposals.md (lines 34-45, RuleId (none), Authenticated, sim 0.299); schreptsummary_2025.sas (lines 499-547, RuleId (none), Authenticated, sim 0.298); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.295); business-rules.md (lines 107-107, RuleId SS-FMT-03, Authenticated, sim 0.289); DEMO-SCRIPT.md (lines 58-64, RuleId (none), Authenticated, sim 0.287); 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.282); KnowledgeAccess.cs (lines 1-50, RuleId (none), Authenticated, sim 0.268); authorization-model.md (lines 88-93, RuleId (none), Authenticated, sim 0.267); PrintedMetricParser.cs (lines 1-50, RuleId (none), Authenticated, sim 0.250); PrintedReportArithmetic.cs (lines 1-45, RuleId (none), Authenticated, sim 0.247); KnowledgeTextChunker.cs (lines 49-98, RuleId (none), Authenticated, sim 0.247); ExcelHeaderNormalizer.cs (lines 1-26, RuleId (none), Authenticated, sim 0.246) | hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False | **PASS** |

## Security proof: School B is not passed to the LLM

Caller: ReportUser `user-a`, grant on School A (`10701`) only.

School B marker that must never appear in retrieval hits or LLM context: `SCHOOL-B-SECRET-TEXT`.

| Case | Hits contain School B | LLM context documents | School B secret in formatted LLM user message | Pass |
|---|---|---|---|---|
| 10 | no | 21 (no School B) | no | **PASS** |
| 11 | no | 0 (no School B) | no | **PASS** |

Case 10 asks about School B without a report id. Authenticated catalog chunks may be retrieved. Generated School B text must not be.

Case 11 sends School B `reportId`. `CanViewReportAsync` fails, retrieval returns empty **before** query embedding, and the grounded prompt contains `(no authorized context documents)`.

## Case notes

### 1. Legacy SAS logic

- Question: What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5?
- Expected source: legacy/sas/createschrptfiles2025.sas, docs/capstone/createschrptfiles-analysis.md, or docs/capstone/business-rules.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: rejected-ai-proposals.md (lines 26-33, RuleId CF-S-00, Authenticated, sim 0.500); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.468); business-rules.md (lines 58-58, RuleId CF-C-19, Authenticated, sim 0.457); createschrptfiles-analysis.md (lines 496-496, RuleId (none), Authenticated, sim 0.449); README.md (lines 298-298, RuleId CF-S-00, Authenticated, sim 0.436); implementation-plan.md (lines 202-226, RuleId CF-S-01, Authenticated, sim 0.435); SasUnivariate.cs (lines 1-50, RuleId CF-S-00, Authenticated, sim 0.423); business-rules.md (lines 52-52, RuleId CF-C-13, Authenticated, sim 0.419)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 2. Business rules

- Question: What does rule CF-C-08 say about mapping empgen ACAD GOVT CLERK PUBINT to PUBLIC and BUS FIRM to PRIVATE?
- Expected source: docs/capstone/business-rules.md
- Expected RuleId: CF-C-08
- Expected scope: Authenticated
- Retrieved: business-rules.md (lines 47-47, RuleId CF-C-08, Authenticated, sim 0.516); schreptsummary_2025.sas (lines 85-134, RuleId (none), Authenticated, sim 0.479); schreptsummary-analysis.md (lines 134-157, RuleId (none), Authenticated, sim 0.477); SchoolReportPresentation.cs (lines 143-190, RuleId (none), Authenticated, sim 0.474); business-rules.md (lines 68-68, RuleId CF-S-07, Authenticated, sim 0.441); createschrptfiles-analysis.md (lines 293-293, RuleId (none), Authenticated, sim 0.434); LegacyRecodes.cs (lines 97-146, RuleId CF-PREP-06, Authenticated, sim 0.428); createschrptfiles-analysis.md (lines 595-595, RuleId (none), Authenticated, sim 0.426)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 3. Salary rules

- Question: When are salary statistics omitted because n ge 5 on salftperm?
- Expected source: docs/capstone/business-rules.md or docs/capstone/createschrptfiles-analysis.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: rejected-ai-proposals.md (lines 7-10, RuleId (none), Authenticated, sim 0.432); README.md (lines 24-30, RuleId CF-S-00, Authenticated, sim 0.403); EVALUATOR-PACKET.md (lines 51-51, RuleId CF-S-00, Authenticated, sim 0.399); createschrptfiles-analysis.md (lines 482-487, RuleId (none), Authenticated, sim 0.397); schreptsummary-analysis.md (lines 287-287, RuleId SS-RPT-03, Authenticated, sim 0.379); PrintedReportArithmetic.cs (lines 44-91, RuleId (none), Authenticated, sim 0.368); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.359); report-map.md (lines 713-713, RuleId CF-S-00, Authenticated, sim 0.341)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 4. Employment rules

- Question: How is employment status counted when jobcat1 is UNKN for analvar D?
- Expected source: docs/capstone/business-rules.md
- Expected RuleId: CF-C-05
- Expected scope: Authenticated
- Retrieved: createschrptfiles2025.sas (lines 198-210, RuleId (none), Authenticated, sim 0.420); schreptsummary-analysis.md (lines 93-93, RuleId (none), Authenticated, sim 0.408); business-rules.md (lines 44-44, RuleId CF-C-05, Authenticated, sim 0.399); corrected-plan.md (lines 182-182, RuleId (none), Authenticated, sim 0.389); createschrptfiles2025.sas (lines 1526-1539, RuleId (none), Authenticated, sim 0.380); createschrptfiles-analysis.md (lines 7-17, RuleId (none), Authenticated, sim 0.377); implementation-plan.md (lines 227-252, RuleId (none), Authenticated, sim 0.356); LegacyRecodes.cs (lines 1-50, RuleId CF-PREP-00, Authenticated, sim 0.353)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 5. Accessibility requirements

- Question: What does the PDF accessibility strategy say about veraPDF, PAC, SemanticArticle, PDFUA_1, and the rule Do not add a green test named PDF is accessible?
- Expected source: docs/accessibility/pdf-accessibility-strategy.md
- Expected RuleId: (none)
- Expected scope: Authenticated
- Retrieved: pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.608); README.md (lines 407-428, RuleId (none), Authenticated, sim 0.546); pdf-accessibility-strategy.md (lines 1-6, RuleId (none), Authenticated, sim 0.510); pdf-accessibility-strategy.md (lines 70-85, RuleId (none), Authenticated, sim 0.500); README.md (lines 119-133, RuleId SS-PAGE-05, Authenticated, sim 0.488); implementation-plan.md (lines 32-47, RuleId CF-S-00, Authenticated, sim 0.469); EVALUATOR-PACKET.md (lines 79-87, RuleId (none), Authenticated, sim 0.468); business-rules.md (lines 183-191, RuleId CF-S-00, Authenticated, sim 0.465)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 6. Modern implementation traceability

- Question: Where does the modern SchoolReportCalculator apply characterized SAS salary suppression CF-S-00?
- Expected source: README.md
- Expected RuleId: CF-S-00
- Expected scope: Authenticated
- Retrieved: business-rules.md (lines 150-150, RuleId SS-SUP-01, Authenticated, sim 0.427); pdf-accessibility-strategy.md (lines 31-41, RuleId CF-S-00, Authenticated, sim 0.415); README.md (lines 961-995, RuleId CF-S-00, Authenticated, sim 0.401); README.md (lines 119-133, RuleId SS-PAGE-05, Authenticated, sim 0.401); corrected-plan.md (lines 17-28, RuleId (none), Authenticated, sim 0.377); README.md (lines 298-298, RuleId CF-S-00, Authenticated, sim 0.367); business-rules.md (lines 81-81, RuleId CF-S-20, Authenticated, sim 0.365); createschrptfiles-analysis.md (lines 488-489, RuleId CF-S-00, Authenticated, sim 0.364)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 7. Generated PDF content

- Question: What is Total Reported on the School A Class of 2025 summary report for school 10701?
- Expected source: 10701-summary-report.pdf (generated report, page 1)
- Expected RuleId: (none)
- Expected scope: Report
- Retrieved: 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.714); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.459); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.457); pdf-accessibility-strategy.md (lines 19-19, RuleId (none), Authenticated, sim 0.457); schreptsummary-analysis.md (lines 410-410, RuleId SS-HDR-02, Authenticated, sim 0.444); createschrptfiles2025.sas (lines 1356-1360, RuleId (none), Authenticated, sim 0.441); schreptsummary_2025.sas (lines 56-84, RuleId (none), Authenticated, sim 0.440); createschrptfiles-analysis.md (lines 944-944, RuleId CF-AMB-03, Authenticated, sim 0.427); report-map.md (lines 665-665, RuleId (none), Authenticated, sim 0.420); README.md (lines 134-144, RuleId (none), Authenticated, sim 0.412); KnowledgeGroundedPrompt.cs (lines 1-31, RuleId (none), Authenticated, sim 0.408); LegacySchoolNames.cs (lines 1-39, RuleId SS-PREP-03, Authenticated, sim 0.408); ui-accessibility-review.md (lines 12-29, RuleId (none), Authenticated, sim 0.401); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.398); DEMO-SCRIPT.md (lines 36-43, RuleId (none), Authenticated, sim 0.392); KnowledgeAccess.cs (lines 1-50, RuleId (none), Authenticated, sim 0.382); legacy-baseline.md (lines 16-22, RuleId (none), Authenticated, sim 0.365); business-rules.md (lines 155-155, RuleId SS-HDR-02, Authenticated, sim 0.356); generated-pdf-rag.md (lines 29-29, RuleId (none), Authenticated, sim 0.355); KnowledgeTextChunker.cs (lines 49-98, RuleId (none), Authenticated, sim 0.349); SPEAKER-NOTES.md (lines 13-16, RuleId (none), Authenticated, sim 0.336)
- Result: hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 8. Report-specific questions

- Question: What employment figures appear on this report?
- Expected source: 10701-summary-report.pdf only (report-scoped)
- Expected RuleId: (none)
- Expected scope: Report
- Retrieved: 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.296)
- Result: hits=1; candidates=1; LLM invoked=True; LLM context docs=1; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 9. Insufficient evidence

- Question: What is the cafeteria lunch menu for next Tuesday at the student union?
- Expected source: (none that answer the question)
- Expected RuleId: (none)
- Expected scope: (none that answer the question)
- Retrieved: ui-accessibility-review.md (lines 59-66, RuleId (none), Authenticated, sim 0.487); ui-accessibility-review.md (lines 12-29, RuleId (none), Authenticated, sim 0.399); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.390); README.md (lines 863-866, RuleId (none), Authenticated, sim 0.379); EVALUATOR-PACKET.md (lines 79-87, RuleId (none), Authenticated, sim 0.366); SPEAKER-NOTES.md (lines 29-32, RuleId (none), Authenticated, sim 0.352); external-rag-api.md (lines 1-12, RuleId (none), Authenticated, sim 0.351); ui-accessibility-review.md (lines 30-37, RuleId (none), Authenticated, sim 0.348)
- Result: hits=8; candidates=2676; LLM invoked=True; LLM context docs=8; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

### 10. Unauthorized report access

- Question: What does the School B report say about employment outcomes for school 23306?
- Expected source: No School B generated report. Authenticated project docs may appear. School B secret must not reach the LLM.
- Expected RuleId: (none)
- Expected scope: Authenticated only if any hit; never Report/23306
- Retrieved: KnowledgeGroundedPrompt.cs (lines 1-31, RuleId (none), Authenticated, sim 0.450); ui-accessibility-review.md (lines 73-78, RuleId (none), Authenticated, sim 0.409); legacy-baseline.md (lines 16-22, RuleId (none), Authenticated, sim 0.408); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.403); corrected-plan.md (lines 153-168, RuleId (none), Authenticated, sim 0.400); schreptsummary_2025.sas (lines 1747-1796, RuleId (none), Authenticated, sim 0.393); README.md (lines 683-701, RuleId (none), Authenticated, sim 0.385); EVALUATOR-PACKET.md (lines 88-96, RuleId (none), Authenticated, sim 0.375); report-map.md (lines 1-16, RuleId (none), Authenticated, sim 0.371); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.364); external-rag-api.md (lines 54-64, RuleId (none), Authenticated, sim 0.353); createschrptfiles-analysis.md (lines 944-944, RuleId CF-AMB-03, Authenticated, sim 0.352); generated-pdf-rag.md (lines 29-29, RuleId (none), Authenticated, sim 0.345); DEMO-SCRIPT.md (lines 51-57, RuleId (none), Authenticated, sim 0.341); rejected-ai-proposals.md (lines 26-33, RuleId CF-S-00, Authenticated, sim 0.336); 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.333); schreptsummary-analysis.md (lines 252-252, RuleId (none), Authenticated, sim 0.324); createschrptfiles2025.sas (lines 838-850, RuleId (none), Authenticated, sim 0.323); LegacySchoolNames.cs (lines 1-39, RuleId SS-PREP-03, Authenticated, sim 0.317); authorization-model.md (lines 37-46, RuleId (none), Authenticated, sim 0.311); business-rules.md (lines 1-25, RuleId (none), Authenticated, sim 0.311)
- Result: hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False
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

### 12. School-wise comparison

- Question: Compare Total Reported across generated reports I can view.
- Expected source: 10701-summary-report.pdf (authorized generated reports only)
- Expected RuleId: (none)
- Expected scope: Report for School A; never School B
- Retrieved: pdf-accessibility-strategy.md (lines 42-69, RuleId (none), Authenticated, sim 0.416); report-map.md (lines 115-115, RuleId (none), Authenticated, sim 0.394); implementation-plan.md (lines 293-317, RuleId SS-FTR-01, Authenticated, sim 0.344); schreptsummary-analysis.md (lines 453-453, RuleId SS-NOTE-06, Authenticated, sim 0.340); README.md (lines 301-301, RuleId CF-C-01, Authenticated, sim 0.335); createschrptfiles-analysis.md (lines 146-146, RuleId (none), Authenticated, sim 0.333); SPEAKER-NOTES.md (lines 41-44, RuleId (none), Authenticated, sim 0.322); external-rag-api.md (lines 93-93, RuleId (none), Authenticated, sim 0.321); EVALUATOR-PACKET.md (lines 54-54, RuleId CF-C-01, Authenticated, sim 0.318); rejected-ai-proposals.md (lines 34-45, RuleId (none), Authenticated, sim 0.299); schreptsummary_2025.sas (lines 499-547, RuleId (none), Authenticated, sim 0.298); SchoolReportPresentation.cs (lines 29-48, RuleId (none), Authenticated, sim 0.295); business-rules.md (lines 107-107, RuleId SS-FMT-03, Authenticated, sim 0.289); DEMO-SCRIPT.md (lines 58-64, RuleId (none), Authenticated, sim 0.287); 10701-summary-report.pdf (page 1, RuleId (none), Report, sim 0.282); KnowledgeAccess.cs (lines 1-50, RuleId (none), Authenticated, sim 0.268); authorization-model.md (lines 88-93, RuleId (none), Authenticated, sim 0.267); PrintedMetricParser.cs (lines 1-50, RuleId (none), Authenticated, sim 0.250); PrintedReportArithmetic.cs (lines 1-45, RuleId (none), Authenticated, sim 0.247); KnowledgeTextChunker.cs (lines 49-98, RuleId (none), Authenticated, sim 0.247); ExcelHeaderNormalizer.cs (lines 1-26, RuleId (none), Authenticated, sim 0.246)
- Result: hits=21; candidates=2676; LLM invoked=True; LLM context docs=21; School B in LLM context=False
- Pass/Fail: PASS
- LLM invoked: True; completion stub: `grounded-answer`

## Limitations

- Lexical embeddings are not `text-embedding-3-small`. Rankings can differ from a live embedding provider.
- The language-model **answer text** is a test stub. This evaluation scores retrieval, authorization, and the grounded prompt payload.
- Case 9 (insufficient evidence) uses the production 0.2 similarity floor. Hashed bag-of-words can still return weakly related catalog chunks. The case passes only when none of those chunks contain cafeteria / lunch-menu evidence.
- Generated-report chunks are evaluation fixtures with the same `GeneratedReport` / `Report` shape as production PDF ingestion. They are not a live OpenAI-indexed working-database snapshot.
- Do not treat this file as PDF/UA validation.

