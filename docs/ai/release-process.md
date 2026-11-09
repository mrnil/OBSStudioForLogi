# Release Process

How to prepare, build and publish a release. Pushing a version tag runs `.github/workflows/release.yml`, which builds, tests and packages the plugin on GitHub and publishes the GitHub release, titled from the release notes, with the notes and the `.lplug4` attached. Nothing needs building or packaging locally.

## Prerequisites

- All changes are on `main`, and the latest **Build and Test** run on `main` passed
- Any device checks the release depends on are done (see "Device Checks Outstanding" in `TODO.md`)
- `README.md`, `USER_MANUAL.md` and `docs/ai/` describe the new behaviour

## Steps

### 1. Update the Version

Update the version in **both** places (SemVer: major for breaking changes, minor for features, patch for fixes only):

- `src/package/metadata/LoupedeckPackage.yaml`: `version:`
- `src/OBSStudioForLogiPlugin.csproj`: `<Version>`, `<AssemblyVersion>` and `<FileVersion>`

The release workflow stops if either doesn't match the tag.

### 2. Update CHANGELOG.md

Turn the `## [Unreleased]` section into `## [{VERSION}] - {DATE}`, and start a new, empty `## [Unreleased]` above it. Use [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) sections (`### Added`, `### Changed`, `### Fixed`, `### Removed`, `### Security`). Every release needs a changelog entry: release notes don't replace it. The release workflow stops if there is no `## [{VERSION}]` heading.

### 3. Write Release Notes

Create `RELEASE_NOTES_v{VERSION}.md` in the repo root, written for users: what's new, what's fixed, anything they need to do (for example re-entering a setting), and the requirements. `RELEASE_NOTES_v2.0.1.md` is a good model.

The first line is the release title, and must be exactly:

```markdown
# v{VERSION} - {TITLE}
```

with `{TITLE}` replaced by a short title, for example `# v2.1.0 - Faster taps and clearer buttons`. The release workflow uses that line as the GitHub release title, and the rest of the file as the release body. It stops if the file is missing, if the first line isn't in that form, or if the title still contains `{` or `}`.

### 4. Commit and Push

```bash
git add src/package/metadata/LoupedeckPackage.yaml src/OBSStudioForLogiPlugin.csproj CHANGELOG.md RELEASE_NOTES_v{VERSION}.md
git commit -m "docs: add release notes and changelog entry for v{VERSION}"
git push origin main
```

Wait for the CI runs on that commit (Build and Test, Dependency Check & Build, CodeQL and, if Markdown changed, Markdown Lint) to pass before tagging. The release workflow refuses to release a commit whose checks failed, and waits for any still running.

Tag the commit you pushed last: CI only runs on the last commit of a push, so an earlier commit from the same push has no checks and the release stops.

### 5. Tag and Push the Tag

```bash
git tag -a v{VERSION} -m "Release v{VERSION}"
git push origin v{VERSION}
```

The tag message isn't used: the title comes from the release notes (step 3).

Pushing the tag starts the **Release** workflow. It:

1. Checks that the tagged commit is on `main` and that every check on it passed (job `ci-passed`). It needs `test-and-package`, `build` and `Analyze (csharp)` to be present, waits up to 30 minutes for checks still running, and stops if any check failed or was cancelled. Nothing below runs unless this passes
2. Checks the two version numbers, the changelog entry, and the release notes file and its title line
3. Builds in Release and runs the tests
4. Packs `OBSStudioForLogiPlugin-v{VERSION}.lplug4`, verifies it and prints its metadata
5. Publishes the GitHub release for the tag, titled from the notes' first line, with the rest of the notes as the body and the package attached. It is marked **Latest**; a version with a suffix, such as `v2.2.0-beta.1`, is published as a pre-release instead and doesn't become Latest

### 6. Check the Published Release

1. Open the Release run under **Actions** and read the `Package, verify and show metadata` step: check the version, display name and supported platforms, and that the package has `actionsymbols/` with one SVG per action class
2. Open **Releases** and read the notes as they render
3. Optionally download the `.lplug4` and run `LogiPluginTool install <package>` for a last check on a device

## Fixing a Failed or Wrong Release

