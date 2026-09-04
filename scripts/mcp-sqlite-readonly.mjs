#!/usr/bin/env node
/**
 * Read-only SQLite MCP for this repo. Opens the database with SQLite
 * read-only mode and accepts only SELECT / WITH / EXPLAIN / safe PRAGMA.
 */
import { createInterface } from "node:readline";
import { existsSync } from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { DatabaseSync } from "node:sqlite";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const candidates = [
  process.argv[2],
  path.join(repoRoot, "data", "schoolreports.db"),
  path.join(
    repoRoot,
    "src",
    "AccessibleSchoolReports.Web",
    "data",
    "schoolreports.db"
  ),
].filter(Boolean);

const dbPath = candidates.find((candidate) => existsSync(candidate));
if (!dbPath) {
  process.stderr.write(
    `mcp-sqlite-readonly: database not found. Tried:\n${candidates.join("\n")}\n`
  );
  process.exit(1);
}

const db = new DatabaseSync(dbPath, { readOnly: true });

const tools = [
  {
    name: "list_tables",
    description: "List user tables in the connected SQLite database.",
    inputSchema: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    name: "schema",
    description:
      "Return columns, foreign keys, indexes, and row counts for all tables.",
    inputSchema: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    name: "query",
    description:
      "Run a read-only SQL statement (SELECT, WITH, EXPLAIN, or safe PRAGMA).",
    inputSchema: {
      type: "object",
      properties: {
        sql: { type: "string", description: "Read-only SQL to execute." },
      },
      required: ["sql"],
      additionalProperties: false,
    },
  },
];

function send(message) {
  process.stdout.write(`${JSON.stringify(message)}\n`);
}

function textResult(text, isError = false) {
  return { content: [{ type: "text", text }], isError };
}

function assertReadOnlySql(sql) {
  const trimmed = sql.trim();
  if (!trimmed) {
    throw new Error("SQL is empty.");
  }
  if (trimmed.includes(";")) {
    const withoutTrailing = trimmed.replace(/;\s*$/, "");
    if (withoutTrailing.includes(";")) {
      throw new Error("Multiple SQL statements are not allowed.");
    }
  }
  const stripped = withoutTrailingComments(trimmed);
  const match = stripped.match(/^([A-Za-z_]+)/);
  const keyword = match?.[1]?.toUpperCase();
  if (keyword === "SELECT" || keyword === "WITH" || keyword === "EXPLAIN") {
    return;
  }
  if (keyword === "PRAGMA") {
    const pragma = stripped.replace(/^PRAGMA\s+/i, "").split(/[\s(=]/, 1)[0].toLowerCase();
    const allowed = new Set([
      "table_info",
      "table_xinfo",
      "index_list",
      "index_info",
      "foreign_key_list",
      "table_list",
      "database_list",
      "compile_options",
      "integrity_check",
      "quick_check",
      "foreign_key_check",
    ]);
    if (!allowed.has(pragma)) {
      throw new Error(`PRAGMA ${pragma} is not allowed in read-only mode.`);
    }
    return;
  }
  throw new Error(`Only SELECT, WITH, EXPLAIN, or safe PRAGMA are allowed. Got: ${keyword ?? "unknown"}`);
}

function withoutTrailingComments(sql) {
  return sql
    .replace(/\/\*[\s\S]*?\*\//g, " ")
    .replace(/--[^\n]*/g, " ")
    .trim();
}

function listTables() {
  return db
    .prepare(
      "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name"
    )
    .all()
    .map((row) => row.name);
}

function tableSchema(tableName) {
  const columns = db.prepare(`PRAGMA table_info(${quoteIdent(tableName)})`).all();
  const foreignKeys = db.prepare(`PRAGMA foreign_key_list(${quoteIdent(tableName)})`).all();
  const indexes = db.prepare(`PRAGMA index_list(${quoteIdent(tableName)})`).all();
  const rowCount = db.prepare(`SELECT COUNT(*) AS n FROM ${quoteIdent(tableName)}`).get().n;
  return { table: tableName, columns, foreignKeys, indexes, rowCount };
}

function quoteIdent(name) {
  if (!/^[A-Za-z_][A-Za-z0-9_]*$/.test(name)) {
    throw new Error(`Invalid identifier: ${name}`);
  }
  return `"${name}"`;
}

function handleTool(name, args = {}) {
  if (name === "list_tables") {
    return textResult(
      JSON.stringify({ database: dbPath, tables: listTables() }, null, 2)
    );
  }
  if (name === "schema") {
    const report = {
      database: dbPath,
      readOnly: true,
      tables: listTables().map(tableSchema),
    };
    return textResult(JSON.stringify(report, null, 2));
  }
  if (name === "query") {
    const sql = String(args.sql ?? "");
    assertReadOnlySql(sql);
    const rows = db.prepare(sql).all();
    return textResult(JSON.stringify({ database: dbPath, rowCount: rows.length, rows }, null, 2));
  }
  throw new Error(`Unknown tool: ${name}`);
}

function handleRequest(message) {
  const { id, method, params } = message;
  if (method === "initialize") {
    send({
      jsonrpc: "2.0",
      id,
      result: {
        protocolVersion: params?.protocolVersion ?? "2024-11-05",
        capabilities: { tools: {} },
        serverInfo: { name: "sqlite-readonly", version: "1.0.0" },
      },
    });
    return;
  }
  if (method === "ping") {
    send({ jsonrpc: "2.0", id, result: {} });
    return;
  }
  if (method === "tools/list") {
    send({ jsonrpc: "2.0", id, result: { tools } });
    return;
  }
  if (method === "resources/list") {
    send({ jsonrpc: "2.0", id, result: { resources: [] } });
    return;
  }
  if (method === "prompts/list") {
    send({ jsonrpc: "2.0", id, result: { prompts: [] } });
    return;
  }
  if (method === "tools/call") {
    try {
      send({
        jsonrpc: "2.0",
        id,
        result: handleTool(params?.name, params?.arguments ?? {}),
      });
    } catch (error) {
      send({
        jsonrpc: "2.0",
        id,
        result: textResult(error instanceof Error ? error.message : String(error), true),
      });
    }
    return;
  }
  if (typeof id !== "undefined") {
    send({
      jsonrpc: "2.0",
      id,
      error: { code: -32601, message: `Method not found: ${method}` },
    });
  }
}

const rl = createInterface({ input: process.stdin, crlfDelay: Infinity });
rl.on("line", (line) => {
  const trimmed = line.trim();
  if (!trimmed) {
    return;
  }
  let message;
  try {
    message = JSON.parse(trimmed);
  } catch {
    return;
  }
  if (message.method?.startsWith("notifications/")) {
    return;
  }
  if (message.method) {
    handleRequest(message);
  }
});

process.stderr.write(`mcp-sqlite-readonly: opened ${dbPath} (read-only)\n`);
