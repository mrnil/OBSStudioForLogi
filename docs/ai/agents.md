# Claude Subagents

This repo defines five Claude Code subagents in `.claude/agents/`. Each one knows this project's rules for a narrow job, so it doesn't start from scratch the way a generic agent does. Claude Code picks one when a request matches its description. You can also name it directly, for example "use docs-sync on this branch".

Other AI tools don't load these files, but they are plain Markdown instructions. Read the agent's file and follow it by hand if your tool has no subagents.

## The Agents

| Agent | Edits files | Use it to |
|-------|-------------|-----------|
| `pr-checker` | No | Triage open GitHub PRs: CI, mergeability, Dependabot bump risk (patch bumps auto-merge, minor and major don't), and the repo's conventions. Never merges, approves, comments or closes. |
| `docs-sync` | No | Before committing, check a diff for the docs that should change with it (`assessment.md`, `TODO.md`, `test-coverage.md` counts, `structure.md`, CHANGELOG, ...). Reports paste-ready fixes. |
| `sdk-researcher` | No | Answer Logi Actions SDK questions from the repo notes and the official `llms.txt` docs, marking each answer as documented, device-verified or inferred. |
| `obs-protocol` | No | Answer OBS WebSocket and obs-websocket-dotnet questions against the library version the plugin uses, from the local clone at `B:\development\obs-websocket-dotnet` and the protocol spec. |
| `icon-designer` | Yes | Add or change button icons and action symbols through `tools/icons/`, following `icon-style.md`. Doesn't commit. |

## Suggested Workflow

1. Plan the change in the main session, and name the files it will touch.
2. Implement it, test first, in the main session or a subagent working in its own git worktree.
3. Review the diff with `/code-review` and `docs-sync`. Use a different agent from the one that wrote the code.
4. Build with the plugin link on, check on a device, and commit.

Steps 2 and 3 can run for several backlog items at once if their files don't overlap. Device checks stay with a person, one item at a time.

## Building From an Agent

Every build of `src/OBSStudioForLogiPlugin.csproj` writes the Logi Plugin Service's `.link` file to point at that build's `bin/` and reloads the plugin. A build in another worktree therefore takes over the device. Agents that build or test pass a scratch folder instead:

```bash
dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj -p:PluginDir=<scratch-dir>/pluginlink/
```

The build still asks the service to reload the plugin, which is harmless. Run tests in one worktree at a time: the fire-and-forget timing flakiness noted in `test-coverage.md` gets worse under CPU load.

## Changing an Agent

Agent files use Claude Code's subagent format: YAML frontmatter (`name`, `description`, `tools`) and a Markdown body. The `description` decides when Claude Code picks the agent, so keep it specific. Keep read-only agents read-only by leaving `Edit` and `Write` out of `tools`. Agents point at the `docs/ai/` files rather than copying their rules, so a rule only has to change in one place. When a doc an agent relies on is renamed or restructured, update the agent in the same commit.
