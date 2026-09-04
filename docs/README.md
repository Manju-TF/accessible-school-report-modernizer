# Documentation

Design and characterization. Observed test output lives in `evidence/`.

## Layout

```text
docs/
  README.md
  capstone/         SAS rules, report map, authz, RAG design
  architecture/     Corrected plan and rejected original
  accessibility/    PDF targeting strategy; UI review
  decisions/        Human-rejected AI proposals
```

## Capstone (`docs/capstone/`)

| File | Role |
|---|---|
| `legacy-baseline.md` | SHA-256 manifest for `/legacy` |
| `createschrptfiles-analysis.md` | Builder SAS characterization |
| `schreptsummary-analysis.md` | Report SAS characterization |
| `business-rules.md` | Combined Rule IDs (`CF-*`, `SS-*`) |
| `report-map.md` | Baseline PDF section map |
| `authentication.md` | Identity cookie sign-in |
| `authorization-model.md` | Roles and school grants |
| `security-architecture.md` | Policies and download/RAG boundaries |
| `external-rag-api.md` | Embedding and language-model integration |
| `generated-pdf-rag.md` | How a generated PDF becomes knowledge chunks |

## Architecture

| File | Role |
|---|---|
| `corrected-plan.md` | Approved architecture plan |
| `implementation-plan.md` | Original four-project plan. Historical only. |

## Accessibility

| File | Role |
|---|---|
| `pdf-accessibility-strategy.md` | Tagged PDF / PDF-UA *target*. Not a validation certificate. |
| `ui-accessibility-review.md` | UI findings. Not a WCAG pass. |

## Decisions

| File | Role |
|---|---|
| `rejected-ai-proposals.md` | Includes the rejected “emit salaries when n < 5” change (`CF-S-00`). |

## Evidence (do not put run logs here)

| Path | Role |
|---|---|
| `evidence/test-results/` | Quality, parity, security, RAG, UI review output |
| `evidence/screenshots/pdf-compare/` | Visual page compare |

Knowledge ingestion reads the catalog in `KnowledgeSourceCatalog` (legacy SAS + listed project markdown). It does not ingest `evidence/`.