The release is created only after every check, the tests and the package verification pass, so a failed run leaves nothing to clean up on the Releases page. Fix the problem, then move the tag to the new commit:

```bash
git push origin :refs/tags/v{VERSION}   # delete the remote tag
git tag -d v{VERSION}
# commit and push the fix to main, then repeat step 5
```

If the run succeeded but the notes or title need a fix, edit the release on GitHub (or with `gh release edit v{VERSION} --title ... --notes-file ...`), and commit the same change to `RELEASE_NOTES_v{VERSION}.md` so the repo matches. Don't move the tag of a published release: if the package itself is wrong, release a new patch version.

## Checklist

- [ ] Latest Build and Test run on `main` passed, and outstanding device checks are done
- [ ] Version updated in `LoupedeckPackage.yaml` **and** the `.csproj`
- [ ] `CHANGELOG.md` entry, with a new empty `[Unreleased]` section
- [ ] `RELEASE_NOTES_v{VERSION}.md`, first line `# v{VERSION} - {TITLE}` with a real title
- [ ] `README.md`, `USER_MANUAL.md`, `TODO.md` and `docs/ai/` up to date
- [ ] Committed and pushed, CI passed
- [ ] Tag `v{VERSION}` pushed
- [ ] Release workflow passed, and the published release's notes, metadata and package checked

## Building a Package Locally

Only needed to try a package before tagging, or if GitHub Actions is unavailable. Every push to `main` and every pull request also uploads an untagged package as an artifact of the **Build and Test** run.

```bash
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
LogiPluginTool pack "b:\development\OBSStudioForLogiPlugin\bin\Release" "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
LogiPluginTool verify "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
LogiPluginTool metadata "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

The local build writes the plugin `.link` file and reloads the plugin. It copies package files but never deletes them, so a file removed from `src/package/` stays in `bin/Release/` until you delete it there: check `bin/Release/metadata/` holds only `Icon256x256.png` and `LoupedeckPackage.yaml`. The release workflow always builds from a clean checkout, so this doesn't affect it.

Use the dotnet global tool (`C:\Users\steph\.dotnet\tools\logiplugintool.exe`), or the SDK tools copy at `B:\development\LogitechBeta\LogiPluginSdkTools\LogiPluginTool.exe`. Don't use `C:\Program Files\Logi\LogiPluginService\LogiPluginTool.exe`: it is missing a companion DLL.

## Troubleshooting

- **"The first line of RELEASE_NOTES ... should be" or "still has a placeholder"**: give the notes a first line of `# v{VERSION} - {TITLE}` with a real title, commit and push, then move the tag (see [Fixing a Failed or Wrong Release](#fixing-a-failed-or-wrong-release)).
- **"Tag ... doesn't match the csproj"**: one of the two version numbers in step 1 wasn't updated.
- **"CHANGELOG.md has no entry" or "RELEASE_NOTES ... is missing"**: the file wasn't committed, or its heading or name doesn't use the tag's version.
- **"a release with the same tag name already exists"**: a release for this tag was already made. Edit it rather than rerunning, or delete it first if it was never meant to be published.
- **Pack fails**: make sure the Release build succeeded and `bin/Release/` has `bin/` and `metadata/`.
- **Verify fails**: check that `metadata/LoupedeckPackage.yaml` and `metadata/Icon256x256.png` are present, and, locally, that no stale files are left in `bin/Release/`.
- **"isn't on main"**: the tag points at a commit that was never pushed to `main`. Push it, wait for CI, then move the tag.
- **"No test-and-package check"** (or `build`, `Analyze (csharp)`): CI never ran on the tagged commit, usually because it wasn't the last commit of a push. Move the tag to the last commit pushed. If a workflow or job was renamed, update `REQUIRED` in `release.yml`.
- **"Checks on ... did not pass"**: a CI run on the tagged commit failed or was cancelled. Rerun it if it was a flaky failure; otherwise fix it on `main` and move the tag. Then rerun the Release workflow.
- **Tests fail only on the runner**: `DoubleTapHelperTests` and `ConnectionManagerTests` wait on real timers and are the first to fail on a slow runner. Rerun the failed job; if it keeps failing, treat it as a real failure.
