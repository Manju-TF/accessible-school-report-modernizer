const jsonHeaders = { Accept: "application/json", "Content-Type": "application/json" };

let cachedToken: string | null = null;
let cachedHeader = "RequestVerificationToken";

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message);
  }
}

export async function getAntiforgery(): Promise<{ token: string; headerName: string }> {
  const response = await fetch("/account/antiforgery", { credentials: "include" });
  if (!response.ok) {
    throw new ApiError("Could not start a secure request.", response.status);
  }

  const body = (await response.json()) as { requestToken?: string; headerName?: string };
  cachedToken = body.requestToken ?? "";
  cachedHeader = body.headerName ?? "RequestVerificationToken";
  return { token: cachedToken, headerName: cachedHeader };
}

async function tokenHeaders(): Promise<HeadersInit> {
  if (!cachedToken) {
    await getAntiforgery();
  }

  return {
    ...jsonHeaders,
    [cachedHeader]: cachedToken ?? "",
  };
}

async function readError(response: Response): Promise<string> {
  try {
    const body = (await response.json()) as { error?: string };
    if (body.error) {
      return body.error;
    }
  } catch {
    /* ignore */
  }

  return response.status === 401
    ? "Sign in to continue."
    : response.status === 403
      ? "You do not have permission for that action."
      : "The request failed.";
}

export async function apiGet<T>(path: string, signal?: AbortSignal): Promise<T> {
  const response = await fetch(path, { credentials: "include", headers: { Accept: "application/json" }, signal });
  if (response.status === 401) {
    throw new ApiError("Sign in to continue.", 401);
  }

  if (!response.ok) {
    throw new ApiError(await readError(response), response.status);
  }

  return (await response.json()) as T;
}

export async function apiSend<T>(path: string, init: RequestInit): Promise<T> {
  const headers = new Headers(await tokenHeaders());
  if (init.headers) {
    const extra = new Headers(init.headers);
    extra.forEach((value, key) => headers.set(key, value));
  }

  const response = await fetch(path, { ...init, credentials: "include", headers });
  if (response.status === 400 && cachedToken) {
    cachedToken = null;
  }

  if (!response.ok) {
    throw new ApiError(await readError(response), response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function apiPost<T>(path: string, body?: unknown, signal?: AbortSignal): Promise<T> {
  return apiSend<T>(path, {
    method: "POST",
    body: body === undefined ? undefined : JSON.stringify(body),
    signal,
  });
}

export async function apiPostStream<T, TProgress>(
  path: string,
  body: unknown,
  onProgress: (progress: TProgress) => void,
  signal?: AbortSignal,
): Promise<T> {
  const headers = new Headers(await tokenHeaders());
  headers.set("Accept", "application/x-ndjson");
  const response = await fetch(path, {
    method: "POST",
    credentials: "include",
    headers,
    body: JSON.stringify(body),
    signal,
  });
  if (response.status === 400 && cachedToken) {
    cachedToken = null;
  }

  if (!response.ok) {
    throw new ApiError(await readError(response), response.status);
  }
  if (!response.body) {
    throw new Error("The server did not provide a generation progress stream.");
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = "";
  let result: T | undefined;

  while (true) {
    const { done, value } = await reader.read();
    buffer += decoder.decode(value, { stream: !done });
    const lines = buffer.split("\n");
    buffer = lines.pop() ?? "";
    for (const line of lines) {
      if (!line.trim()) {
        continue;
      }

      const event = JSON.parse(line) as
        | { type: "progress"; progress: TProgress }
        | { type: "result"; result: T };
      if (event.type === "progress") {
        onProgress(event.progress);
      } else if (event.type === "result" && event.result !== undefined) {
        result = event.result;
      }
    }

    if (done) {
      break;
    }
  }

  if (buffer.trim()) {
    const event = JSON.parse(buffer) as { type?: string; result?: T };
    if (event.type === "result" && event.result !== undefined) {
      result = event.result;
    }
  }

  if (result === undefined) {
    throw new Error("The generation progress stream ended before the report results were received.");
  }

  return result;
}

export async function apiPostForm<T>(path: string, form: FormData): Promise<T> {
  if (!cachedToken) {
    await getAntiforgery();
  }

  form.set("__RequestVerificationToken", cachedToken ?? "");
  const response = await fetch(path, {
    method: "POST",
    credentials: "include",
    headers: { [cachedHeader]: cachedToken ?? "", Accept: "application/json" },
    body: form,
  });
  if (!response.ok) {
    throw new ApiError(await readError(response), response.status);
  }

  return (await response.json()) as T;
}
