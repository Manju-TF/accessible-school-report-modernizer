# External RAG embedding API

This document describes the **embedding** integration only. Application sign-in stays **ASP.NET Core Identity** with the existing cookie ticket (`.asr.auth`). This slice does not add JWT login, OAuth, or Entra ID, and it does not change cookie authentication.

The external HTTP API is used only to create vectors for RAG. It is not an identity provider.

The Knowledge Assistant page is at `/knowledge-assistant` and requires authentication plus `RequireRagAccess`. An authorized report details page (`/reports/{id}`) can start **Ask about this report**. That action stores the report id in a server-side session only after `CanViewReportAsync`. Retrieval then uses chunks for that report only. A query-string `report` value is re-checked the same way; changing it cannot load another school’s report. Failed lookups return “That report is not available.” with no school or report metadata.

`IKnowledgeAssistantService` answers from authorized chunks through `ILanguageModelService`. Application sign-in stays Identity cookies (`.asr.auth`). The language-model API key is not an identity provider and is not a user JWT. The page does not display API keys, embeddings, unauthorized text, physical file paths, or internal database identifiers.

On startup (except the `Testing` environment) `KnowledgeStartup` ingests the catalog and writes local lexical embeddings. Retrieval does not call OpenAI for those vectors. The OpenAI key is used for chat completions. Restart the web app after changing secrets so ingest/index and the trimmed key are loaded.

## Connecting ingestion to embeddings

Ingestion (legacy files and generated PDFs) writes `KnowledgeDocument` / `KnowledgeChunk` text only. Embedding is a **separate** pass, `IKnowledgeEmbeddingIndexService.IndexPendingEmbeddingsAsync`, so report generation is not blocked on the provider.

Flow: document → chunk → embed permitted text → store `Embedding` and `EmbeddingModel`.

| Chunk state | Action |
|---|---|
| No vector, or `EmbeddingModel` ≠ current `{Provider}/{Model}` | Embed |
| Vector present for the current model | Skip (unchanged) |
| Caller cannot access the document | Skip; text is not sent |
| Provider error on one chunk | Record failure; continue |

`KnowledgeIndexResult` is the later UI status model: documents indexed, chunks indexed, chunks skipped, failures, and duration.

## Authorization-aware retrieval

`IKnowledgeRetrievalService` requires the authenticated caller.

Required order:

1. Authentication (`RequireRagAccess`: Admin, ReportUser, Viewer)
2. Authorization scope (`KnowledgeAccess` + school/report checks)
3. Candidate selection (authorized documents only; current embedding model)
4. Question embedding and cosine similarity
5. Minimum similarity threshold and top-K
6. LLM receives **only** those authorized hits

Unauthorized chunks are never loaded as candidates and never passed to `ILanguageModelService`. Global `Authenticated` documents are visible to RAG-authorized users. `Admin` documents are Admin-only. Generated PDF chunks additionally require `CanViewReportAsync`.

Each hit includes source metadata: `RuleId`, `SchoolId`, `ReportId`, `ReportYear`, `SourceLocation`, `SourceIdentifier`, and similarity.

## Language model

| Setting | Default |
|---|---|
| Interface | `ILanguageModelService` |
| Assistant | `IKnowledgeAssistantService` |
| Implementation | `OpenAiCompatibleLanguageModelService` |
| Endpoint | `https://api.openai.com/v1/chat/completions` |
| Model | `gpt-4o-mini` |

Flow: authenticated user → authorization-aware retrieval → authorized chunks only → grounded prompt → external LLM → answer + sources.

Retrieved SAS, Markdown, and PDF text is **data**, not instructions. The system prompt forbids invented rules or report values, unauthorized disclosure, following embedded document instructions, and deterministic report calculations. Context is wrapped in `BEGIN/END UNTRUSTED PROJECT DATA`. Fence markers inside chunk text are neutralized.

