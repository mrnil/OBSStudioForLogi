# Release Process

## Overview

This document describes the complete process for creating and publishing a new release of the OBSStudioForLogiPlugin.

## Prerequisites

- All code changes committed to git
- All tests passing
- Documentation updated (README, CHANGELOG, memory bank)

## Release Steps

### 1. Update Version Number

Update version in `src/package/metadata/LoupedeckPackage.yaml`:

```yaml
version: "0.9.1"  # Update to new version
```

### 2. Build Release Configuration

```bash
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release
```

This will:

- Build the DLL to `bin/Release/bin/OBSStudioForLogiPlugin.dll`
- Copy all dependencies
- Copy metadata files
- Create plugin link file
- Trigger hot-reload

### 3. Create Release Notes

Create `RELEASE_NOTES_v{VERSION}.md` with:

- Bug fixes
- New features
- UI/UX improvements
- Technical details
- Installation instructions
- Requirements
- Acknowledgments

### 4. Commit Release Notes

```bash
git add RELEASE_NOTES_v{VERSION}.md
git commit -m "docs: add release notes for v{VERSION}"
```

### 5. Create Git Tag

```bash
git tag -a v{VERSION} -m "Release v{VERSION} - {TITLE}

{SUMMARY OF CHANGES}"
```

### 6. Create .lplug4 Package

Use LogiPluginTool (installed as dotnet global tool):

```bash
LogiPluginTool pack "b:\development\OBSStudioForLogiPlugin\bin\Release" "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

**Note:** LogiPluginTool is available as:
- dotnet global tool: `C:\Users\steph\.dotnet\tools\logiplugintool.exe` (preferred, callable as `LogiPluginTool` from any directory)
- SDK tools copy: `B:\development\LogitechBeta\LogiPluginSdkTools\LogiPluginTool.exe`
- ❌ `C:\Program Files\Logi\LogiPluginService\LogiPluginTool.exe` — BROKEN (missing companion DLL, do NOT use)

### 7. Verify Package

```bash
LogiPluginTool verify "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

### 8. Check Package Metadata

```bash
LogiPluginTool metadata "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v{VERSION}.lplug4"
```

Verify:

- Version number is correct
- Display name is correct
- Supported platforms are correct

### 9. Push to Git

```bash
git push origin main
git push origin v{VERSION}
```

### 10. Create GitHub Release

1. Go to GitHub repository
2. Click "Releases" → "Draft a new release"
3. Select the tag `v{VERSION}`
4. Set release title: `v{VERSION} - {TITLE}`
5. Copy content from `RELEASE_NOTES_v{VERSION}.md`
6. Attach the `.lplug4` file
7. Mark as pre-release if applicable
8. Publish release

## LogiPluginTool Commands Reference

### Pack Plugin

```bash
LogiPluginTool pack <InputDirectoryPath> <OutputPackagePath>
```

- `<InputDirectoryPath>`: Path to `bin/Release` directory
- `<OutputPackagePath>`: Path where `.lplug4` file will be created

### Verify Plugin

```bash
LogiPluginTool verify <PackagePath>
```

- Unpacks and verifies the plugin package
- Checks for errors and missing files

### Show Metadata

```bash
LogiPluginTool metadata <PackagePath>
```

- Displays plugin metadata in JSON format
- Shows version, name, supported platforms, etc.

### Install Plugin

```bash
LogiPluginTool install <PackagePath>
```

- Installs plugin to Logi Plugin Service
- Useful for testing before distribution

### Uninstall Plugin

```bash
LogiPluginTool uninstall <PluginName>
```

- Removes plugin from Logi Plugin Service

## Version Numbering

Follow Semantic Versioning (SemVer):

- **Major** (X.0.0): Breaking changes, major new features
- **Minor** (0.X.0): New features, backward compatible
- **Patch** (0.0.X): Bug fixes, backward compatible

Examples:

- `v0.9.0`: Added audio volume display (minor feature)
- `v0.9.1`: Fixed icon update bugs (patch)
- `v1.0.0`: First stable release (major)

## Release Types

### Patch Release (0.0.X)

- Bug fixes only
- No new features
- Backward compatible
- Example: v0.9.1

### Minor Release (0.X.0)

- New features
- Bug fixes
- Backward compatible
- Example: v0.9.0

### Major Release (X.0.0)

- Breaking changes
- Major new features
- May not be backward compatible
- Example: v1.0.0

## Checklist

Before creating a release, verify:

- [ ] All code changes committed
- [ ] All tests passing
- [ ] Version number updated in `LoupedeckPackage.yaml`
- [ ] README.md updated with new features
- [ ] CHANGELOG.md updated
- [ ] TODO.md updated
- [ ] Memory bank documentation updated
- [ ] Release notes created
- [ ] Git tag created
- [ ] .lplug4 package created
- [ ] Package verified
- [ ] Package metadata checked
- [ ] Git pushed (commits and tags)
- [ ] GitHub release created
- [ ] .lplug4 file attached to release

## Post-Release

After publishing:

1. Announce release on relevant channels
2. Update any external documentation
3. Monitor for bug reports
4. Plan next release based on feedback

## Troubleshooting

### Package Creation Fails

- Ensure Release build completed successfully
- Check that `bin/Release` directory exists
- Verify all dependencies are present
- Check LogiPluginTool output for errors

### Package Verification Fails

- Check package file is not corrupted
- Ensure all required files are in `bin/Release`
- Verify metadata files are present in `metadata/` folder

### Version Mismatch

- Update version in `LoupedeckPackage.yaml`
- Rebuild Release configuration
- Recreate .lplug4 package

## File Locations

- **Source**: `src/OBSStudioForLogiPlugin.csproj`
- **Build Output**: `bin/Release/`
- **Metadata**: `src/package/metadata/LoupedeckPackage.yaml`
- **Release Package**: `OBSStudioForLogiPlugin-v{VERSION}.lplug4`
- **Release Notes**: `RELEASE_NOTES_v{VERSION}.md`
- **LogiPluginTool**: `C:\Users\steph\.dotnet\tools\logiplugintool.exe` (dotnet global tool, callable as `LogiPluginTool` from any directory)
- **LogiPluginTool (alt)**: `B:\development\LogitechBeta\LogiPluginSdkTools\LogiPluginTool.exe`
- ❌ **NOT**: `C:\Program Files\Logi\LogiPluginService\LogiPluginTool.exe` (broken, missing DLL)

## Example Release Commands

Complete release for v0.9.1:

```bash
# 1. Update version in LoupedeckPackage.yaml (manual)

# 2. Build
dotnet build src/OBSStudioForLogiPlugin.csproj -c Release

# 3. Create release notes (manual)

# 4. Commit release notes
git add RELEASE_NOTES_v0.9.1.md
git commit -m "docs: add release notes for v0.9.1"

# 5. Create tag
git tag -a v0.9.1 -m "Release v0.9.1 - Bug Fixes and Icon Updates"

# 6. Create package (LogiPluginTool is a dotnet global tool, callable from any directory)
LogiPluginTool pack "b:\development\OBSStudioForLogiPlugin\bin\Release" "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v0.9.1.lplug4"

# 7. Verify package
LogiPluginTool verify "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v0.9.1.lplug4"

# 8. Check metadata
LogiPluginTool metadata "b:\development\OBSStudioForLogiPlugin\OBSStudioForLogiPlugin-v0.9.1.lplug4"

# 9. Push to git
git push origin main
git push origin v0.9.1

# 10. Create GitHub release (manual via web interface)
```
