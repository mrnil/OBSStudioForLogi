# AGENTS.md

Instructions for AI coding assistants working in this repository (Claude Code, Cursor, Copilot, Codex, etc).

## What this project is

A Logitech/Loupedeck hardware plugin (MX Creative Console, Loupedeck CT/Live) that controls OBS Studio via the OBS WebSocket 5.x protocol. C#/.NET 10.0. See `docs/ai/product.md` for the full feature overview.

## Start here

Read these before making changes — they are kept current and are the source of truth for this project (not a generic template):

| File | Read this for |
|------|----------------|
| [`docs/ai/product.md`](docs/ai/product.md) | What the plugin does, feature groups, target users |
| [`docs/ai/structure.md`](docs/ai/structure.md) | Repo layout, architectural patterns, layering |
| [`docs/ai/tech.md`](docs/ai/tech.md) | Stack, dependencies, build/test/package commands |
| [`docs/ai/guidelines.md`](docs/ai/guidelines.md) | Code style, service/command layer patterns, testing patterns — **read before writing code** |
| [`docs/ai/secure-coding.md`](docs/ai/secure-coding.md) | Security principles (input validation, credentials, memory, comms) |
| [`docs/ai/commit-conventions.md`](docs/ai/commit-conventions.md) | Commit message rules |
| [`docs/ai/test-coverage.md`](docs/ai/test-coverage.md) | Test architecture, coverage targets, running tests |
| [`docs/ai/assessment.md`](docs/ai/assessment.md) | Known issues / findings backlog, prioritised |
| [`docs/ai/release-process.md`](docs/ai/release-process.md) | Full release checklist |

Deeper reference material (read only when relevant to the task at hand):

- `sdk-quick-reference.md`, `adjustable-command-pattern.md`, `icon-update-patterns.md`, `image-rendering-simplified.md` — Loupedeck SDK command patterns
- `obs-websocket-api-complete.md`, `obs-audio-api-analysis.md`, `protocol-gap-analysis.md` — OBS WebSocket protocol coverage
- `refactoring-patterns.md`, `vu-meters-learnings.md`, `multi-instance-obs-design.md` — past decisions and design notes

## When SDK or plugin knowledge is missing

The Logi Actions SDK (the SDK this plugin is built on — `PluginApi.dll`, `Plugin`/`PluginDynamicCommand`/`PluginDynamicFolder`/`ActionEditorCommand` base classes, etc.) has an AI-friendly documentation index at **<https://logitech.github.io/actions-sdk-docs/llms.txt>**. It's an `llms.txt`-format index linking out to the real docs pages — C# plugin development, haptics, icons, plugin features (logging/settings/localization/multistate actions), supported devices, distribution guidelines.

If you hit a question about SDK behavior that isn't answered by `docs/ai/sdk-quick-reference.md` or the other reference docs above — an API you haven't used before, an edge case in lifecycle/rendering behavior, a class or method whose exact contract you're unsure of — **fetch and consult this index rather than guessing or inferring behavior from the SDK's shape**. `PluginApi.dll` is a compiled reference-only dependency in this repo (see `docs/ai/tech.md`), so you cannot read its source to verify assumptions; the official docs are the only reliable source of truth, and guessing wrong here produces plugin code that compiles but misbehaves at runtime in ways tests can't catch (the SDK-dependent `src/Actions/` layer is exempt from strict TDD — see `docs/ai/test-coverage.md`).

## Non-negotiables

- Follow `docs/ai/guidelines.md` exactly for code style — this project's `.editorconfig` enforces BCL type names (`String`, not `string`), no `var`, mandatory `this.` qualification, and Allman braces as warnings, but the convention is followed strictly regardless.
- All new business logic needs tests (see `docs/ai/guidelines.md` TDD Scope and `docs/ai/test-coverage.md`). The `src/Actions/` (Loupedeck SDK) layer is exempt from strict coverage targets — see the same section for why.
- Commits follow Conventional Commits and must explain **why**, not just what — see `docs/ai/commit-conventions.md`.
- Tests run locally only; CI (`.github/workflows/dependency-check.yml`) does not run them — do not add them to CI without first fixing the fire-and-forget timing flakiness noted in `docs/ai/test-coverage.md`.
- When you fix something tracked in `docs/ai/assessment.md` or `TODO.md`, update those files in the same change — they drift fast otherwise (this has already happened once).
- Every `.md` file you write or edit must pass `rumdl` (config: `.markdownlint.json`, enforced in CI by `.github/workflows/markdown-lint.yml`). The rules that actually get hit in practice:
  - Blank line before **and** after every heading, list block, and fenced code block (MD022/MD032/MD031).
  - No bare URLs — use `[text](url)` or wrap in `<angle brackets>` (MD034).
  - Wrap C#/generic syntax like `` `Func<OBSStats>` `` or `` `List<T>` `` in backticks when it appears in prose outside a code fence — unescaped `<...>` reads as an HTML tag (MD033).
  - Ordered lists must increment (`1.`, `2.`, `3.` — not `1.` repeated) (MD029).
  - No trailing whitespace, except exactly two trailing spaces for an intentional Markdown hard line-break inside a paragraph (MD009).
- Every GitHub Actions workflow you write or edit must be least-privilege on `GITHUB_TOKEN`:
  - Always declare an explicit top-level `permissions:` block — never rely on the repo/org default, which may be more permissive than this workflow needs.
  - Default to `contents: read`. Only add a broader scope (`pull-requests: write`, `contents: write`, etc.) to the specific job that actually needs it, not to the whole workflow — job-level `permissions:` fully replaces the workflow-level block for that job, it doesn't add to it, so a job with no block inherits everything from the top level.
  - Before adding a scope, check whether the action you're calling actually needs it — e.g. GitHub's native PR annotations (inline diagnostics in the "Files changed" tab) work off the job's log output and need no extra permission; only actions that post comments, reviews, labels, or merges need `pull-requests: write`.

## Keeping this current

`docs/ai/*.md` is maintained like code, not written once and forgotten. When you change architecture, dependencies, versions, or fix a tracked issue, update the relevant file(s) in the same commit or PR.
