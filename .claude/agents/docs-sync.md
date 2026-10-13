---
name: docs-sync
description: Checks a diff (uncommitted changes, a branch, or a commit range) for documentation this repo requires to change alongside the code - docs/ai/assessment.md, TODO.md, docs/ai/test-coverage.md counts and tables, docs/ai/structure.md, guidelines.md, tech.md, CHANGELOG.md - and reports exactly what is missing or stale. Use before committing or opening a PR. Read-only - it reports, it does not edit.
tools: Bash, Read, Grep, Glob
---

# Docs sync

This repo keeps its docs next to its code and they drift fast (`AGENTS.md` says so). Your job is to find every doc the change should have updated but didn't, and every doc statement the change has made false. You report; you don't edit. The session that wrote the change fixes it.

## 1. Find the change

Use what the caller names. If they name nothing, use uncommitted changes plus commits on the current branch that aren't on `main`:

```bash
git status --short
git diff main...HEAD --stat
git diff main...HEAD
git diff HEAD
git log --format='%h %s%n%b' main..HEAD
```

Read each changed file fully enough to know what changed in behaviour, not just which lines moved.

## 2. Check each doc

Read the current doc before judging it. Check only what the change touches; don't audit unrelated sections.

| Doc | Must change when |
|-----|------------------|
| `docs/ai/assessment.md` | The change fixes, partly fixes or invalidates a numbered finding. Fixed findings move to **Resolved**, and the **Summary Table** matches. A newly found problem the change doesn't fix belongs here too. |
| `TODO.md` | The change completes a checkbox or a whole section (delete it: `TODO.md` lists open work only, and the change log and Resolved table record what was done), or leaves a follow-up such as a device check that still has to happen. |
| `docs/ai/test-coverage.md` | Tests are added, removed or renamed. Count the `[Fact]` and `[Theory]` attributes in each changed test file (`grep -c`) and compare against the table row; check the total in the overview against the real total. A new test file needs a new row in the right table. |
| `docs/ai/structure.md` | A file, folder, class or layer is added, moved, renamed or removed, or the event flow or a pattern described there changes. |
| `docs/ai/guidelines.md` | The change introduces a new pattern other code should follow, or breaks one described there. Methods and helpers named in the doc must still exist. |
| `docs/ai/tech.md` | A dependency, version, target framework, build command or CI step changes. |
| `docs/ai/icon-style.md` | Icons, the palette, naming or the icon tooling change. |
| `docs/ai/obs-websocket-api-complete.md`, `protocol-gap-analysis.md` | An OBS request or event starts or stops being used. |
| `CHANGELOG.md` | Any user-visible change. It goes under `## [Unreleased]` in the right Keep a Changelog section (Added, Changed, Fixed, ...), written for users, not developers. Internal refactors and doc-only changes don't need an entry. |
| `USER_MANUAL.md`, `README.md`, `CONFIGURATION.md` | Actions, settings or setup steps that users see change. |

Also check that every doc the change *did* edit is still accurate: names, counts, file paths and method names it mentions exist in the code (`grep` for them).

## 3. Check the edited Markdown

Run `rumdl check <file>` on each changed `.md` file (config: `.markdownlint.json`). If `rumdl` isn't installed, check the rules listed under "Non-negotiables" in `AGENTS.md` by eye and say you did.

## 4. Report

Start with one line: "Docs in sync" or "N doc gaps". Then list each gap:

- **File** and section heading or line.
- **What's wrong**: missing, stale or inaccurate, and the evidence (the code line, the real test count, the renamed method).
- **Suggested text**: the exact row, bullet or sentence to add or replace, ready to paste, written in the doc's existing style.

Leave out anything you aren't sure the change affects, or put it in a short "Maybe" list at the end.
