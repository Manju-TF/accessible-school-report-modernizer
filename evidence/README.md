# Evidence

Observed artifacts only. This folder is **not** empty. Do not invent missing reports.

`docs/` holds design and characterization. `evidence/` holds what was actually run or captured.

## Layout

```text
evidence/
  README.md
  test-results/          observed test and review output
  screenshots/pdf-compare/  page images from a visual PDF compare
```

## What is here

| Path | What it is |
|---|---|
| `test-results/final-quality-report.md` | Solution build/test snapshot (4 Sep 2026). One known fail: `LegacyModernParityTests`. |
| `test-results/parity-results.md` | Metric-by-metric baseline PDF vs sample-school calculator. **FAIL** (different populations). |
| `test-results/security-test-results.md` | 32-case application security suite runner output. |
| `test-results/rag-evaluation.md` | 11-case RAG retrieval evaluation (lexical embeddings; School B leak check). |
| `test-results/security-ui-review.md` | Playwright MCP UI security/accessibility review. Findings are not claimed fixed. |
| `screenshots/pdf-compare/` | Baseline / fixture / generated page images plus `fixture-layout.pdf`. Visual compare only — not PDF/UA validation. |

## What is not here

These were never produced. Do not add placeholder files that look like they were.

| Missing | Why |
|---|---|
| `test-results/performance-results.md` | No timed 189-school benchmark was recorded. Run History stores `DurationMilliseconds` per run. |
| `pdf-validation/` | No veraPDF, PAC, or screen-reader report. Do not claim the PDF is accessible. |
| Per-step evidence packs from the rejected 14-step plan | Corrected plan keeps evidence at the end, not after every step. |

Empty `baseline/`, `demo/`, and `mcp/` placeholders were removed. Legacy files stay in `/legacy`. Working copies belong in `src/`, `data/`, or here — never written back to `/legacy`.
