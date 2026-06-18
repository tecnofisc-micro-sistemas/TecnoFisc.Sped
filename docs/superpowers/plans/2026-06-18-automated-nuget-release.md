# Automated NuGet Release Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Publish the NuGet package family automatically after a PR is merged into `main`.

**Architecture:** Keep release versioning explicit in `Directory.Build.props` and release notes explicit in docs. Move the automated publication boundary to `push` on `main`: CI validates PRs, and the release workflow builds, tests, packs, validates that the version is not already published, pushes to nuget.org, creates `vX.Y.Z`, and creates the GitHub Release.

**Tech Stack:** GitHub Actions, .NET SDK from `global.json`, NuGet v3 flat container API, `dotnet nuget push`, `softprops/action-gh-release`.

---

### Task 1: Update Release Workflow

**Files:**
- Modify: `.github/workflows/release.yml`

- [ ] **Step 1: Change triggers**

Use `push` to `main` and keep `workflow_dispatch` for dry runs. Remove `push` tag trigger so auto-created tags do not recursively publish.

- [ ] **Step 2: Add version and duplicate validations**

Read `<Version>` from `Directory.Build.props`, validate that `v$VERSION` does not already exist, and after packing validate that every generated `.nupkg` version does not already exist in nuget.org.

- [ ] **Step 3: Publish and create release artifacts**

Publish `.nupkg` files only on `push` to `main` or manual dispatch with `dry_run=false`. Create the git tag and GitHub Release only after NuGet push succeeds.

### Task 2: Update CI Workflow

**Files:**
- Modify: `.github/workflows/ci.yml`

- [ ] **Step 1: Keep CI focused on validation**

Remove the `pack` job from CI because the release workflow now owns packing and publishing on `main`.

### Task 3: Update Documentation

**Files:**
- Modify: `README.md`
- Modify: `CLAUDE.md`
- Modify: `CHANGELOG.md`

- [ ] **Step 1: Document the new release flow**

Document that PRs to `main` must contain the version bump and release notes, and that merge to `main` publishes automatically.

- [ ] **Step 2: Add release note for the current unpublished change**

Add a `[Não publicado]` section for the new TXT sniffer metadata implementation currently on `dev`.

### Task 4: Verify

**Files:**
- Validate: `.github/workflows/release.yml`
- Validate: `.github/workflows/ci.yml`
- Validate: `TecnoFisc.Sped.slnx`

- [ ] **Step 1: Validate YAML parses**

Use Ruby/Psych if available, or a simple PowerShell parse fallback, to check YAML syntax.

- [ ] **Step 2: Run build/test/pack locally**

Run `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet pack` to validate the repository still produces packages.

- [ ] **Step 3: Inspect git diff**

Confirm only workflow/docs/plan files changed.