Set `LanguageModel:ApiKey` in user secrets. Committed `appsettings.json` keeps it empty. Unit tests use `FakeLanguageModelService` or a scripted `HttpMessageHandler`; they never call a live model.

```powershell
dotnet user-secrets set LanguageModel:ApiKey "your-key" --project src/AccessibleSchoolReports.Web
```

## Provider

| Setting | Default |
|---|---|
| Provider | `Lexical` (default). Set `OpenAICompatible` only if you want remote vectors. |
| Interface | `IEmbeddingService` (Application) |
| Implementation | `LexicalEmbeddingService` by default; `OpenAiCompatibleEmbeddingService` when `Embeddings:Provider` is `OpenAICompatible` |
| Endpoint | `https://api.openai.com/v1/embeddings` (overridable) |

Any OpenAI-compatible embeddings endpoint can be configured (`Embeddings:Endpoint`). Azure OpenAI-style URLs work when they accept the same JSON body (`model`, `input`, `dimensions`) and a bearer token.

There is no live provider call in unit tests. Tests use `FakeEmbeddingService` or a scripted `HttpMessageHandler`.

## Model

| Setting | Default |
|---|---|
| Model | `text-embedding-3-small` |
| Dimensions | `1536` (required; validated on every vector) |

The configured dimension is sent as `dimensions` when the provider supports it. A response vector of a different length is rejected (`EmbeddingDimensionException`). Stored chunks record `EmbeddingModel` as `{Provider}/{Model}`.

## Data sent

The provider receives **only text that the current user is allowed to process**.

| Call | Payload |
|---|---|
| `EmbedPermittedChunksAsync` | `KnowledgeChunk.Content` for chunks that pass `EmbeddingAccess` / `IReportAuthorizationService` |
| `EmbedQueryAsync` | The caller’s query string only (not report rows) |

Unauthorized generated-report chunks are **not** placed in the HTTP body. They are returned in `SkippedUnauthorizedChunkIds`.

Graduate worksheets and raw student tables are not an embedding source. Report PDFs are already reduced to authorized knowledge chunks before this service runs.

## Credentials

`Embeddings:ApiKey` is bound from configuration. Committed `appsettings.json` keeps the value empty.

Set the key in user secrets or the environment. Do not commit it. Do not put it in Blazor components, `wwwroot`, or browser-visible config.

```powershell
dotnet user-secrets set Embeddings:ApiKey "your-key" --project src/AccessibleSchoolReports.Web
```

The key is sent only as `Authorization: Bearer` from the server-side `HttpClient`. Application authentication remains Identity cookies.

`EmbeddingOptions.ToString()` omits the key. Embedding logs include provider, model, status, and counts — not the key and not chunk text.

`HttpClient` logging for this client is filtered to **Warning** so default request-header traces are not written.

## Error handling

| Condition | Behavior |
|---|---|
| Missing key / invalid endpoint / non-positive dimensions | `EmbeddingConfigurationException`; no HTTP call |
| Timeout | Linked cancellation after `TimeoutSeconds`; `EmbeddingTimeoutException` |
| Caller `CancellationToken` | Honored on send and retry delays |
| `429` / `408` / `5xx` | Retry up to `MaxRetries` with `Retry-After` or exponential backoff (capped at 30s) |
| Other HTTP errors | Fail without retry |
| Wrong vector length | `EmbeddingDimensionException` |

## Privacy considerations

- Sending a chunk to the provider discloses that text to the configured vendor.
- School-scoped report text is sent only when the signed-in user may access that school.
- Do not point `Embeddings:Endpoint` at a vendor you are not allowed to share report text with.
- API keys stay on the server. They are not part of the Identity cookie or the Blazor circuit.

## Limitations

- The assistant page does not perform report calculations.
- No live provider is required for CI; unit tests never call the public internet.
- Provider quotas, retention, and training-use policies are outside this repository.
- Identity/cookie authentication is unchanged and is not replaced by the embedding key.
