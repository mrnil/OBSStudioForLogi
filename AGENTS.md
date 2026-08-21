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

## Non-negotiables

- Follow `docs/ai/guidelines.md` exactly for code style — this project's `.editorconfig` enforces BCL type names (`String`, not `string`), no `var`, mandatory `this.` qualification, and Allman braces as warnings, but the convention is followed strictly regardless.
- All new business logic needs tests (see `docs/ai/guidelines.md` TDD Scope and `docs/ai/test-coverage.md`). The `src/Actions/` (Loupedeck SDK) layer is exempt from strict coverage targets — see the same section for why.
- Commits follow Conventional Commits and must explain **why**, not just what — see `docs/ai/commit-conventions.md`.
- Tests run locally only; CI (`.github/workflows/dependency-check.yml`) does not run them — do not add them to CI without first fixing the fire-and-forget timing flakiness noted in `docs/ai/test-coverage.md`.
- When you fix something tracked in `docs/ai/assessment.md` or `TODO.md`, update those files in the same change — they drift fast otherwise (this has already happened once).

## Keeping this current

`docs/ai/*.md` is maintained like code, not written once and forgotten. When you change architecture, dependencies, versions, or fix a tracked issue, update the relevant file(s) in the same commit or PR.
