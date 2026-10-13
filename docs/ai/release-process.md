# Release Process

How to build, package and publish a release. Paths assume the repo is at `b:\development\OBSStudioForLogiPlugin`.

## Prerequisites

- All changes committed, and all tests passing (`dotnet test tests/OBSStudioForLogiPlugin.Tests/OBSStudioForLogiPlugin.Tests.csproj`)
- Any device checks the release depends on are done (see "Device Checks Outstanding" in `TODO.md`)
- `README.md`, `USER_MANUAL.md` and `docs/ai/` describe the new behaviour

## Steps

### 1. Update the Version

Update the version in **both** places (SemVer: major for breaking changes, minor for features, patch for fixes only):

- `src/package/metadata/LoupedeckPackage.yaml`: `version:`
- `src/OBSStudioForLogiPlugin.csproj`: `<Version>`, `<AssemblyVersion>` and `<FileVersion>`

### 2. Update CHANGELOG.md

Turn the `## [Unreleased]` section into `## [{VERSION}] - {DATE}`, and start a new, empty `## [Unreleased]` above it. Use [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) sections (`### Added`, `### Changed`, `### Fixed`, `### Removed`, `### Security`). Every release needs a changelog entry: release notes don't replace it.

### 3. Write Release Notes

Create `RELEASE_NOTES_v{VERSION}.md` in the repo root, written for users: what's new, what's fixed, anything they need to do (for example re-entering a setting), and the requirements. `RELEASE_NOTES_v2.0.1.md` is a good model.

### 4. Commit and Tag

```bash
git add src/package/metadata/LoupedeckPackage.yaml src/OBSStudioForLogiPlugin.csproj CHANGELOG.md RELEASE_NOTES_v{VERSION}.md
git commit -m "docs: add release notes and changelog entry for v{VERSION}"
git tag -a v{VERSION} -m "Release v{VERSION} - {TITLE}"
```

### 5. Build Release

```bash
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
```

This builds into `bin/Release/`, copies `src/package/` into it, writes the plugin `.link` file and reloads the plugin.

The build copies package files but never deletes them, so a file removed from `src/package/` stays in `bin/Release/` until you delete it there. Check `bin/Release/metadata/` holds only `Icon256x256.png` and `LoupedeckPackage.yaml`.

### 6. Package, Verify and Check Metadata

```bash
LogiPluginTool pack "b:\development\OBSStudioForLogiPlugin\bin\Release" "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
LogiPluginTool verify "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
LogiPluginTool metadata "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

Use the dotnet global tool (`C:\Users\steph\.dotnet\tools\logiplugintool.exe`), or the SDK tools copy at `B:\development\LogitechBeta\LogiPluginSdkTools\LogiPluginTool.exe`. Don't use `C:\Program Files\Logi\LogiPluginService\LogiPluginTool.exe`: it is missing a companion DLL.

In the metadata output, check:

- The version, display name and supported platforms
- The package root has `actionsymbols/` with one SVG per action class (`ActionSymbolTests` checks the source folder)

`LogiPluginTool install <package>` installs the package locally, which is a useful last check before publishing.

### 7. Push

```bash
git push origin main
git push origin v{VERSION}
```

### 8. Create the GitHub Release

1. On GitHub, open **Releases** → **Draft a new release**
2. Select the tag `v{VERSION}` and set the title to `v{VERSION} - {TITLE}`
3. Paste the content of `RELEASE_NOTES_v{VERSION}.md`
4. Attach the `.lplug4` file
5. Mark it as a pre-release if applicable, then publish

## Checklist

- [ ] Tests pass and outstanding device checks are done
- [ ] Version updated in `LoupedeckPackage.yaml` **and** the `.csproj`
- [ ] `CHANGELOG.md` entry, with a new empty `[Unreleased]` section
- [ ] `RELEASE_NOTES_v{VERSION}.md`
- [ ] `README.md`, `USER_MANUAL.md`, `TODO.md` and `docs/ai/` up to date
- [ ] Commit and tag
- [ ] Release build, package created, verified and metadata checked
- [ ] Commits and tag pushed
- [ ] GitHub release published with the `.lplug4` attached

## Troubleshooting

- **Pack fails**: make sure the Release build succeeded and `bin/Release/` has `bin/` and `metadata/`.
- **Verify fails**: check that `metadata/LoupedeckPackage.yaml` and `metadata/Icon256x256.png` are present, and that no stale files are left in `bin/Release/`.
- **Wrong version in the package**: update the YAML, rebuild Release, and pack again.
